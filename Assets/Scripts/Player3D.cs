using System;
using UnityEngine;

public class Player3D : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    private Rigidbody _rb;
    private Vector3 _velocity;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
    }

    private void Update()
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        _velocity = new Vector3(x, 0, y).normalized * _speed;
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
    }
}
