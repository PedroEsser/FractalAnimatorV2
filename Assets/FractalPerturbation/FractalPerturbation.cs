using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI_Utils;
using UnityEngine.EventSystems;
using static FixedFloat;

public class FractalPerturbation : MonoBehaviour
{
    [Header("Compute Shader")]
    public ComputeShader shader;

    [Header("Output")]
    public RenderTexture target;

    public NavigationHandler navigationHandler;
    public RebaseHandler rebaseHandler;

    public int kernel = 0;
    public int Width => (int)window.Width;
    public int Height => (int)window.Height;

    ComputeBuffer orbitBuffer;
    public RawImage image;
    public UI_Window window;
    public float lightDirection = 0.0f;
    public float lightHeight = 2.0f;

    

    void OnEnable()
    {
        navigationHandler = new NavigationHandler(window);
        rebaseHandler.SetNavigationHandler(navigationHandler);
        kernel = shader.FindKernel("MandelbrotKernel");
    }

    void OnDisable()
    {
        rebaseHandler.ReleaseBuffers();
        if (target != null) { target.Release(); target = null; }
    }

    void Update()
    {
        HandleInput();
        if(Width > 0 && Height > 0){
            CreateOrResizeTarget();
            UpdateShader();
            Dispatch();
            if (image) image.texture = target;
        }
    }

    void HandleInput(){
        if(Input.GetKeyDown(KeyCode.Space)){ 
            rebaseHandler.ToggleDebug();
        }
    }

    void CreateOrResizeTarget()
    {
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

    void UpdateShader(){
        navigationHandler.UpdateShader(shader);
        rebaseHandler.UpdateShader(shader, kernel);
        shader.SetFloat("LightDirection", lightDirection);
        shader.SetFloat("LightHeight", lightHeight);

        if(target != null){
            shader.SetTexture(kernel, "Result", target);
        }
    }

    void Dispatch()
    {
        int gx = Mathf.CeilToInt(Width  / 8.0f);
        int gy = Mathf.CeilToInt(Height / 8.0f);

        gx = Mathf.Max(1, gx);
        gy = Mathf.Max(1, gy);

        shader.Dispatch(kernel, gx, gy, 1);
    }
}
