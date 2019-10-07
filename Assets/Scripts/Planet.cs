﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : PrefabObject
{
    public enum PlanetState
    {
        Balanced,
        Critical,
        Extinction,
        Desperation,
        Prosperous
    }

    public enum GrowthState
    {
        VeryHigh,
        High,
        Medium,
        Low,
        VeryLow
    }

    public enum ResourceType
    {
        Tech,
        Environment
    }

    public enum EvolutionState
    {
        Technology,
        Environment
    }

    public struct PlanetData
    {
        public int Growth;
        public int Population;
        public PlanetState State;
        public ResourceType ResourceType;
        public EvolutionState EvolutionState;
        public int TechResource;
        public int EnvironmentResource;
    }

    [SerializeField]
    private float _radius = 1.0f;

    public string Name;
    // public int Population;
    public PlanetData Data;

    public List<Alien> _aliens;
    public List<Building> _buildings;
    private List<Bush> _bushes;

    private GameObject _child;

    private Vector3 _center;
    private float _speed;

    public WarningArrow WarningArrow;

    private float _timeBeforePopulationGrow = 2.0f;
    private float _time;

    public const int VERY_HIGH_GROWTH = 20;
    public const int HIGH_GROWTH = 10;
    public const int MEDIUM_GROWTH = 5;
    public const int LOW_GROWTH = 3;
    public const int VERY_LOW_GROWTH = 1;

    public const int RESOURCE_VERY_HIGH = 1000;
    public const int RESOURCE_HIGH = 500;
    public const int RESOURCE_LOW = 100;
    public const int RESOURCE_VERY_LOW = 10;

    private void Awake()
    {
        _aliens = new List<Alien>();
        _buildings = new List<Building>();
        _bushes = new List<Bush>();
    }

    public GrowthState GetGrowthState()
    {
        int growth = Data.Growth;

        if (growth > VERY_HIGH_GROWTH)
        {
            return GrowthState.VeryHigh;
        }

        if (growth > HIGH_GROWTH)
        {
            return GrowthState.High;
        }

        if (growth > MEDIUM_GROWTH)
        {
            return GrowthState.Medium;
        }

        if (growth > LOW_GROWTH)
        {
            return GrowthState.Low;
        }

        return GrowthState.VeryLow;
    }

    public void Orbit(Vector3 center, float speed = 5.0f)
    {
        //_child = new GameObject("Planet child");
        //_child
        _center = center;
        _speed = speed;
    }

    private void Update()
    {
        transform.RotateAround(_center, Vector3.up, _speed * Time.deltaTime);

        if(Data.State != PlanetState.Extinction)
        {
            Evolve();
        }

        _time += Time.deltaTime;
    }

    void SpawnBuilding()
    {
        var building = PrefabController.Instance.GetPrefabInstance(PrefabType.Building);
        Vector3 dir = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
        building.transform.position = transform.position + dir * _radius;
        building.transform.up = dir;
        building.transform.parent = transform;
        _buildings.Add(building.GetComponent<Building>());
    }

    void DespawnBuilding()
    {
        if (_buildings.Count == 0)
            return;

        var building = _buildings[Random.Range(0, _buildings.Count)];
        // TODO: Dust or something? Confetti?
        Destroy(building.gameObject);
    }

    void SpawnBush()
    {
        var building = PrefabController.Instance.GetPrefabInstance(PrefabType.Bush);
        Vector3 dir = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
        building.transform.position = transform.position + dir * _radius;
        building.transform.up = dir;
        building.transform.parent = transform;
        _bushes.Add(building.GetComponent<Bush>());
    }

    void DespawnBush()
    {
        if (_bushes.Count == 0)
            return;

        var bush = _bushes[Random.Range(0, _bushes.Count)];
        // TODO: Dust or something? Confetti?
        Destroy(bush.gameObject);
    }

    void SpawnAlien()
    {
        var alien = PrefabController.Instance.GetPrefabInstance<Alien>(PrefabType.Alien);
        Vector3 dir = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
        alien.transform.position = transform.position + dir * _radius;
        alien.transform.up = dir;
        alien.transform.parent = transform;
        alien.OriginPlanet = this;
        _aliens.Add(alien);
    }

    void DespawnAlien()
    {
        if (_aliens.Count == 0)
            return;

        var alien = _aliens[Random.Range(0, _aliens.Count)];
        // TODO: Dust or something? Confetti?
        Destroy(alien.gameObject);
    }

    void Evolve()
    {
        // TODO: Add more stuff. States?
        if (_time > _timeBeforePopulationGrow)
        {
            int prev = Data.Population;
            Data.Population += Data.Growth;
            int populationDiff = Data.Population - prev;

            Data.Population = Mathf.Max(0, Data.Population);

            if(populationDiff > 0 && Data.EnvironmentResource > 30)
            {
                SpawnAlien();
                Data.EnvironmentResource -= 5;
            }
            else if(populationDiff > 0)
            {
                DespawnAlien();
            }

            _time = 0.0f;

            float techChance = Data.TechResource > Data.EnvironmentResource ? 0.9f : 0.1f;
            float environmentChance = 1.0f - techChance;

            int highDec = 5;
            int lowDec = 1;

            if (Random.Range(0.0f, 1.0f) < techChance)
            {
                Data.EvolutionState = EvolutionState.Technology;
                Data.EnvironmentResource -= lowDec;
                Data.TechResource -= highDec;
            }
            else
            {
                Data.EvolutionState = EvolutionState.Environment;
                Data.EnvironmentResource -= highDec;
                Data.TechResource -= lowDec;
            }

            Data.EnvironmentResource = Mathf.Max(0, Data.EnvironmentResource);
            Data.TechResource = Mathf.Max(0, Data.TechResource);

            if (Data.EnvironmentResource < RESOURCE_VERY_LOW && Data.TechResource < RESOURCE_VERY_LOW)
            {   
                Data.State = Planet.PlanetState.Extinction;
            }
            else if (Data.EnvironmentResource < RESOURCE_LOW && Data.TechResource < RESOURCE_LOW)
            {
                Data.State = Planet.PlanetState.Desperation;
                if (Random.Range(0.0f, 1.0f) < 0.02f)
                {
                    DespawnBuilding();
                }
            }
            else if (Data.EnvironmentResource < RESOURCE_HIGH && Data.TechResource < RESOURCE_HIGH)
            {
                Data.State = Planet.PlanetState.Critical;
                WarningArrow.gameObject.SetActive(true);
            }
            else if (Data.EnvironmentResource < RESOURCE_VERY_HIGH && Data.TechResource < RESOURCE_VERY_HIGH)
            {
                Data.State = Planet.PlanetState.Balanced;
                WarningArrow.gameObject.SetActive(false);
                SpawnBuilding();

                if(Random.Range(0.0f, 1.0f) < 0.02f)
                {
                    SpawnBuilding();
                }
                else if(Random.Range(0.0f, 1.0f) < 0.02f)
                {
                    SpawnBush();
                }
            }

            float growthRandom = Random.Range(0.0f, 1.0f);

            switch (Data.State)
            {
                case Planet.PlanetState.Balanced:
                    if (growthRandom > 0.7f)
                    {
                        Data.Growth += 1;
                    }
                    else if (growthRandom < 0.2f)
                    {
                        Data.Growth -= 1;
                    }
                    break;
                case Planet.PlanetState.Prosperous:
                    if (growthRandom > 0.7f)
                    {
                        Data.Growth += 2;
                    }
                    else if (growthRandom < 0.05f)
                    {
                        Data.Growth -= 1;
                    }
                    break;
                case Planet.PlanetState.Critical:
                    if (growthRandom > 0.7f)
                    {
                        Data.Growth -= 1;
                        DespawnBush();
                    }
                    else if (growthRandom < 0.2f)
                    {
                        Data.Growth += 1;
                    }
                    break;
                case Planet.PlanetState.Desperation:
                    if (growthRandom > 0.8f)
                    {
                        Data.Growth -= 2;
                    }
                    else if (growthRandom < 0.1f)
                    {
                        Data.Growth += 1;
                    }
                    break;
                case Planet.PlanetState.Extinction:
                    Data.Growth = 0;
                    break;
                default:
                    break;
            }
        }

    }
}
