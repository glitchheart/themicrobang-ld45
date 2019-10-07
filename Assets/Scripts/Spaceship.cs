using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : PrefabObject
{
    public enum Intent
    {
        None,
        GiveEnvironment,
        GiveTech,
        TakeEnvironment,
        TakeTech,
        Kamikaze
    }

    public enum State
    {
        Idle,
        TravelingOutgoing,
        TravelingIngoing
    }

    [SerializeField]
    private float _speed = 20.0f;

    [SerializeField]
    private ParticleSystem _particleSystem;

    private State _state;

    public Alien Alien { get; set; }
    public Intent TravelIntent { get; private set; }
    public Planet OriginPlanet { get; private set; }
    public Planet DestinationPlanet { get; private set; }

    private Vector3 _currentDirection;

    private float _timeBeforeNewDecision;
    private float _currentTime;

    private float _destructDelay;

    private int _takenAmount;

    public RocketCamera RocketCamera;

    public void TakeOff(Planet originPlanet, Planet destinationPlanet, Intent intent)
    {
        OriginPlanet = originPlanet;
        DestinationPlanet = destinationPlanet;
        TravelIntent = intent;
        transform.position += transform.up * 1.0f;
        _particleSystem.Stop();

        if (intent == Intent.GiveEnvironment)
        {
            _takenAmount = Random.Range(10, 30);
            OriginPlanet.Data.EnvironmentResource -= _takenAmount;
        }
        else if (intent == Intent.GiveTech)
        {
            _takenAmount = Random.Range(10, 30);
            OriginPlanet.Data.TechResource -= _takenAmount;
        }

        StartCoroutine(DelayTakeOff());
    }

    IEnumerator DelayTakeOff()
    {
        yield return new WaitForSeconds(2.0f);
        transform.parent = null;
        _state = State.TravelingOutgoing;
        _particleSystem.Play();
        _timeBeforeNewDecision = 0.5f;
        _currentDirection = (DestinationPlanet.transform.position - transform.position).normalized;
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Idle:
                break;
            case State.TravelingOutgoing:
            case State.TravelingIngoing:
                if (_currentTime >= _timeBeforeNewDecision)
                {
                    var dest = _state == State.TravelingIngoing ? OriginPlanet : DestinationPlanet;
                    if (Random.Range(0, 100) < 10)
                    {
                        var destDir = (dest.transform.position - transform.position).normalized;
                        _currentDirection = destDir + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f)).normalized;
                    }
                    else
                    {
                        _currentDirection = (dest.transform.position - transform.position).normalized;
                    }

                    _timeBeforeNewDecision = Random.Range(0.3f, 4.0f);
                    _currentTime = 0.0f;
                }

                var dir = Vector3.RotateTowards(transform.up, _currentDirection, 2.0f * Time.deltaTime, 2.0f * Time.deltaTime);
                transform.up = dir;
                transform.position += transform.up * _speed * Time.deltaTime;
                _currentTime += Time.deltaTime;

                if (Vector3.Distance(OriginPlanet.transform.position, DestinationPlanet.transform.position) < 3.0f)
                {
                    //Destroy(gameObject);
                }

                _destructDelay += Time.deltaTime;
                break;
        }
    }

    [SerializeField]
    private Font font;

    private GUIStyle _style;

    [SerializeField]
    private Texture _gear;


    [SerializeField]
    private Texture _environment;

    void Awake()
    {
        _style = new GUIStyle();
        _style.normal.textColor = Color.white;
        _style.fontSize = 15;
        _style.font = font;
    }

    void OnGUI()
    {
        var pos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.0f - Vector3.right * 2.0f);

        if (pos.z >= 0.0f)
        {
            string travelText = "";
            string intentText = "";

            switch (_state)
            {
                case Spaceship.State.TravelingOutgoing:
                    travelText = $"Flying to {DestinationPlanet.Name}";
                    break;
                case Spaceship.State.TravelingIngoing:
                    travelText = $"Flying home to {OriginPlanet.Name}";
                    break;
            }

            Texture texToDraw = null;

            switch (TravelIntent)
            {
                case Spaceship.Intent.GiveEnvironment:
                    intentText += $"Carrying {_takenAmount}";
                    texToDraw = _environment;
                    break;
                case Spaceship.Intent.TakeEnvironment:
                    texToDraw = _environment;
                    intentText += $"Carrying {_takenAmount}";
                    break;
                case Spaceship.Intent.GiveTech:
                    intentText += $"Carrying {_takenAmount}";
                    texToDraw = _gear;
                    break;
                case Spaceship.Intent.TakeTech:
                    intentText += $"Carrying {_takenAmount}";
                    texToDraw = _gear;
                    break;
                case Spaceship.Intent.Kamikaze:
                    intentText += "With no peaceful intentions";
                    break;
            }

            GUI.Label(new Rect(pos.x, Screen.height - pos.y, 200, 200), travelText, _style);
            GUI.Label(new Rect(pos.x, Screen.height - pos.y + 30, 200, 200), intentText, _style);

            if (texToDraw != null)
            {
                GUI.Label(new Rect(pos.x + 80, Screen.height - pos.y + 25, 30, 30), texToDraw, _style);
            }
        }
    }

    private void Explode()
    {
        var explosion = PrefabController.Instance.GetPrefabInstance<Explosion>(PrefabType.Explosion);
        explosion.transform.position = transform.position;
        explosion.Explode();
        OriginPlanet.DespawnAlien(Alien);
        GameController.Instance.RemoveShip(this);

        if (RocketCamera.transform.parent == transform)
        {
            RocketCamera.transform.parent = null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_state == State.TravelingOutgoing || _state == State.TravelingIngoing) && _destructDelay > 2.0f)
        {
            var planet = other.GetComponent<Planet>();
            if (planet == DestinationPlanet && _state == State.TravelingOutgoing)
            {
                switch (TravelIntent)
                {
                    case Intent.GiveEnvironment:
                        DestinationPlanet.Data.EnvironmentResource += _takenAmount;
                        _takenAmount = 0;
                        break;
                    case Intent.GiveTech:
                        DestinationPlanet.Data.TechResource += _takenAmount;
                        _takenAmount = 0;
                        break;
                    case Intent.TakeEnvironment:
                        _takenAmount = Random.Range(10, 30);
                        DestinationPlanet.Data.EnvironmentResource -= _takenAmount;
                        break;
                    case Intent.TakeTech:
                        _takenAmount = Random.Range(10, 30);
                        DestinationPlanet.Data.TechResource -= _takenAmount;
                        break;
                    case Intent.Kamikaze:
                        DestinationPlanet.Data.TechResource -= Random.Range(0, 20);
                        DestinationPlanet.Data.EnvironmentResource -= Random.Range(0, 20);
                        Explode();
                        break;
                }
                _state = State.TravelingIngoing;
            }
            else if (planet == OriginPlanet && _state == State.TravelingIngoing)
            {
                switch (TravelIntent)
                {
                    case Intent.TakeEnvironment:
                        OriginPlanet.Data.EnvironmentResource += _takenAmount;
                        break;
                    case Intent.TakeTech:
                        OriginPlanet.Data.TechResource += _takenAmount;
                        _takenAmount = 0;
                        break;
                }
                _state = State.Idle;
                if(RocketCamera.transform.parent == transform)
                {
                    RocketCamera.transform.parent = null;
                }
                Destroy(gameObject);
            }
            else
            {
                Explode();
            }
            _destructDelay = 0.0f;
        }
    }
}
