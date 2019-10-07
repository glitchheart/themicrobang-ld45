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

    private int _animPopMenu; 

    [SerializeField]
    private Animator menuAnimator;

    [SerializeField]
    private string popString;

    protected override void Start()
    {
        _animPopMenu = Animator.StringToHash(popString);
    }

    public override void OnClicked(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            menuAnimator.SetTrigger(_animPopMenu);
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
