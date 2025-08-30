using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;

public class RebaseHandler : MonoBehaviour
{

    NavigationHandler navigationHandler;
    BigComplex Center => navigationHandler.Center;
    FixedFloat Scale => navigationHandler.Scale;
    public int Width => navigationHandler.Width;
    public int Height => navigationHandler.Height;

    public Vector2Int C0Pixel = new Vector2Int(-1, -1);
    BigComplex C0 = null;
    public int maxIterations = 200;
    public float bailout = 4f;
    public bool debug = true;

    ComputeBuffer orbitBuffer;
    private bool needNewRef = true;

    public void SetNavigationHandler(NavigationHandler navigationHandler){
        this.navigationHandler = navigationHandler;
    }

    public void Update(){
        if(navigationHandler == null || Width <= 0 || Height <= 0) return;
        needNewRef = NeedNewReference();
        if(needNewRef){
            HandleRebase();
        }
    }

    public void ReleaseBuffers()
    {
        if (orbitBuffer != null)
        {
            orbitBuffer.Release();
            orbitBuffer = null;
        }
    }

    public void ToggleDebug(){
        debug = !debug;
    }

    void OnGUI()
    {
        if(!debug || C0 == null) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.red;
        GUI.Label(new Rect(10, 10, 100, 20), $"Scale: {Scale.ToDouble()}", style);
        GUI.Label(new Rect(10, 30, 100, 20), $"Center: {Center.Real.ToDouble()} {Center.Imag.ToDouble()}", style);
        GUI.Label(new Rect(10, 50, 100, 20), $"C0: {C0.Real.ToDouble()} {C0.Imag.ToDouble()}", style);
    }

    // Rebase

    public static long OrthogonalPixelDistance(BigInteger a, BigInteger b, BigInteger scale)
    {
        BigInteger dF = BigInteger.Abs(a - b);
        BigInteger px = dF / scale;
        if (px > long.MaxValue) return long.MaxValue;
        return (long)px;
    }

    bool NeedNewReference(){
        if(needNewRef || C0Pixel.x < 0 || C0 == null || !IsSafeReference(C0)) return true;
        
        long dxPixels = OrthogonalPixelDistance(Center.Real.Value, C0.Real.Value, Scale.Value);
        long dyPixels = OrthogonalPixelDistance(Center.Imag.Value, C0.Imag.Value, Scale.Value);
        const long MAX_PIXELS = 120;
        long pixelDistance = dxPixels * dxPixels + dyPixels * dyPixels;
        return pixelDistance > MAX_PIXELS * MAX_PIXELS;
    }

    bool IsSafeReference(BigComplex c, int? maxIter= null, float? bailoutVal= null)
    {
        BigComplex z = BigComplex.ZERO;
        if(maxIter == null) maxIter = maxIterations;
        if(bailoutVal == null) bailoutVal = bailout;
        for (int n = 0; n < maxIter + 10; n++)
        {
            z = z.Square() + c;
            if (z.Abs() > bailoutVal) return false;
        }
        return true;
    }

    (bool found, int dx, int dy, BigComplex cRef) PickSafeReferencePixel(Vector2Int anchor, int maxRadius)
    {
        Vector2Int diffPixels = new Vector2Int(anchor.x - Width/2, anchor.y - Height/2);
        
        for (int dy = -maxRadius + diffPixels.y; dy <= maxRadius + diffPixels.y; dy++)
        for (int dx = -maxRadius + diffPixels.x; dx <= maxRadius + diffPixels.x; dx++)
            {
                BigComplex candidate = new BigComplex(Center.Real + (long)dx * Scale, Center.Imag + (long)dy * Scale);
                if (IsSafeReference(candidate))
                    return (true, dx, dy, candidate);
            }
        
        return (false, 0, 0, null);
    }

    public UnityEngine.Vector4[] BuildReferenceOrbitDataForExactPerturbation(BigComplex c, int N)
    {
        UnityEngine.Vector4[] orbitData = new UnityEngine.Vector4[N];
        BigComplex Z = BigComplex.ZERO;
        for (int i = 0; i < N; i++)
        {
            orbitData[i] = Z.ToVector4();
            Z = Z.Square() + c;
        }
        return orbitData;
    }

    public UnityEngine.Vector4[] BuildReferenceOrbitDataForSeriesApproximation(BigComplex c, int N)
    {
        UnityEngine.Vector4[] orbitData = new UnityEngine.Vector4[3*N];
        BigComplex Z = BigComplex.ZERO;
        BigComplex A = BigComplex.ZERO;
        BigComplex B = BigComplex.ZERO;
        for (int i = 0; i < N; i++)
        {
            orbitData[i * 3 + 0] = Z.ToVector4();
            orbitData[i * 3 + 1] = A.ToVector4();
            orbitData[i * 3 + 2] = B.ToVector4();
            B = 2 * Z * B + A.Square();
            A = 2 * Z * A + BigComplex.ONE;
            Z = Z.Square() + c;
        }
        return orbitData;
    }
    

    void HandleRebase(Vector2Int? anchor = null, int maxRadius = 2){
        var pick = PickSafeReferencePixel(anchor ?? new Vector2Int(Width/2, Height/2), maxRadius);
        if (pick.found)
        {
            Debug.Log($"Rebase Successful: new C0 at pixel {C0Pixel.x}, {C0Pixel.y} with value {C0}");
            C0Pixel = new Vector2Int(Width / 2 + pick.dx, Height / 2 + pick.dy);
            C0 = pick.cRef;
            ReleaseBuffers();
            UnityEngine.Vector4[] orbitData = BuildReferenceOrbitDataForExactPerturbation(C0, maxIterations);
            //UnityEngine.Vector4[] orbitData = BuildReferenceOrbitDataForSeriesApproximation(C0, maxIterations);
            orbitBuffer = new ComputeBuffer(orbitData.Length, sizeof(float) * 4);
            orbitBuffer.SetData(orbitData);
            needNewRef = false;
        }
        else
        {
            Debug.Log($"Rebase failed around center {Center.Real.ToDouble()} {Center.Imag.ToDouble()} (scale {Scale.ToDouble()}). keeping old C0: {C0}");
        }
    }

    public void UpdateShader(ComputeShader shader, int kernel){
        if(C0 == null) return;
        shader.SetVector("C0", C0.ToVector4());
        BigComplex delta = Center - C0;
        shader.SetVector("Center_Diff", delta.ToVector4());
        shader.SetInts("C0_Pixel", C0Pixel.x, C0Pixel.y);
        shader.SetBuffer(kernel, "OrbitData", orbitBuffer);
        shader.SetInt("MaxIterations", maxIterations);
        shader.SetFloat("Bailout", bailout);
        shader.SetInt("Debug", debug ? 1 : 0);
    }
}
