using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : PrefabObject
{
    [SerializeField]
    private ParticleSystem[] _particleSystems;

    private void OnValidate()
    {
        if(_particleSystems == null)
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    public void Explode()
    {
        _particleSystems[0].Play();
        _particleSystems[1].Play();

        StartCoroutine(DelayDestruction());
    }

    IEnumerator DelayDestruction()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
