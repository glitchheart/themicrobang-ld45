using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Universe : MonoBehaviour
{
    private Planet[] _planets;
    private int _ringCount;

    [SerializeField]
    private int _maxRings = 10;

    public void PlaceNextPlanet()
    {
        if(_ringCount < _maxRings)
        {
            var planet = PrefabController.Instance.GetRandomPlanet();
            Vector3 dir = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)).normalized;
            planet.transform.position = dir * (8 + _ringCount++ * 3.0f);
            planet.Orbit(Vector3.zero, Random.Range(10, 20));
        }
    }
}
