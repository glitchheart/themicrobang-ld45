using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : Controller<CameraController>
{
    public const int INTRO_CAMERA = 1;
    public const int INITIAL_PLAY_CAMERA = 2;
    public const int PLANET_CAMERA = 3;

    private Dictionary<int, GameCamera> _gameCameras;

    public GameCamera CurrentCamera { get; private set; }

    protected override void OnAwake()
    {
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
