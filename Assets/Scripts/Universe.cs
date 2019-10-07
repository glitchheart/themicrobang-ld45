using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Universe : MonoBehaviour
{
    [SerializeField]
    private string[] _planetNames;

    public List<Planet> Planets;
    private int _ringCount;

    [SerializeField]
    private int _maxRings = 10;

    private void Start()
    {
        Planets = new List<Planet>();
    }

    public void SpawnResources(Planet.ResourceType resourceType)
    {
        var cloud = PrefabController.Instance.GetPrefabInstance<ResourceCloud>(PrefabType.ResourceCloud);
        cloud.SpawnWithType(resourceType);
        cloud.transform.position = new Vector3(Random.Range(-90.0f, 90.0f), Random.Range(-90.0f, 90.0f), Random.Range(-90.0f, 90.0f));
    }

    public Planet PlaceNextPlanet()
    {
        if(_ringCount < _maxRings)
        {
            var planet = PrefabController.Instance.GetRandomPlanet();
            Planets.Add(planet);
            planet.Name = _planetNames[Random.Range(0, _planetNames.Length)];
            planet.Data.State = Planet.PlanetState.Balanced;

            if(Random.Range(0.0f, 1.0f) > 0.5f)
            {
                planet.Data.ResourceType = Planet.ResourceType.Environment;
                planet.Data.TechResource = 500;
                planet.Data.EnvironmentResource = 1000;
            }
            else
            {
                planet.Data.ResourceType = Planet.ResourceType.Tech;
                planet.Data.TechResource = 1000;
                planet.Data.EnvironmentResource = 500;
            }

            float startingGrowth = Random.Range(0.0f, 1.0f);

            if(startingGrowth > 0.0f && startingGrowth < 0.2f)
            {
                planet.Data.Growth = Planet.VERY_HIGH_GROWTH;
            }
            else if(startingGrowth > 0.2f && startingGrowth < 0.4f)
            {
                planet.Data.Growth = Planet.HIGH_GROWTH;
            }
            else if(startingGrowth > 0.4f && startingGrowth < 0.6f)
            {
                planet.Data.Growth = Planet.MEDIUM_GROWTH;
            }
            else if(startingGrowth > 0.6f && startingGrowth < 0.8f)
            {
                planet.Data.Growth = Planet.LOW_GROWTH;
            }
            else if(startingGrowth > 0.8f && startingGrowth < 1.0f)
            {
                planet.Data.Growth = Planet.VERY_LOW_GROWTH;
            }

            Vector3 dir = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)).normalized;
            planet.transform.position = dir * (_ringCount * 10.0f);
            planet.Orbit(Vector3.zero, Random.Range(10, 20));

            var circle = PrefabController.Instance.GetPrefabInstance(PrefabType.UniverseCircle);
            circle.transform.position = Vector3.zero;
            float width = (_ringCount) * 20.0f;
            circle.transform.localScale = new Vector3(width, width, width);

            _ringCount++;

            return planet;
        }

        return null;
    }
}