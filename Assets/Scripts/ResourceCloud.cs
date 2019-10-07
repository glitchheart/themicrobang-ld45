using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Planet;

public class ResourceCloud : PrefabObject
{
    public bool Empty => _objects.Count == 0;

    private int _animColor;

    [SerializeField]
    private Color _environmentColor;

    [SerializeField]
    private Color _techColor;

    [SerializeField]
    private MeshRenderer _meshRenderer;

    private Material _material;

    public ResourceType _resourceType;

    private bool _playerTaking;
    private float _takeTimer;

    [Range(0.05f, 1.0f)]
    [SerializeField]
    private float _timePerResource = 0.1f;

    [SerializeField]
    public int Amount = 10;

    private List<Resource> _objects;

    private void Awake()
    {
        _animColor = Shader.PropertyToID("_color");
        _material = _meshRenderer.material;
    }

    public void SpawnWithType(Planet.ResourceType resourceType, int amount = 30)
    {
        _resourceType = resourceType;
        _material.SetColor(_animColor, _resourceType == ResourceType.Environment ? _environmentColor : _techColor);
        Amount = amount;

        SpawnCloud();
    }

    public void SpawnCloud()
    {
        _objects = new List<Resource>();

        for (int i = 0; i < Amount; i++)
        {
            var obj = PrefabController.Instance.GetPrefabInstance<Resource>(_resourceType == ResourceType.Tech ? PrefabType.Gear : PrefabType.Tree);
            obj.transform.position = transform.position + Random.insideUnitSphere;
            obj.transform.parent = transform;
            obj.transform.eulerAngles = new Vector3(Random.Range(-90, 90), 0, 0);
            _objects.Add(obj);
        }
    }

    private void Update()
    {
        transform.Rotate(new Vector3(30 * Time.deltaTime, 0, 0));
        if(_objects != null)
        {
            foreach(var obj in _objects)
            {
                obj.transform.RotateAroundLocal(new Vector3(0, 1, 0), 10 * Time.deltaTime);
                obj.transform.up = Vector3.up;
            }
        }

        if(_playerTaking)
        {
            _takeTimer += Time.deltaTime;
        }
    }

    public Resource Take()
    {
        if(!_playerTaking)
        {
            _takeTimer = 0.0f;
            _playerTaking = true;
        }

        if (_takeTimer >= _timePerResource)
        {
            _takeTimer = 0.0f;

            if (_objects.Count > 0)
            {
                var obj = _objects[0];
                obj.transform.parent = null;
                _objects.Remove(obj);
                return obj;
            }
        }

        return null;
    }

    public void StopTaking()
    {
        _playerTaking = false;
        if(Empty)
        {
            Destroy(gameObject);
        }
    }
}
