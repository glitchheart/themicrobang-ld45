using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Planet;

public class ResourceCloud : PrefabObject
{
    public ResourceType _resourceType;

    [SerializeField]
    public int Amount = 10;

    private GameObject[] _objects;

    private void Start()
    {
        _resourceType = Random.Range(0, 50) > 50 ? ResourceType.Environment : ResourceType.Tech;
        SpawnCloud(Amount);
    }

    public void SpawnCloud(int amount)
    {
        _objects = new GameObject[amount];

        for (int i = 0; i < amount; i++)
        {
            var obj = PrefabController.Instance.GetPrefabInstance(_resourceType == ResourceType.Tech ? PrefabType.Gear : PrefabType.Tree);
            obj.transform.position = transform.position + Random.insideUnitSphere;
            obj.transform.parent = transform;
            obj.transform.eulerAngles = new Vector3(Random.Range(-90, 90), 0, 0);
            _objects[i] = obj;
        }
    }

    private void Update()
    {
        if(_objects != null)
        {
            foreach(var obj in _objects)
            {
                obj.transform.RotateAroundLocal(new Vector3(1, 0, 0), 3.0f * Time.deltaTime);
            }
        }
    }
}
