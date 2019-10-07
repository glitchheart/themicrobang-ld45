using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Resource : PrefabObject
{
    public Vector3 Destination;

    public Planet.ResourceType ResourceType;

    [HideInInspector]
    [SerializeField]
    private AudioSource _audioSource;

    [HideInInspector]
    [SerializeField]
    private Renderer _renderer;

    private void OnValidate()
    {
        if(_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        if (_renderer == null)
            _renderer = GetComponentInChildren<Renderer>();
    }

    public void SelfDestruct()
    {
        StartCoroutine(TimedDestruction());
    }

    IEnumerator TimedDestruction()
    {
        _audioSource.pitch = Random.Range(0.9f, 1.1f);
        _audioSource.Play();
        _renderer.enabled = false;

        while (_audioSource.isPlaying)
            yield return null;

        Destroy(gameObject);
    }
}
