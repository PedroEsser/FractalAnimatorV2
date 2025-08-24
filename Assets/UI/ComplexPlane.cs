using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ComplexPlane : MonoBehaviour
{

    public UI_Window Window;
    public Material Material;
    public QuadComplex Center = QuadComplex.ZERO;
    public QuadFloat Zoom = new QuadFloat(4);
    public QuadFloat PixelDx;
    public QuadFloat PixelDy;
    public QuadFloat ZoomSpeed = new QuadFloat(1.01f) ^ 10;

    public void Start()
    {
        Window.OnMouseScroll.AddListener(OnMouseScroll);
        Window.OnMouseDrag.AddListener(OnMouseDrag);
        SetZoom(Zoom);
        SetCenter(Center);
    }

    public void OnMouseScroll(PointerEventData eventData)
    {
        int power = (int)(-eventData.scrollDelta.y);
        SetZoom(Zoom * (ZoomSpeed ^ power));
    }

    public void OnMouseDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta;
        QuadFloat w = new QuadFloat(Window.Width);
        QuadFloat dx = new QuadFloat(delta.x) / w;
        QuadFloat dy = new QuadFloat(delta.y) / w;
        SetCenter(Center - new QuadComplex(dx, -dy) * Zoom);
    }

    public void SetZoom(QuadFloat zoom)
    {
        Zoom = zoom;
        UpdatePixelDelta();
        Debug.Log("PixelDx: " + PixelDx);
        Debug.Log("Zoom: " + Zoom);
    }

    public void SetCenter(QuadComplex center)
    {
        Center = center;
        Material.SetVector("_Center_Real", Center.real.ToVector4());
        Material.SetVector("_Center_Imag", Center.imag.ToVector4());
        UpdatePixelDelta();
    }

    void UpdatePixelDelta()
    {
        float w = Window.Width;
        float h = Window.Height;
        PixelDx = Zoom / new QuadFloat(w);
        PixelDy = PixelDx;

        Material.SetVector("_PixelDx", PixelDx.ToVector4());
        Material.SetVector("_PixelDy", PixelDy.ToVector4());
    }

}
