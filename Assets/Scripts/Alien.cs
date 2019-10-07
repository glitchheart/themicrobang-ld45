using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : PrefabObject
{
    public enum AlienState
    {
        Idle,
        Walking,
        Applauding,
        Angry,
        InShip
    }

    #region animator hashes
    private int _animIdle;
    private int _animWalk;
    private int _animYay;
    #endregion

    public Planet OriginPlanet;

    [SerializeField]
    private AlienState _state;

    private AlienState State
    {
        set
        {
            _timer = 0.0f;
            _state = value;
            if (_state == AlienState.Idle)
            {
                _animator.SetTrigger(_animIdle);
            }
            else if (_state == AlienState.Walking)
            {
                transform.Rotate(new Vector3(0, Random.Range(-90, 90), 0), Space.Self);
                _animator.SetTrigger(_animWalk);
                _currentTimeToWalk = Random.Range(_minTimeToWalk, _maxTimeToWalk);
            }
            else if (_state == AlienState.Applauding)
            {
                _animator.SetTrigger(_animYay);
            }
        }
    }

    [Header("Applause settings")]
    [SerializeField]
    private float _timeToApplaud = 3.0f;

    [Header("Walking time settings")]
    [SerializeField]
    private float _minTimeToWalk = 1.0f;

    [SerializeField]
    private float _maxTimeToWalk = 10.0f;

    private float _currentTimeToWalk;

    private float _timer;

    [HideInInspector]
    [SerializeField]
    private Animator _animator;

    private void OnValidate()
    {
        if(_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    private void Awake()
    {
        _animIdle = Animator.StringToHash("idle");
        _animWalk = Animator.StringToHash("walk");
        _animYay = Animator.StringToHash("yay");
    }

    private Spaceship.Intent GetIntent()
    {
        var environment = OriginPlanet.Data.EnvironmentResource;
        var tech = OriginPlanet.Data.TechResource;

        if(environment < Planet.RESOURCE_LOW)
        {
            return Spaceship.Intent.TakeEnvironment;
        }

        if(tech < Planet.RESOURCE_LOW)
        {
            return Spaceship.Intent.TakeTech;
        }

        if(environment > Planet.RESOURCE_HIGH)
        {
            return Spaceship.Intent.GiveEnvironment;
        }

        if(tech > Planet.RESOURCE_HIGH)
        {
            return Spaceship.Intent.GiveTech;
        }

        return Spaceship.Intent.None;
    }

    private void Update()
    {
        switch(_state)
        {
            case AlienState.Idle:
                if(Random.Range(0, 100) < 5)
                {
                    State = AlienState.Walking;
                }
                break;
            case AlienState.Walking:
                float distance = Vector3.Distance(transform.position, transform.parent.position);
                transform.position = transform.parent.position;
                transform.Rotate(new Vector3(0, 0, 10 * Time.deltaTime), Space.Self);
                transform.position = transform.parent.position + transform.up * distance;

                if(_timer >= _currentTimeToWalk)
                {
                    int rand = Random.Range(0, 1000);

                    if (rand < 20)
                    {
                        Spaceship.Intent intent = GetIntent();
                        if(intent != Spaceship.Intent.None)
                        {
                            OriginPlanet.Data.TechResource -= 10;
                            GameController.Instance.InstantiateRocketShip(this, intent);
                            _state = AlienState.InShip;
                        }
                    }
                    else if (rand < 100)
                    {
                        State = AlienState.Applauding;
                    }
                    else if (rand < 200)
                    {
                        State = AlienState.Idle;
                    }
                }
                break;
            case AlienState.Applauding:
                if(_timer >= _timeToApplaud)
                {
                    State = AlienState.Idle;
                }
                break;
            case AlienState.Angry:
                break;
        }

        _timer += Time.deltaTime;
    }
}
