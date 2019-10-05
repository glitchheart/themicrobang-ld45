using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GameController : MonoBehaviour
{
    public interface IGameCommand
    {
    }

    public struct SpawnStarCommand
    {
        public Bounds Bounds;
        public int Amount;
    }

    public enum GameMode
    {
        Intro,
        Playing,
        Paused
    }

    [SerializeField]
    private GameMode _gameMode;

    [SerializeField]
    private PlayableDirector _introPlayable;

    private Queue<IGameCommand> _gameCommands;

    private void Start()
    {
        CameraController.Instance.SwitchToCamera(CameraController.INTRO_CAMERA);
    }

    private void Update()
    {
        switch(_gameMode)
        {
            case GameMode.Intro:
                if (Input.anyKeyDown)
                {
                    _introPlayable.Play();
                }
                break;
            case GameMode.Playing:
                break;
            case GameMode.Paused:
                break;
        }
    }

    public void StartPlayMode()
    {
        HelpUIController.Instance.SetHelpTextEnabled(false);
        CameraController.Instance.SwitchToCamera(CameraController.INITIAL_PLAY_CAMERA);
        CameraController.Instance.CurrentCamera.CameraOrbit.Running = false;
    }
}
