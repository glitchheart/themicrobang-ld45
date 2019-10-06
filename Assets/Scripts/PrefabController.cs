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

public class PrefabController : Controller<PrefabController>
{
    public const int STAR_PREFAB = 0;
    public const int PLANET1_PREFAB = 1;
    public const int PLANET2_PREFAB = 2;
    public const int GEAR_PREFAB = 3;

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

    public GameObject GetPrefabInstance(int index)
    {
        // TODO: Change this to use pools at some point
        return Instantiate(_prefabs[index]);
    }
}
