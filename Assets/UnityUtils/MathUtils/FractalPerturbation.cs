using System;
using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ComplexDouble;
using static HighPrecision;
using UI_Utils;
using UnityEngine.EventSystems;

public class FractalPerturbation : MonoBehaviour
{
    [Header("Compute Shader")]
    public ComputeShader shader;

    [Header("Output")]
    public RenderTexture target;

    [Header("View")]
    public Vector2Int c0Pixel = new Vector2Int(-1, -1);
    public ComplexDouble c0; // world coords of reference orbit (double for orbit builder)

    // Fixed-point camera state (authoritative)
    BigComplex center = BigComplex.FromDecimalString("-0.717413872065201+0.208571862722845i");
    BigInteger centerReF = ToFixed(-0.717413872065201), centerImF = ToFixed(0.208571862722845); // fixed-point center
    BigInteger scaleF = ToFixed(1.0 / 1024.0);               // fixed-point pixel scale (world units per pixel)
    BigInteger c0ReF, c0ImF;         // fixed-point C0 (kept aligned with c0)
    bool c0FixedInitialized = false;

    [Header("Iterations")]
    public int maxIterations = 200;
    public float bailout = 4f;


    public int Width => (int)window.Width;
    public int Height => (int)window.Height;

    ComputeBuffer orbitBuffer;
    public RawImage image;
    UnityEngine.Vector2? lastMousePos;
    public UI_Window window;
    public bool debug = true;
    public float lightDirection = 0.0f;
    public float lightHeight = 2.0f;

    void OnEnable()
    {
        window.OnMouseDown.AddListener(OnMouseDown);
    }

    void OnDisable()
    {
        ReleaseBuffers();
        if (target != null) { target.Release(); target = null; }
    }

    void Update()
    {
        CreateOrResizeTarget();
        if(window.IsMouseOver){
            HandleInput();
        }
        BuildAndUploadOrbit();
        Dispatch();
        if (image) image.texture = target;
    }

    void OnMouseDown(PointerEventData eventData)
    {
        Debug.Log("Mouse down at " + eventData.position);
        if(eventData.button == PointerEventData.InputButton.Right){
            Vector2Int anchor = new Vector2Int(Mathf.RoundToInt(eventData.position.x), Mathf.RoundToInt(eventData.position.y));
            Debug.Log($"Before rebase: Center fixed: {ToDouble(centerReF)} {ToDouble(centerImF)}");
            HandleRebase(anchor, 8);
            UpdateShader();
            Debug.Log($"After rebase: Center fixed: {ToDouble(centerReF)} {ToDouble(centerImF)}");
        }
    }

    void HandleInput()
    {
        // Zoom: powers of two for exactness (use integer steps)
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            int steps = (int)Mathf.Round(scroll);
            scaleF = Zoom(scaleF, steps);
        }

        // Drag: integer pixel deltas
        if (Input.GetMouseButtonDown(0))
            lastMousePos = Input.mousePosition;
        else if (Input.GetMouseButton(0) && lastMousePos.HasValue)
        {
            UnityEngine.Vector2 cur = Input.mousePosition;
            UnityEngine.Vector2 d   = cur - lastMousePos.Value;
            lastMousePos = cur;

            centerReF = AddPixels(centerReF, -(long)Mathf.RoundToInt(d.x), scaleF);
            centerImF = AddPixels(centerImF, -(long)Mathf.RoundToInt(d.y), scaleF);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            lastMousePos = null;
        }

