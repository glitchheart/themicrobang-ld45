using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : PrefabObject
{
    [SerializeField]
    private ParticleSystem[] _particleSystems;

    [SerializeField]
    private AudioSource _audioSource;

    private void OnValidate()
    {
        if(_particleSystems == null)
            _particleSystems = GetComponentsInChildren<ParticleSystem>();

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    public void Explode()
    {
        _particleSystems[0].Play();
        _particleSystems[1].Play();
        _audioSource.pitch = Random.Range(0.7f, 1.3f);
        _audioSource.Play();
        StartCoroutine(DelayDestruction());
    }

    IEnumerator DelayDestruction()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
