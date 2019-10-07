using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : PrefabObject
{
    #region animator hashes
    private int _animScaleUp;
    #endregion

    [HideInInspector]
    [SerializeField]
    private AudioSource _audioSource;

    [HideInInspector]
    [SerializeField]
    private Animator _animator;

    private void OnValidate()
    {
        if(_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }
    }

    private void Awake()
    {
        _audioSource.Play();
        _animScaleUp = Animator.StringToHash("scaleUp");
    }

    public void Appear()
    {
        _animator.SetTrigger(_animScaleUp);
    }
}