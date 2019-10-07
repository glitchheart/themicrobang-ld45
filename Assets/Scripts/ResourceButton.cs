using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
[AddComponentMenu("MicroBang/ResourceButton")]
public class ResourceButton : ResourceMenuButton
{
    [SerializeField]
    private GameController _gameController;

    [SerializeField]
    private bool take;

    [SerializeField]
    private Planet.ResourceType resourceType;

    public override void OnDown(PointerEventData eventData)
    {
        _gameController.ChangeResource(resourceType, take);
    }

    public override void OnUp(PointerEventData eventData)
    {
        _gameController.ResetResourceChange();
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
