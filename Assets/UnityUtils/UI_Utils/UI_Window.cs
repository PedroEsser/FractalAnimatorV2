using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UI_Window : MonoBehaviour
{

    public Image Background;
    public RectTransform Rect;
    public float Width => Rect.rect.width;
    public float Height => Rect.rect.height;
    public Vector2 RectSize => new Vector2(Width, Height);
    public Vector2 RectPosition => new Vector2(Rect.rect.x, Rect.rect.y);
    
    private bool _isMouseOver = false;
    private bool _isMouseDown = false;
    public bool IsMouseOver
    {
        get => _isMouseOver;
    }

    public bool IsMouseDown
    {
        get => _isMouseDown;
    }

    public UnityEvent<PointerEventData> OnMouseEnter;
    public UnityEvent<PointerEventData> OnMouseExit;
    public UnityEvent<PointerEventData> OnMouseDown;
    public UnityEvent<PointerEventData> OnMouseUp;
    public UnityEvent<PointerEventData> OnMouseDrag;
    public UnityEvent<PointerEventData> OnMouseScroll;

    public void MouseEnter(BaseEventData eventData){
        _isMouseOver = true;
        OnMouseEnter.Invoke((PointerEventData)eventData);
    }

    public void MouseExit(BaseEventData eventData){
        _isMouseOver = false;
        OnMouseExit.Invoke((PointerEventData)eventData);
    }

    public void MouseDown(BaseEventData eventData){ 
        _isMouseDown = true;
        OnMouseDown.Invoke((PointerEventData)eventData);
    }

    public void MouseUp(BaseEventData eventData){
        _isMouseDown = false;
        OnMouseUp.Invoke((PointerEventData)eventData);
    }

    public void MouseDrag(BaseEventData eventData){
        OnMouseDrag.Invoke((PointerEventData)eventData);
    }

    public void MouseScroll(BaseEventData eventData){
        OnMouseScroll.Invoke((PointerEventData)eventData);
    }
}