        if (Input.GetKey(KeyCode.W)) { centerImF = AddPixels(centerImF, +1, scaleF); }
        if (Input.GetKey(KeyCode.S)) { centerImF = AddPixels(centerImF, -1, scaleF); }
        if (Input.GetKey(KeyCode.A)) { centerReF = AddPixels(centerReF, -1, scaleF); }
        if (Input.GetKey(KeyCode.D)) { centerReF = AddPixels(centerReF, +1, scaleF); }
        if(Input.GetKeyDown(KeyCode.Space)){ debug = !debug; }
    }

    void CreateOrResizeTarget()
    {
        // Ensure width/height reflect the actual render target we’ll dispatch to
        if (target != null && (target.width != Width || target.height != Height))
        {
            target.Release();
            target = null;
        }

        if (target == null && Width > 0 && Height > 0)
        {
            target = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat);
            target.enableRandomWrite = true;
            target.Create();
        }
    }

    void ReleaseBuffers()
    {
        if (orbitBuffer != null)
        {
            orbitBuffer.Release();
            orbitBuffer = null;
        }
    }
    (List<ComplexDouble> orbit, List<ComplexDouble> derivativeOrbit) BuildReferenceOrbit(ComplexDouble c, int N)
    {
        List<ComplexDouble> orbit = new List<ComplexDouble>(N);
        List<ComplexDouble> derivativeOrbit = new List<ComplexDouble>(N);
        ComplexDouble z = new ComplexDouble(0.0, 0.0);
        ComplexDouble dZ = new ComplexDouble(0.0, 0.0);
        for (int i = 0; i < N; i++)
        {
            orbit.Add(z);
            derivativeOrbit.Add(dZ);
            z = z * z + c;
            dZ = 2 * z * dZ + ComplexDouble.ONE;
        }
        return (orbit, derivativeOrbit);
    }

    bool IsSafeReference(ComplexDouble c, int? maxIter= null, float? bailoutVal= null)
    {
        ComplexDouble z = new ComplexDouble(0.0, 0.0);
        if(maxIter == null) maxIter = maxIterations;
        if(bailoutVal == null) bailoutVal = bailout;
        for (int n = 0; n < maxIter + 10; n++)
        {
            z = z * z + c;
            if (z.squaredNorm() > bailoutVal) return false;
        }
        return true;
    }

    // Try to pick a safe reference. First search near lastRefPixel, then around center with growing radius.
    (bool found, int dx, int dy, ComplexDouble cRef) PickSafeReferencePixel(Vector2Int anchor, int maxRadius = 3)
    {
        Vector2Int diffPixels = new Vector2Int(anchor.x - Width/2, anchor.y - Height/2);
        
        for (int dy = -maxRadius + diffPixels.y; dy <= maxRadius + diffPixels.y; dy++)
        for (int dx = -maxRadius + diffPixels.x; dx <= maxRadius + diffPixels.x; dx++)
            {
                BigInteger candReF = AddPixels(centerReF, dx, scaleF);
                BigInteger candImF = AddPixels(centerImF, dy, scaleF);

                double candRe = ToDouble(candReF);
                double candIm = ToDouble(candImF);

                ComplexDouble candidate = new ComplexDouble(candRe, candIm);
                if (IsSafeReference(candidate))
                    return (true, dx, dy, candidate);
            }
        
        return (false, 0, 0, default(ComplexDouble));
    }

    void HandleRebase(Vector2Int? anchor = null, int maxRadius = 1){
        var pick = PickSafeReferencePixel(anchor ?? new Vector2Int(Width/2, Height/2), maxRadius);
        if (pick.found)
        {
            c0Pixel = new Vector2Int(Width / 2 + pick.dx, Height / 2 + pick.dy);
            c0 = pick.cRef;

            c0ReF = AddPixels(centerReF, pick.dx, scaleF);
            c0ImF = AddPixels(centerImF, pick.dy, scaleF);
            c0FixedInitialized = true;
            Debug.Log($"Rebase Successful: new C0 at pixel {c0Pixel.x}, {c0Pixel.y} with value {c0}");
        }
        else
        {
            Debug.LogWarning($"Rebase failed around center {ToDouble(centerReF)} {ToDouble(centerImF)} (scale {ToDouble(scaleF)}). keeping old C0: {c0}");
        }
    }

    void BuildAndUploadOrbit()
    {
        bool needNewRef = false;

        if (c0Pixel.x < 0) needNewRef = true;

        if (!needNewRef && !IsSafeReference(c0))
        {
            Debug.Log("C0 unsafe → rebase");
            needNewRef = true;
        }
        if (!needNewRef)
        {
            long dxPixels = OrthogonalPixelDistance(centerReF, c0ReF, scaleF);
            long dyPixels = OrthogonalPixelDistance(centerImF, c0ImF, scaleF);
            // squared distance (clamped) - choose threshold you want (tuneable)
            const long MAX_PIXELS = 120; // tune this
            long pix2 = dxPixels * dxPixels + dyPixels * dyPixels;
            if (pix2 > MAX_PIXELS * MAX_PIXELS)
            {
                Debug.Log($"C0 too far ({dxPixels},{dyPixels}) → rebase");
                needNewRef = true;
            }
        }
        

        if (needNewRef)
        {
            HandleRebase();
        }
        else
        {
            if (!c0FixedInitialized)
            {
                c0ReF = ToFixed(c0.re);
                c0ImF = ToFixed(c0.im);
                c0FixedInitialized = true;
            }
        }

        UpdateShader();
    }

    void UpdateShader(){
        ReleaseBuffers();

        var (orbit, derivativeOrbit) = BuildReferenceOrbit(c0, maxIterations);

        var packed = new UnityEngine.Vector4[orbit.Count * 2];
        for (int i = 0; i < orbit.Count; i++) {
            packed[i * 2] = orbit[i].AsVector4();
            packed[i * 2 + 1] = derivativeOrbit[i].AsVector4();
        }

        orbitBuffer = new ComputeBuffer(packed.Length, sizeof(float) * 4);
        orbitBuffer.SetData(packed);

        shader.SetInt("Width",  Width);
        shader.SetInt("Height", Height);
        shader.SetInt("MaxIterations", maxIterations);
        shader.SetFloat("Bailout", bailout);

        // C0 as double-double
        shader.SetVector("C0_Re", HighPrecision.SplitToFloat2(c0.re));
        shader.SetVector("C0_Im", HighPrecision.SplitToFloat2(c0.im));

        // Exact center - C0 via fixed-point, then split to DD
        BigInteger deltaReF = centerReF - c0ReF;
        BigInteger deltaImF = centerImF - c0ImF;
        double deltaReD = ToDouble(deltaReF);
        double deltaImD = ToDouble(deltaImF);
        shader.SetVector("Center_Re_Diff", HighPrecision.SplitToFloat2(deltaReD));
        shader.SetVector("Center_Im_Diff", HighPrecision.SplitToFloat2(deltaImD));

        // Scale as DD
        shader.SetVector("Scale", HighPrecision.SplitToFloat2(ToDouble(scaleF)));

        shader.SetInts("C0_Pixel", c0Pixel.x, c0Pixel.y);

        int kernel = shader.FindKernel("MandelbrotKernel");
        shader.SetBuffer(kernel, "OrbitHL", orbitBuffer);
        if(target != null){
            shader.SetTexture(kernel, "Result", target);
        }
        shader.SetInt("Debug", debug ? 1 : 0);
        shader.SetFloat("LightDirection", lightDirection);
        shader.SetFloat("LightHeight", lightHeight);
    }

    void Dispatch()
    {
        int kernel = shader.FindKernel("MandelbrotKernel");
        int gx = Mathf.CeilToInt(Width  / 8.0f);
        int gy = Mathf.CeilToInt(Height / 8.0f);

        gx = Mathf.Max(1, gx);
        gy = Mathf.Max(1, gy);

        shader.Dispatch(kernel, gx, gy, 1);
    }
}
