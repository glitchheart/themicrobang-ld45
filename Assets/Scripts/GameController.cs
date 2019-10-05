using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GameController : MonoBehaviour
{
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
}
