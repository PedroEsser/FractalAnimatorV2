using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ComplexDouble;

public class FractalPerturbation : MonoBehaviour
{
    [Header("Compute Shader")]
    public ComputeShader shader;

    [Header("Output")]
    public RenderTexture target;

    [Header("View")]
    public double centerRe = 0;   // tweak for fun
    public double centerIm =  0;
    public double pixelScale = 3.0 / 1024.0;       // world units per pixel (zoom)

    [Header("Iterations")]
    public int maxIters = 200;
    public float bailout = 4f;

    [Header("Resolution")]
    public int width = 1024;
    public int height = 1024;

    ComputeBuffer orbitBuffer;
    public RawImage image;

    struct OrbitHL
    {
        public float re_hi, re_lo, im_hi, im_lo;
    }

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
        if (Input.GetKey(KeyCode.Q)) pixelScale *= 0.98;
        if (Input.GetKey(KeyCode.E)) pixelScale /= 0.98;

        double pan = pixelScale;
        if (Input.GetKey(KeyCode.W)) centerIm += pan;
        if (Input.GetKey(KeyCode.S)) centerIm -= pan;
        if (Input.GetKey(KeyCode.A)) centerRe -= pan;
        if (Input.GetKey(KeyCode.D)) centerRe += pan;

        BuildAndUploadOrbit();
        Dispatch();
        image.texture = target;
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

    List<(double re, double im)> BuildReferenceOrbit(double cRe, double cIm, int N)
    {
        var list = new List<(double, double)>(N);
        double zr = 0.0, zi = 0.0;

        for (int i = 0; i < N; i++)
        {
            list.Add((zr, zi));
            double zr2 = zr * zr - zi * zi + cRe;
            double zi2 = 2.0 * zr * zi + cIm;
            zr = zr2; zi = zi2;
        }
        return list;
    }

    bool IsSafeReference(double cRe, double cIm, int maxIters, float bailout)
    {
        double zr = 0.0, zi = 0.0;
        for (int n = 0; n < maxIters; n++)
        {
            double zr2 = zr * zr - zi * zi + cRe;
            double zi2 = 2.0 * zr * zi + cIm;
            zr = zr2;
            zi = zi2;

            if (zr * zr + zi * zi > bailout) return false;
        }
        return true;
    }

    // Find nearest safe reference pixel (search radius in pixels)
    (Vector2 referenceOffset, double refRe, double refIm) PickSafeReference(
        double centerRe, double centerIm, double pixelScale, int maxIters, float bailout, int maxRadius = 10)
    {
        // Spiral search around center pixel
        for (int r = 0; r <= maxRadius; r++)
        {
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    double testRe = centerRe + dx * pixelScale;
                    double testIm = centerIm + dy * pixelScale;

                    if (IsSafeReference(testRe, testIm, maxIters, bailout))
                        return (new Vector2(dx, dy), testRe, testIm);
                }
            }
        }

        // Fallback: use center itself if nothing found
        return (Vector2.zero, centerRe, centerIm);
    }

    void BuildAndUploadOrbit()
    {
        ReleaseBuffers();

        var (refOffset, cRefRe, cRefIm) = PickSafeReference(
            centerRe, centerIm, pixelScale, maxIters, bailout);

        var orbit = BuildReferenceOrbit(cRefRe, cRefIm, maxIters);

        OrbitHL[] packed = new OrbitHL[orbit.Count];
        for (int i = 0; i < orbit.Count; i++)
        {
            SplitDouble(orbit[i].re, out float rhi, out float rlo);
            SplitDouble(orbit[i].im, out float ihi, out float ilo);
            packed[i] = new OrbitHL { re_hi = rhi, re_lo = rlo, im_hi = ihi, im_lo = ilo };
        }

        orbitBuffer = new ComputeBuffer(packed.Length, sizeof(float) * 4);
        orbitBuffer.SetData(packed);

        // --- 4) Compute C0 and scale as dd ---
        SplitDouble(cRefRe, out float c0re_hi, out float c0re_lo);
        SplitDouble(cRefIm, out float c0im_hi, out float c0im_lo);
        SplitDouble(pixelScale, out float scl_hi, out float scl_lo);

        int kernel = shader.FindKernel("MandelbrotKernel");
        shader.SetInt("Width", width);
        shader.SetInt("Height", height);
        shader.SetInt("MaxIters", maxIters);
        shader.SetFloat("Bailout", bailout);

        // --- 5) Pass reference orbit center (C0) ---
        shader.SetVector("C0_Re", new Vector2(c0re_hi, c0re_lo));
        shader.SetVector("C0_Im", new Vector2(c0im_hi, c0im_lo));

        // --- 6) Pass screen center separately for mapping ---
        SplitDouble(centerRe, out float centerRe_hi, out float centerRe_lo);
        SplitDouble(centerIm, out float centerIm_hi, out float centerIm_lo);
        shader.SetVector("ScreenCenter_Re", new Vector2(centerRe_hi, centerRe_lo));
        shader.SetVector("ScreenCenter_Im", new Vector2(centerIm_hi, centerIm_lo));

        shader.SetVector("Scale", new Vector2(scl_hi, scl_lo));
        shader.SetInt("OrbitLength", maxIters);
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
