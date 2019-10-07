using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
        }
        else if (Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }
        OnAwake();
    }

    protected virtual void OnAwake() { }
}
public enum PrefabType
{
    Star = 0,
    Planet1 = 1,
    Planet2 = 2,
    Gear = 3,
    GearCloud = 4,
    Spaceship = 5,
    Building = 6,
    Alien = 7
}
public class PrefabController : Controller<PrefabController>
{
    

    [SerializeField]
    private GameObject[] _prefabs;

    private List<Planet> _planets;

    protected override void OnAwake()
    {
        _planets = new List<Planet>();

        foreach(var prefab in _prefabs)
        {
            var planet = prefab.GetComponent<Planet>();
            if(planet != null)
            {
                _planets.Add(planet);
            }
        }
    }

    public Planet GetRandomPlanet()
    {
        var planet = Instantiate(_planets[Random.Range(0, _planets.Count)]);
        return planet;
    }

    public GameObject GetPrefabInstance(PrefabType type)
    {
        // TODO: Change this to use pools at some point
        return Instantiate(_prefabs[(int)type]);
    }
}
