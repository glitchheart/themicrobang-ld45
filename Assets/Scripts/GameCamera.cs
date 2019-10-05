using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [SerializeField]
    public int ID;

    [HideInInspector]
    [SerializeField]
    public Cinemachine.CinemachineVirtualCamera VirtualCamera;

    [HideInInspector]
    [SerializeField]
    public CameraOrbit CameraOrbit;

    private void OnValidate()
    {
        if(VirtualCamera == null)
        {
            VirtualCamera = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        }

        if (CameraOrbit == null)
        {
            CameraOrbit = GetComponent<CameraOrbit>();
        }
    }
}
