using UnityEngine;

public class Player2D : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    private Rigidbody2D _rb;
    private Vector2 _velocity;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        _velocity = new Vector2(x, y).normalized * _speed;
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
    }
}
