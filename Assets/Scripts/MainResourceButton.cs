using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
[AddComponentMenu("MicroBang/MainResourceButton")]
public class MainResourceButton : ResourceMenuButton
{
    [SerializeField]
    private GameObject _menu;

    public override void OnClicked(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            _menu.SetActive(!_menu.activeInHierarchy);
        }
    }

    public override void OnEnter(PointerEventData eventData)
    {
        transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    }

    public override void OnExit(PointerEventData eventData)
    {
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }
}
