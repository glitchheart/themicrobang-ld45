using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;

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
            GameObject go = PrefabController.Instance.GetPrefabInstance(PrefabType.Star);
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
        Credits,
        Intro,
        Playing,
        Paused
    }

    public enum PlayMode
    {
        MovingAround,
        Placing,
        PlanetCloseUp,
        TakingResources
    }

    private List<Spaceship> _spaceShips;

    [SerializeField]
    private RocketCamera _rocketCam;

    [Header("Resources")]
    public int EnvironmentAmount;
    public int TechAmount;

    #region animator hashes
    private int _animFade;
    private int _animPopMenuBar;
    private int _animPopOverview;
    #endregion

    [SerializeField]
    private LayerMask _castingLayers;

    [SerializeField]
    private Universe _universe;

    [SerializeField]
    private Transform _cameraOrbit;

    [SerializeField]
    private GameMode _gameMode;

    private PlayMode _playMode;
    private Planet _currentPlanet;
    private ResourceCloud _currentCloud;

    [SerializeField]
    private TMPro.TextMeshProUGUI _planetText;

    [SerializeField]
    private TMPro.TextMeshProUGUI _planetPopulationText;

    [SerializeField]
    private TMPro.TextMeshProUGUI _planetStateText;

    [SerializeField]
    private TMPro.TextMeshProUGUI _environmentAmount;

    [SerializeField]
    private TMPro.TextMeshProUGUI _techAmount;


    [SerializeField]
    private RectTransform _growthArrow;

    [SerializeField]
    private TMPro.TextMeshProUGUI _techResourceText;

    [SerializeField]
    private TMPro.TextMeshProUGUI _environmentResourceText;

    [SerializeField]
    private GameObject _planetButton;
    private Animator _planetTextAnimator;

    [SerializeField]
    private Animator _menuBarAnimator;

    [SerializeField]
    private Animator _overviewAnimator;

    [SerializeField]
    private PlayableDirector _introPlayable;

    private GameCommand _currentCommand;
    private Queue<GameCommand> _gameCommands;

    #region post-processing
    [SerializeField]
    private PostProcessVolume _postProcessVolume;

    private DepthOfField _depthOfField;
    #endregion

    private Vector3 _lastMousePosition;

    private Camera _mainCamera;

    [SerializeField]
    private float _rotateSpeed = 2.0f;
    private float _scrollSpeed = 2.0f;

    private List<Resource> _resourcesOnTheWay;
    private List<Resource> _resourcesToRemove;

    protected override void OnAwake()
    {
        _mainCamera = Camera.main;
        _animFade = Animator.StringToHash("fade");
        _animPopMenuBar = Animator.StringToHash("pop");
        _animPopOverview = Animator.StringToHash("pop");
        _gameCommands = new Queue<GameCommand>();
        _planetTextAnimator = _planetText.GetComponent<Animator>();
        _depthOfField = _postProcessVolume.profile.GetSetting<DepthOfField>();
    }

    private void Start()
    {
        _spaceShips = new List<Spaceship>();
        _resourcesOnTheWay = new List<Resource>();
        _resourcesToRemove = new List<Resource>();
        _depthOfField.active = false;
        CameraController.Instance.SwitchToCamera(CameraController.INTRO_CAMERA);
    }

    public void PlacePlanet()
    {
        _universe.PlaceNextPlanet();
    }

    private float _resourceTimer = 0.0f;

    private const float INITIAL_RESOURCE_TIMER = 0.2f;
    private const float FAST_RESOURCE_TIMER = 0.05f;
    private const float FASTEST_RESOURCE_TIMER = 0.01f;

    private float _currentResourceTimer = INITIAL_RESOURCE_TIMER;
    private int _totalResourceCount = 0;

    public void ResetResourceChange()
    {
        _resourceTimer = 0.0f;
        _totalResourceCount = 0;
        _currentResourceTimer = INITIAL_RESOURCE_TIMER;
    }

    public void ChangeResource(Planet.ResourceType type, bool take)
    {
        if (_resourceTimer < 0.0f)
        {
            int amount = 5;
            int change = take ? -amount : amount;
            switch (type)
            {
                case Planet.ResourceType.Environment:
                    if ((take && _currentPlanet.Data.EnvironmentResource >= amount) || (!take && EnvironmentAmount >= amount))
                    {
                        _currentPlanet.Data.EnvironmentResource += change;
                        EnvironmentAmount -= change;
                        _currentPlanet.Data.EnvironmentResource = Mathf.Max(0, _currentPlanet.Data.EnvironmentResource);
                    }
                    break;
                case Planet.ResourceType.Tech:
                    if ((take && _currentPlanet.Data.TechResource >= amount) || (!take && TechAmount >= amount))
                    {
                        _currentPlanet.Data.TechResource += change;
                        TechAmount -= change;
                        _currentPlanet.Data.TechResource = Mathf.Max(0, _currentPlanet.Data.TechResource);
                    }
                    break;
            }
            _resourceTimer = _currentResourceTimer;
            _totalResourceCount++;
        }
        else
        {
            _resourceTimer -= Time.deltaTime;

            if (_totalResourceCount > 20)
            {
                _currentResourceTimer = FASTEST_RESOURCE_TIMER;
            }
            else if (_totalResourceCount > 3)
            {
                _currentResourceTimer = FAST_RESOURCE_TIMER;
            }
        }
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

        _planetText.text = planet.Name;
        _planetPopulationText.text = $"{planet.Data.Population}";
        _planetStateText.text = planetState;

        // _planetText.text = $"Name: {planet.Name}\nPopulation: {planet.Data.Population}\nGrowth: {planet.Data.Growth}\n";
        // _planetText.text += $"EvoState: {(planet.Data.EvolutionState == Planet.EvolutionState.Environment ? "Environment" : "Technology")}\n";
        // _planetText.text += $"Planet State: {planetState}\n";
        // _planetText.text += $"Tech: {planet.Data.TechResource}\n";
        // _planetText.text += $"Environment: {planet.Data.EnvironmentResource}\n";

        _environmentAmount.text = $"{planet.Data.EnvironmentResource}";
        _techAmount.text = $"{planet.Data.TechResource}";
    }

    void CollectResource(Resource resource)
    {
        if (resource.ResourceType == Planet.ResourceType.Environment)
            EnvironmentAmount += 5;
        else
            TechAmount += 5;

        _resourcesToRemove.Add(resource);
    }

    public void RemoveShip(Spaceship ship)
    {
        _spaceShips.Remove(ship);
        if(_spaceShips.Count == 0)
        {
            _rocketCam.TurnOff();
        }
    }

    private void Update()
    {
        if (!RunCommands())
        {
            switch (_gameMode)
            {
                case GameMode.Credits:
                    break;
                case GameMode.Intro:
                    if (Input.anyKeyDown)
                    {
                        _introPlayable.Play();
                    }
                    break;
                case GameMode.Playing:
                    if (_resourcesOnTheWay.Count > 0)
                    {
                        foreach (var resource in _resourcesOnTheWay)
                        {
                            if (Vector3.Distance(resource.transform.position, resource.Destination) < 1.0f)
                            {
                                CollectResource(resource);
                            }
                            else
                            {
                                resource.transform.Translate((resource.Destination - resource.transform.position) * 10 * Time.deltaTime);
                            }
                        }

                        foreach (var resource in _resourcesToRemove)
                        {
                            _resourcesOnTheWay.Remove(resource);
                            resource.SelfDestruct();
                        }

                        _resourcesToRemove.Clear();
                    }

                    if (_playMode == PlayMode.PlanetCloseUp)
                    {
                        SetPlanetText(_currentPlanet);
                        _growthArrow.rotation = Quaternion.Euler(0.0f, 0.0f, GetGrowthArrowRotation(_currentPlanet));

                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            _depthOfField.active = false;
                            _planetTextAnimator.SetTrigger(_animFade);
                            _playMode = PlayMode.MovingAround;
                            _menuBarAnimator.SetTrigger(_animPopMenuBar);
                            _currentPlanet = null;
                            CameraController.Instance.SwitchToCamera(CameraController.FIRST_PERSON_CAMERA);
                        }
                    }
                    else if (_playMode == PlayMode.TakingResources)
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            _currentCloud.StopTaking();
                            _currentCloud = null;
                            _playMode = PlayMode.MovingAround;
                        }
                        else
                        {
                            if (_currentCloud.Empty)
                            {
                                _currentCloud.StopTaking();
                                _currentCloud = null;
                                _playMode = PlayMode.MovingAround;
                            }
                            else
                            {
                                var resource = _currentCloud.Take();
                                if (resource != null)
                                {
                                    resource.Destination = _mainCamera.transform.position - _mainCamera.transform.up * 0.1f + _mainCamera.transform.right * UnityEngine.Random.Range(-0.1f, 0.1f);
                                    _resourcesOnTheWay.Add(resource);
                                }
                            }

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
                            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _castingLayers))
                            {
                                var planet = hit.collider.GetComponent<Planet>();
                                var cloud = hit.collider.GetComponent<ResourceCloud>();

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
                                    _depthOfField.active = true;

                                    SetPlanetText(planet);
                                    _growthArrow.rotation = Quaternion.Euler(0.0f, 0.0f, GetGrowthArrowRotation(_currentPlanet));
                                    _planetTextAnimator.SetTrigger(_animFade);
                                    _menuBarAnimator.SetTrigger(_animPopMenuBar);
                                    _planetButton.SetActive(false);
                                }
                                else if (cloud != null)
                                {
                                    _currentCloud = cloud;
                                    _playMode = PlayMode.TakingResources;
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

        _techResourceText.text = $"{TechAmount}";
        _environmentResourceText.text = $"{EnvironmentAmount}";
    }

    public void InstantiateRocketShip(Alien alienThatWantsToTravel)
    {
        var ship = PrefabController.Instance.GetPrefabInstance<Spaceship>(PrefabType.Spaceship);
        _spaceShips.Add(ship);
        ship.transform.position = alienThatWantsToTravel.transform.position;
        ship.transform.up = alienThatWantsToTravel.transform.up;
        alienThatWantsToTravel.transform.parent = alienThatWantsToTravel.transform;
        alienThatWantsToTravel.transform.localPosition = Vector3.zero;
        ship.transform.parent = alienThatWantsToTravel.transform;
        ship.TakeOff(alienThatWantsToTravel.OriginPlanet, _universe.Planets[UnityEngine.Random.Range(0, _universe.Planets.Count)], Spaceship.Intent.Kamikaze);

        _rocketCam.ShowShip(ship.transform);
    }

    public void IntroDone()
    {
        _gameMode = GameMode.Intro;
    }

    public void StartPlayMode()
    {
        HelpUIController.Instance.SetHelpTextEnabled(false);
        CameraController.Instance.SwitchToCamera(CameraController.FIRST_PERSON_CAMERA);

        QueueCommand(new SpawnStarCommand
        {
            Amount = 70,
            Radius = 100,
            Delay = 0.05f,
            Callback = () =>
            {
                _gameMode = GameMode.Playing;

                for (int i = 0; i < 10; i++)
                {
                    PlacePlanet();
                }

                for(int i = 0; i < 4; i++)
                {
                    _universe.SpawnResources(Planet.ResourceType.Environment);
                    _universe.SpawnResources(Planet.ResourceType.Tech);
                }

                CameraController.Instance.CurrentCamera.GetComponent<FirstPersonCamera>().Active = true;

                _overviewAnimator.SetTrigger(_animPopOverview);

                // TODO: Show movement controls!
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
