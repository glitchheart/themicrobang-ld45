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

    [SerializeField]
    private Transform _cameraOrbit;

    [SerializeField]
    private GameMode _gameMode;

    [SerializeField]
    private PlayableDirector _introPlayable;

    private GameCommand _currentCommand;
    private Queue<GameCommand> _gameCommands;

    private Vector3 _lastMousePosition;
    private float _rotateSpeed = 10.0f;

    protected override void OnAwake()
    {
        _gameCommands = new Queue<GameCommand>();
    }

    private void Start()
    {
        CameraController.Instance.SwitchToCamera(CameraController.INTRO_CAMERA);
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
                    if (Input.GetMouseButton(0))
                    {
                        float h = _rotateSpeed * Input.GetAxis("Mouse X");
                        float v = _rotateSpeed * Input.GetAxis("Mouse Y");

                        if (_cameraOrbit.eulerAngles.z + v <= 0.1f || _cameraOrbit.eulerAngles.z + v >= 270.0f)
                            v = 0;

                        _cameraOrbit.eulerAngles = new Vector3(_cameraOrbit.eulerAngles.x,
                                                               _cameraOrbit.eulerAngles.y + h,
                                                               _cameraOrbit.eulerAngles.z + v);
                    }

                    float scrollFactor = Input.GetAxis("Mouse ScrollWheel");

                    if (scrollFactor != 0)
                    {
                        _cameraOrbit.localScale = _cameraOrbit.localScale * (1f - scrollFactor);
                    }

                    CameraController.Instance.CurrentCamera.transform.rotation = Quaternion.Euler(CameraController.Instance.CurrentCamera.transform.rotation.x, CameraController.Instance.CurrentCamera.transform.rotation.y, 0);
                    CameraController.Instance.CurrentCamera.transform.LookAt(_cameraOrbit.position);
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
