using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Controller : MonoBehaviour
{
    public float _speed = 1f;
    private Rigidbody2D _rb;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }


    void FixedUpdate()
    {
       if(Mathf.Abs(_rb.velocity.x) < _speed)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _rb.AddForce(Vector2.right * 50f);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _rb.AddForce(- Vector2.right * 50f);
            }
        }
    }
}
