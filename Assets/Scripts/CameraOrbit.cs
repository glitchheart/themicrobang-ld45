using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [SerializeField]
    private float _orbitSpeed = 10.0f;

    [SerializeField]
    public bool Running;

    [SerializeField]
    private Transform _orbitAround;

    void Update()
    {
        if(Running)
        {
            transform.RotateAround(_orbitAround.position, Vector3.up, _orbitSpeed * Time.deltaTime);
        }
    }
}
