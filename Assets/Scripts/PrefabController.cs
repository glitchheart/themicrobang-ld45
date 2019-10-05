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

    [SerializeField]
    private GameObject[] _prefabs;

    protected override void OnAwake()
    {
    }

    public GameObject GetPrefabInstance(int index)
    {
        // TODO: Change this to use pools at some point
        return Instantiate(_prefabs[index]);
    }
}
