using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    #region animator hashes
    private int _animScaleUp;
    #endregion

    [HideInInspector]
    [SerializeField]
    private Animator _animator;

    private void OnValidate()
    {
        if(_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    private void Awake()
    {
        _animScaleUp = Animator.StringToHash("scaleUp");
    }

    public void Appear()
    {
        _animator.SetTrigger(_animScaleUp);
    }
}
