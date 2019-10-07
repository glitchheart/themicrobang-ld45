using System.Collections;
using System.Collections.Generic;
 using UnityEngine;
 using UnityEngine.UI;
 using UnityEngine.Events;
 using UnityEngine.EventSystems;

public abstract class ResourceMenuButton : Button
{
    [SerializeField]
    private GameObject _menuToShow;

    private bool _down;
    private PointerEventData _downEventData;

    void Update()
    {
        if(_down)
        {
            OnDown(_downEventData);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        _downEventData = eventData;
        _down = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        _downEventData = null;
        _down = false;
        OnUp(eventData);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        OnClicked(eventData);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        OnEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnExit(eventData);
    }

    public virtual void OnDown(PointerEventData eventData) {}
    public virtual void OnClicked(PointerEventData eventData) {}

    public virtual void OnUp(PointerEventData eventData) {}

    public virtual void OnEnter(PointerEventData eventData) {}

    public virtual void OnExit(PointerEventData eventData) {}

}
