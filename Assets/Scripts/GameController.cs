﻿using System;
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

            if (_spawned == Amount)
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
        Placing,
        PlanetCloseUp
    }

    #region animator hashes
    private int _animFade;
    #endregion

    [SerializeField]
    private LayerMask _planetLayer;

    [SerializeField]
    private Universe _universe;

    [SerializeField]
    private Transform _cameraOrbit;

    [SerializeField]
    private GameMode _gameMode;

    private PlayMode _playMode;
    private Planet _currentPlanet;

    [SerializeField]
    private TMPro.TextMeshProUGUI _planetText;

    [SerializeField]
    private RectTransform _growthArrow;

    [SerializeField]
    private GameObject _planetButton;
    private Animator _planetTextAnimator;

    [SerializeField]
    private PlayableDirector _introPlayable;

    private GameCommand _currentCommand;
    private Queue<GameCommand> _gameCommands;

    private Vector3 _lastMousePosition;


    [SerializeField]
    private float _rotateSpeed = 2.0f;
    private float _scrollSpeed = 2.0f;

    protected override void OnAwake()
    {
        _animFade = Animator.StringToHash("fade");
        _gameCommands = new Queue<GameCommand>();
        _planetTextAnimator = _planetText.GetComponent<Animator>();
    }

    private void Start()
    {
        CameraController.Instance.SwitchToCamera(CameraController.INTRO_CAMERA);
    }

    public void PlacePlanet()
    {
        _universe.PlaceNextPlanet();
    }

    float GetGrowthArrowRotation(Planet planet)
    {
        float rotation = 0.0f;

        float growth = planet.Data.Growth;
        Planet.GrowthState state = planet.GetGrowthState();

        switch (state)
        {
            case Planet.GrowthState.VeryHigh:
                rotation = 90.0f;
                break;
            case Planet.GrowthState.High:
                rotation = 45.0f;
                break;
            case Planet.GrowthState.Medium:
                rotation = 0.0f;
                break;
            case Planet.GrowthState.Low:
                rotation = -45.0f;
                break;
            case Planet.GrowthState.VeryLow:
                rotation = -90.0f;
                break;
            default:
                break;
        }

        return rotation;
    }

    void SetPlanetText(Planet planet)
    {
        String planetState = "";

        switch (planet.Data.State)
        {
            case Planet.PlanetState.Balanced:
                planetState = "Balanced";
                break;
            case Planet.PlanetState.Critical:
                planetState = "Critical";
                break;
            case Planet.PlanetState.Prosperous:
                planetState = "Prosperous";
                break;
            case Planet.PlanetState.Desperation:
                planetState = "Desperation";
                break;
            case Planet.PlanetState.Extinction:
                planetState = "Extinction";
                break;
        }

        _planetText.text = $"Name: {planet.Name}\nPopulation: {planet.Data.Population}\nGrowth: {planet.Data.Growth}\n";
        _planetText.text += $"EvoState: {(planet.Data.EvolutionState == Planet.EvolutionState.Environment ? "Environment" : "Technology")}\n";
        _planetText.text += $"Planet State: {planetState}\n";
        _planetText.text += $"Tech: {planet.Data.TechResource}\n";
        _planetText.text += $"Environment: {planet.Data.EnvironmentResource}\n";
    }

    private void Update()
    {
        if (!RunCommands())
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
                    if (_playMode == PlayMode.PlanetCloseUp)
                    {
                        SetPlanetText(_currentPlanet);
                        _growthArrow.rotation = Quaternion.Euler(0.0f, 0.0f, GetGrowthArrowRotation(_currentPlanet));

                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            _planetTextAnimator.SetTrigger(_animFade);
                            _playMode = PlayMode.MovingAround;
                            CameraController.Instance.SwitchToCamera(CameraController.INITIAL_PLAY_CAMERA);
                        }
                    }
                    else if (_playMode == PlayMode.MovingAround) // || _playMode == PlayMode.PlanetCloseUp)
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

                        if (Input.GetMouseButtonDown(0))
                        {
                            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _planetLayer))
                            {
                                var planet = hit.collider.GetComponent<Planet>();
                                if (planet != null)
                                {
                                    _currentPlanet = planet;
                                    _playMode = PlayMode.PlanetCloseUp;
                                    CameraController.Instance.SwitchToCamera(CameraController.PLANET_CAMERA);
                                    var planetCamera = CameraController.Instance.CurrentCamera;
                                    planetCamera.transform.parent = planet.transform;
                                    planetCamera.transform.position = planet.transform.position + new Vector3(0, 0, -5);
                                    // planetCamera.transform.localRotation = Quaternion.identity;
                                    planetCamera.transform.LookAt(_currentPlanet.transform);
                                    

                                    SetPlanetText(planet);
                                    _growthArrow.rotation = Quaternion.Euler(0.0f, 0.0f, GetGrowthArrowRotation(_currentPlanet));
                                    _planetTextAnimator.SetTrigger(_animFade);
                                    _planetButton.SetActive(false);
                                }
                            }
                        }
                    }
                    //else if(_playMode == PlayMode.Placing)
                    //{
                    //    if(Input.GetMouseButton(0))
                    //    {

                    //    }

                    //    if(Input.GetMouseButtonUp(0))
                    //    {
                    //        //_playMode = PlayMode.MovingAround;
                    //    }
                    //}

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

        QueueCommand(new SpawnStarCommand
        {
            Amount = 20,
            Radius = 50,
            Delay = 0.1f,
            Callback = () =>
{
    _gameMode = GameMode.Playing;

    for (int i = 0; i < 3; i++)
    {
        PlacePlanet();
    }
}
        });
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
        if (_currentCommand == null && _gameCommands.Count > 0)
        {
            _currentCommand = _gameCommands.Dequeue();
            _currentCommand.Execute();
        }

        return _currentCommand != null;
    }
}
