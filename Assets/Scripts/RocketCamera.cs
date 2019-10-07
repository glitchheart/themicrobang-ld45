﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketCamera : MonoBehaviour
{
    private bool _enabled;

    #region animator hashes
    private int _animToggleCam;
    #endregion

    [SerializeField]
    private Animator _animator;

    private void Awake()
    {
        _animToggleCam = Animator.StringToHash("toggle");
    }

    public void ShowShip(Transform shipTransform)
    {
        if(!_enabled)
        {
            _animator.SetTrigger(_animToggleCam);
        }

        _enabled = true;

        transform.parent = shipTransform;
        transform.position = -transform.forward * 2.0f;
        transform.LookAt(shipTransform.position);
    }

    public void TurnOff()
    {
        if(_enabled)
        {
            _animator.SetTrigger(_animToggleCam);
            _enabled = false;    
        }
    }

    private void Update()
    {
        if(_enabled)
        {
            transform.LookAt(transform.parent);
        }
    }

}
