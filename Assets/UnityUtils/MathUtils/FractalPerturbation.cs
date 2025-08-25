using System;
using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ComplexDouble;
using static HighPrecision;

public class FractalPerturbation : MonoBehaviour
{
    [Header("Compute Shader")]
    public ComputeShader shader;

    [Header("Output")]
    public RenderTexture target;

    [Header("View")]
    public Vector2Int c0Pixel = new Vector2Int(-1, -1);
    public ComplexDouble c0;         // world coords of reference orbit
    public ComplexDouble center = new ComplexDouble(-0.717413872065201, 0.208571862722845);        // -0,717413872065201 + 0,208571862722845i
    public ComplexDouble centerDiff;
    public double pixelScale = 3.0 / 1024.0;   // world units per pixel

    [Header("Iterations")]
    public int maxIters = 200;
    public float bailout = 4f;

    [Header("Resolution")]
    public int width = 1024;
    public int height = 1024;

    ComputeBuffer orbitBuffer;
    public RawImage image;

    UnityEngine.Vector2? lastMousePos;
    void OnEnable()
    {
        CreateTarget();
        BuildAndUploadOrbit();
    }

    void OnDisable()
    {
        ReleaseBuffers();
    }

    void Update()
    {
        HandleInput();

        BuildAndUploadOrbit();
        Dispatch();
        image.texture = target;
    }

