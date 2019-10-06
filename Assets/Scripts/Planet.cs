using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public struct PlanetState
    {
    }

    public string Name;
    public int Population;

    private GameObject _child;

    private Vector3 _center;
    private float _speed;

    private float _timeBeforePopulationGrow = 2.0f;
    private float _time;

    public void Orbit(Vector3 center, float speed = 10.0f)
    {
        //_child = new GameObject("Planet child");
        //_child
        _center = center;
        _speed = speed;
    }

    private void Update()
    {
        transform.RotateAround(_center, Vector3.up, _speed * Time.deltaTime);

        Evolve();

        _time += Time.deltaTime;
    }

    void Evolve()
    {
        // TODO: Add more stuff. States?
        if(_time > _timeBeforePopulationGrow)
        {
            Population += 1;
            _time = 0.0f;
        }
    }
}
