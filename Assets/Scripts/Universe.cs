using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Universe : MonoBehaviour
{
    [SerializeField]
    private string[] _planetNames;

    private Planet[] _planets;
    private int _ringCount;

    [SerializeField]
    private int _maxRings = 10;

    public Planet PlaceNextPlanet()
    {
        if(_ringCount < _maxRings)
        {
            var planet = PrefabController.Instance.GetRandomPlanet();
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
            planet.transform.position = dir * (8 + _ringCount++ * 3.0f);
            planet.Orbit(Vector3.zero, Random.Range(10, 20));

            return planet;
        }

        return null;
    }
}