    void HandleInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.001f)
        {
            double zoomFactor = Math.Pow(0.9, scroll);
            pixelScale *= zoomFactor;
        }

        if (Input.GetMouseButtonDown(0))
            lastMousePos = Input.mousePosition;
        else if (Input.GetMouseButton(0) && lastMousePos.HasValue)
        {
            UnityEngine.Vector2 delta = (UnityEngine.Vector2)Input.mousePosition - lastMousePos.Value;
            lastMousePos = Input.mousePosition;

            center = center - ComplexDouble.AsComplexDouble(delta) * pixelScale;
        }
        else if (Input.GetMouseButtonUp(0))
            lastMousePos = null;

        double pan = pixelScale * 10;
        if (Input.GetKey(KeyCode.W)) center._im += pan;
        if (Input.GetKey(KeyCode.S)) center._im -= pan;
        if (Input.GetKey(KeyCode.A)) center._re -= pan;
        if (Input.GetKey(KeyCode.D)) center._re += pan;
    }

    void CreateTarget()
    {
        if (target != null && (target.width != width || target.height != height))
        {
            target.Release();
            target = null;
        }
        if (target == null)
        {
            target = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
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

    List<ComplexDouble> BuildReferenceOrbit(ComplexDouble c, int N)
    {
        var list = new List<ComplexDouble>(N);
        ComplexDouble z = new ComplexDouble(0.0, 0.0);

        for (int i = 0; i < N; i++)
        {
            list.Add(z);
            z = z * z + c;
        }
        return list;
    }

    bool IsSafeReference(ComplexDouble c, int maxIters, float bailout)
    {
        ComplexDouble z = new ComplexDouble(0.0, 0.0);
        for (int n = 0; n < maxIters+10; n++)
        {
            z = z * z + c;
            if (z.squaredNorm() > bailout) return false;
        }
        return true;
    }

    (bool found, int dx, int dy, ComplexDouble cRef) PickSafeReferencePixel(
        int width, int height,
        int centerX, int centerY,
        ComplexDouble center,
        double pixelScale, int maxIters, float bailout,
        Vector2Int? lastRefPixel = null,
        int maxRadius = 5)
    {
        // Search near last reference first
        /*if (lastRefPixel.HasValue)
        {
            int baseX = lastRefPixel.Value.x;
            int baseY = lastRefPixel.Value.y;

            for (int r = 0; r <= maxRadius; r++)
            {
                for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    int px = baseX + dx;
                    int py = baseY + dy;
                    if (px < 0 || py < 0 || px >= width || py >= height) continue;

                    ComplexDouble p = new ComplexDouble(px - centerX, py - centerY);
                    p *= pixelScale;
                    p += center;

                    if (IsSafeReference(p, maxIters, bailout))
                        return (true, px - centerX, py - centerY, p);
                }
            }
        }*/

        // Fallback: search around screen center
        for (int r = 0; r <= maxRadius; r++)
        {
            for (int dy = -r; dy <= r; dy++)
            for (int dx = -r; dx <= r; dx++)
            {
                int px = centerX + dx;
                int py = centerY + dy;
                if (px < 0 || py < 0 || px >= width || py >= height) continue;

                ComplexDouble p = new ComplexDouble(dx, dy);
                p *= pixelScale;
                p += center;

                if (IsSafeReference(p, maxIters, bailout))
                    return (true, dx, dy, p);
            }
        }

        return (false, 0, 0, center);
    }

    void BuildAndUploadOrbit()
    {
        ReleaseBuffers();

        int cx = width / 2;
        int cy = height / 2;

        bool needNewRef = false;

        // --- Check if current C0 orbit is unsafe ---
        if (!IsSafeReference(c0, maxIters, bailout))
            needNewRef = true;

        // --- Check if C0 too far from center (screen-space threshold) ---
        ComplexDouble diff = center - c0;
        if (diff.squaredNorm() > pixelScale * pixelScale * 300)
            needNewRef = true;

        // --- Pick a new safe reference orbit if needed ---
        if (needNewRef)
        {
            var pick = PickSafeReferencePixel(
                width, height, cx, cy,
                center,
                pixelScale, maxIters, bailout,
                c0Pixel.x >= 0 ? (Vector2Int?)c0Pixel : null
            );

            if (pick.found)
            {
                c0Pixel = new Vector2Int(cx + pick.dx, cy + pick.dy);
                c0 = pick.cRef;
                Debug.Log($"Found safe reference at pixel {c0Pixel} with value {c0}");
            }
            else
            {
                Debug.Log($"Failed to find safe reference");
            }
        }

        // --- Build reference orbit at C0 ---
        var orbit = BuildReferenceOrbit(c0, maxIters);

        // --- Pack orbit for GPU ---
        var packed = new UnityEngine.Vector4[orbit.Count];
        for (int i = 0; i < orbit.Count; i++)
            packed[i] = orbit[i].AsVector4();

        orbitBuffer = new ComputeBuffer(packed.Length, sizeof(float) * 4);
        orbitBuffer.SetData(packed);

        int kernel = shader.FindKernel("MandelbrotKernel");

        shader.SetInt("Width", width);
        shader.SetInt("Height", height);
        shader.SetInt("MaxIters", maxIters);
        shader.SetFloat("Bailout", bailout);

        // --- Upload C0 ---
        shader.SetVector("C0_Re", AsVector2(c0.re));
        shader.SetVector("C0_Im", AsVector2(c0.im));


        BigInteger c0FixedRe = HighPrecision.ToFixed(c0.re);
        BigInteger c0FixedIm = HighPrecision.ToFixed(c0.im);

        BigInteger centerFixedRe = HighPrecision.ToFixed(center.re);
        BigInteger centerFixedIm = HighPrecision.ToFixed(center.im);

        BigInteger deltaFixedRe = centerFixedRe - c0FixedRe;
        BigInteger deltaFixedIm = centerFixedIm - c0FixedIm;

        // Convert delta to DDComplex for GPU
        double deltaRe = HighPrecision.ToDouble(deltaFixedRe);
        double deltaIm = HighPrecision.ToDouble(deltaFixedIm);
        

        shader.SetVector("Center_Re_Diff", ComplexDouble.AsVector2(deltaRe));
        shader.SetVector("Center_Im_Diff", ComplexDouble.AsVector2(deltaIm));

        // --- Upload scale ---
        shader.SetVector("Scale", new UnityEngine.Vector2((float)pixelScale, 0f));

        // --- Upload C0 pixel for perturbation ---
        shader.SetInts("C0Pixel", c0Pixel.x, c0Pixel.y);

        // --- Set orbit buffer and result texture ---
        shader.SetBuffer(kernel, "OrbitHL", orbitBuffer);
        CreateTarget();
        shader.SetTexture(kernel, "Result", target);
    }

    void Dispatch()
    {
        int kernel = shader.FindKernel("MandelbrotKernel");
        int gx = Mathf.CeilToInt(width / 8.0f);
        int gy = Mathf.CeilToInt(height / 8.0f);
        shader.Dispatch(kernel, gx, gy, 1);
    }
}
