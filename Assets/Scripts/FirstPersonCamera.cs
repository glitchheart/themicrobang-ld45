using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [SerializeField]
    public bool Active = true;

    [SerializeField]
    private float _movementSpeed = 20.0f;

    [SerializeField]
    private float _speedMultiplier = 2.0f;

    #region mouse look
    public float lookX;
    public float lookY;
    public float Speed = 5f;
    #endregion

    void Update()
    {
        if (!Active)
            return;

        UpdateLookRotation();

        Vector3 dir = Vector3.zero;

        if(Input.GetKey(KeyCode.W))
        {
            dir.y = 1.0f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            dir.y = -1.0f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            dir.x = -1.0f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            dir.x = 1.0f;
        }

        dir.Normalize();

        float speed = Input.GetKey(KeyCode.LeftShift) ? _movementSpeed * _speedMultiplier : _movementSpeed;

        transform.Translate(dir.y * transform.forward * speed * Time.deltaTime);
        transform.Translate(dir.x * transform.right * speed * Time.deltaTime);
    }

    void UpdateLookRotation()
    {
        if (!Input.GetMouseButton(1))
            return;

        lookX = Input.GetAxis("Mouse X") * Speed;
        lookY = Input.GetAxis("Mouse Y") * Speed;

        Vector3 rot = new Vector3(-lookY, lookX, 0);
        transform.Rotate(rot);
    }
}
