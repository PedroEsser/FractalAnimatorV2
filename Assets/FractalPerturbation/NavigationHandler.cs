using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UI_Utils;

public class NavigationHandler
{
    public static readonly FixedFloat zoomFactor = new FixedFloat(1.5);
    public BigComplex Center = BigComplex.ParseDecimalString("-0.717413872065201 + 0.208571862722845i");
    public FixedFloat Scale = new FixedFloat(1.0 / 1024.0);

    private UI_Window window;
    public int Width => (int)window.Width;
    public int Height => (int)window.Height;

    public NavigationHandler(UI_Window window){
        this.window = window;
        window.OnMouseDrag.AddListener(OnDrag);
        window.OnMouseScroll.AddListener(OnScroll);
    }

    public void OnDrag(PointerEventData eventData){
        Vector2 delta = eventData.delta;
        Center = Center - new BigComplex(delta.x * Scale, delta.y * Scale);
    }

    public void OnScroll(PointerEventData eventData){
        int steps = -(int)Mathf.Round(eventData.scrollDelta.y);
        Scale *= zoomFactor ^ steps;
    }

    public void UpdateShader(ComputeShader shader){
        shader.SetVector("Scale", Scale.ToVector2());
        shader.SetInt("Width",  Width);
        shader.SetInt("Height", Height);
    }


}
