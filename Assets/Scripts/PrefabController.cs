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
    Star,
    Planet,
    Gear,
    ResourceCloud,
    Spaceship,
    Building,
    Alien,
    Tree,
    UniverseCircle,
    Explosion,
    WarningArrow
}

public class PrefabController : Controller<PrefabController>
{
    private Dictionary<PrefabType, List<GameObject>> _prefabMap;

    [SerializeField]
    private PrefabObject[] _prefabs;

    protected override void OnAwake()
    {
        _prefabMap = new Dictionary<PrefabType, List<GameObject>>();

        foreach(var prefab in _prefabs)
        {
            if(!_prefabMap.ContainsKey(prefab.Type))
            {
                _prefabMap.Add(prefab.Type, new List<GameObject>());
            }

            _prefabMap[prefab.Type].Add(prefab.gameObject);
        }
    }

    public Planet GetRandomPlanet()
    {
        var planet = Instantiate(GetPrefabInstance<Planet>(PrefabType.Planet));
        return planet;
    }

    public GameObject GetPrefabInstance(PrefabType type)
    {
        // TODO: Change this to use pools at some point
        var list = _prefabMap[type];

        var randomPrefab = list[Random.Range(0, list.Count)];
        return Instantiate(randomPrefab);
    }

    public T GetPrefabInstance<T>(PrefabType type) where T : PrefabObject
    {
        return GetPrefabInstance(type).GetComponent<T>();
    }
}
