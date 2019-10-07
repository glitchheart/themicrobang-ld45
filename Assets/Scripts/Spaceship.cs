using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : PrefabObject
{
    public enum Intent
    {
        GiveEnvironment,
        GiveTech,
        TakeEnvironment,
        TakeTech,
        Kamikaze
    }

    public enum State
    {
        Idle,
        Traveling
    }

    [SerializeField]
    private float _speed = 20.0f;

    [SerializeField]
    private ParticleSystem _particleSystem;

    private State _state;

    public Intent TravelIntent { get; private set; }
    public Planet OriginPlanet { get; private set; }
    public Planet DestinationPlanet { get; private set; }

    private Vector3 _currentDirection;

    private float _timeBeforeNewDecision;
    private float _currentTime;

    private float _destructDelay;

    public void TakeOff(Planet originPlanet, Planet destinationPlanet, Intent intent)
    {
        OriginPlanet = originPlanet;
        DestinationPlanet = destinationPlanet;
        TravelIntent = intent;
        transform.position += transform.up * 1.0f;
        _particleSystem.Stop();
        StartCoroutine(DelayTakeOff());
    }

    IEnumerator DelayTakeOff()
    {
        yield return new WaitForSeconds(2.0f);
        transform.parent = null;
        _state = State.Traveling;
        _particleSystem.Play();
        _timeBeforeNewDecision = 0.5f;
        _currentDirection = (DestinationPlanet.transform.position - transform.position).normalized;
    }

    private void Update()
    {
        switch(_state)
        {
            case State.Idle:
                break;
            case State.Traveling:
                if (_currentTime >= _timeBeforeNewDecision)
                {
                    if(Random.Range(0, 100) < 10)
                    {
                        var destDir = (DestinationPlanet.transform.position - transform.position).normalized;
                        _currentDirection = destDir + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f)).normalized;
                    }
                    else
                    {
                        _currentDirection = (DestinationPlanet.transform.position - transform.position).normalized;
                    }

                    _timeBeforeNewDecision = Random.Range(0.3f, 4.0f);
                    _currentTime = 0.0f;
                }

                var dir = Vector3.RotateTowards(transform.up, _currentDirection, 2.0f * Time.deltaTime, 2.0f * Time.deltaTime);
                transform.up = dir;
                transform.position += transform.up *_speed * Time.deltaTime;
                _currentTime += Time.deltaTime;

                if(Vector3.Distance(OriginPlanet.transform.position, DestinationPlanet.transform.position) < 3.0f)
                {
                    //Destroy(gameObject);
                }

                _destructDelay += Time.deltaTime;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(_state == State.Traveling && _destructDelay > 2.0f)
        {
            var explosion = PrefabController.Instance.GetPrefabInstance<Explosion>(PrefabType.Explosion);
            explosion.transform.position = transform.position;
            explosion.Explode();
            GameController.Instance.RemoveShip(this);
            Destroy(gameObject);
        }
    }
}
