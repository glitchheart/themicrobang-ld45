using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public const int INTRO_CAMERA = 1;
    public const int INITIAL_PLAY_CAMERA = 2;

    public static CameraController Instance;

    private Dictionary<int, GameCamera> _gameCameras;

    public GameCamera CurrentCamera { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        _gameCameras = new Dictionary<int, GameCamera>();

        var cameras = FindObjectsOfType<GameCamera>();
        foreach (var cam in cameras)
        {
            _gameCameras.Add(cam.ID, cam);
        }
    }

    public void SwitchToCamera(int id)
    {
        if (_gameCameras.ContainsKey(id))
        {
            if (CurrentCamera != null)
            {
                CurrentCamera.VirtualCamera.Priority = 0;
            }

            CurrentCamera = _gameCameras[id];
            CurrentCamera.VirtualCamera.Priority = 10;
        }
    }
}
