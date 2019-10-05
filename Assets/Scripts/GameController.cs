using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GameController : Controller<GameController>
{
    public abstract class GameCommand
    {
        public MonoBehaviour MB;
        public Action Callback;
        public Action OnDone;
        public abstract void Execute();
    }

    public class SpawnStarCommand : GameCommand
    {
        public float Delay;
        public float Radius;
        public int Amount;

        private int _spawned;

        IEnumerator SpawnStar()
        {
            yield return new WaitForSeconds(Delay);
            GameObject go = PrefabController.Instance.GetPrefabInstance(PrefabController.STAR_PREFAB);
            go.transform.localScale = new Vector3(20, 20, 20);
            go.transform.position = UnityEngine.Random.insideUnitSphere * Radius;
            go.GetComponent<Star>().Appear();
            _spawned++;

            if(_spawned == Amount)
            {
                Callback?.Invoke();
                OnDone?.Invoke();
            }
            else
            {
                MB.StartCoroutine(SpawnStar());
            }
        }

        public override void Execute()
        {
            MB.StartCoroutine(SpawnStar());
        }
    }

    public enum GameMode
    {
        Intro,
        Playing,
        Paused
    }

    public enum PlayMode
    {
        MovingAround,
        Placing
    }

    [SerializeField]
    private Universe _universe;

    [SerializeField]
    private Transform _cameraOrbit;

    [SerializeField]
    private GameMode _gameMode;

    private PlayMode _playMode;
    private GameObject _planetToPlace;

    [SerializeField]
    private PlayableDirector _introPlayable;

    private GameCommand _currentCommand;
    private Queue<GameCommand> _gameCommands;

    private Vector3 _lastMousePosition;

    [SerializeField]
    private float _rotateSpeed = 2.0f;
    private float _scrollSpeed = 2.0f;

    public SphericalCoordinates sc;

    protected override void OnAwake()
    {
        sc = new SphericalCoordinates(transform.position, 3f, 10f, 0f, Mathf.PI * 2f, 0f, Mathf.PI / 4f);
        _gameCommands = new Queue<GameCommand>();
    }

    private void Start()
    {
        CameraController.Instance.SwitchToCamera(CameraController.INTRO_CAMERA);
    }

    public void PlacePlanet()
    {
        _universe.PlaceNextPlanet();
    }

    private void Update()
    {
        if(!RunCommands())
        {
            switch (_gameMode)
            {
                case GameMode.Intro:
                    if (Input.anyKeyDown)
                    {
                        _introPlayable.Play();
                    }
                    break;
                case GameMode.Playing:
                    if(_playMode == PlayMode.MovingAround)
                    {
                        if (Input.GetMouseButton(1))
                        {
                            float h = _rotateSpeed * Input.GetAxis("Mouse X");
                            float v = _rotateSpeed * Input.GetAxis("Mouse Y");
                            _cameraOrbit.transform.RotateAroundLocal(_cameraOrbit.transform.right, h * _rotateSpeed * Time.deltaTime);
                            _cameraOrbit.transform.RotateAroundLocal(_cameraOrbit.transform.up, v * _rotateSpeed * Time.deltaTime);
                        }

                        float scrollFactor = Input.GetAxis("Mouse ScrollWheel");

                        _cameraOrbit.transform.localScale = _cameraOrbit.transform.localScale * (1f - scrollFactor);
                    }
                    else if(_playMode == PlayMode.Placing)
                    {
                        if(Input.GetMouseButton(0))
                        {
                            
                        }

                        if(Input.GetMouseButtonUp(0))
                        {
                            _playMode = PlayMode.MovingAround;
                            _planetToPlace = null;
                        }
                    }

                    break;
                case GameMode.Paused:
                    break;
            }
        }
    }

    public void StartPlayMode()
    {
        HelpUIController.Instance.SetHelpTextEnabled(false);
        CameraController.Instance.SwitchToCamera(CameraController.INITIAL_PLAY_CAMERA);
        CameraController.Instance.CurrentCamera.CameraOrbit.Running = false;

        QueueCommand(new SpawnStarCommand { Amount = 20, Radius = 50, Delay = 0.1f, Callback = () =>
        {
            _gameMode = GameMode.Playing;
        } });
    }

    public void QueueCommand(GameCommand command)
    {
        command.MB = this;
        command.OnDone = OnDone;
        _gameCommands.Enqueue(command);
    }

    void OnDone()
    {
        _currentCommand = null;
    }

    bool RunCommands()
    {
        if(_currentCommand == null && _gameCommands.Count > 0)
        {
            _currentCommand = _gameCommands.Dequeue();
            _currentCommand.Execute();
        }

        return _currentCommand != null;
    }
}
