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

    void Awake()
    {
        _style = new GUIStyle();
        _style.normal.textColor = Color.white;
        _style.fontSize = 20;
        _style.font = font;
    }

    void OnGUI()
    {
        var pos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.0f);

        string text = "";

        switch(_state)
        {
            case Spaceship.State.TravelingOutgoing:
            text = $"Flying to {DestinationPlanet.Name}";
            break;
            case Spaceship.State.TravelingIngoing:
            text = $"Flying home to {OriginPlanet.Name}";
            break;
        }

        switch(TravelIntent)
        {
            case Spaceship.Intent.GiveEnvironment:
            case Spaceship.Intent.TakeEnvironment:
            text += $"\nCarrying {_takenAmount} environment";
            break;
            case Spaceship.Intent.GiveTech:
            case Spaceship.Intent.TakeTech:
            text += $"\nCarrying {_takenAmount} tech";
            break;
            case Spaceship.Intent.Kamikaze:
            text += "\nWith no peaceful intentions";
            break;
        }

        GUI.Label(new Rect(pos.x, Screen.height - pos.y, 200, 200), text, _style);
    }

    private void Explode()
    {
        var explosion = PrefabController.Instance.GetPrefabInstance<Explosion>(PrefabType.Explosion);
        explosion.transform.position = transform.position;
        explosion.Explode();
        OriginPlanet.DespawnAlien(Alien);
        GameController.Instance.RemoveShip(this);
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
