using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AgentController : MonoBehaviour
{
    public float speed;
    public float tilt;

    private Rigidbody Rigidbody;
    private BoxCollider BoxCollider;

    private Vector3 MovementDirection = Vector3.zero;

    private float Speed = 1.0f;
    private float Tilt = 1.0f;

    private void Start()
    {
        Rigidbody = gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
        BoxCollider = gameObject.GetComponent(typeof(BoxCollider)) as BoxCollider;
        
        SetMovementDirection(Vector3.forward);
    }

    public void SetMovementDirection(Vector3 movement)
    {
        MovementDirection = movement;
    }
    
    Vector3 GetInputTranslationDirection()
    {
        Vector3 newMovementDirection = Vector3.zero;
        // if (Input.GetKey(KeyCode.W))
        // {
        //     direction += Vector3.forward;
        // }
        // if (Input.GetKey(KeyCode.S))
        // {
        //     direction += Vector3.back;
        // }
        if (Input.GetKey(KeyCode.A))
        {
            newMovementDirection += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            newMovementDirection += Vector3.right;
        }
        // if (Input.GetKey(KeyCode.Q))
        // {
        //     direction += Vector3.down;
        // }
        // if (Input.GetKey(KeyCode.E))
        // {
        //     direction += Vector3.up;
        // }
        return newMovementDirection;
    }

    void FixedUpdate ()
    {
        if (GetInputTranslationDirection() != Vector3.zero)
            MovementDirection = GetInputTranslationDirection();
        // float moveHorizontal = Input.GetAxis ("Horizontal");
        // float moveVertical = Input.GetAxis ("Vertical");
        //
        // Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
        GetComponent<Rigidbody>().velocity = MovementDirection * Speed;

        GetComponent<Rigidbody>().position = new Vector3 
        (
            Mathf.Clamp (GetComponent<Rigidbody>().position.x, BoxCollider.bounds.min.x, BoxCollider.bounds.max.x), 
            0.0f, 
            Mathf.Clamp (GetComponent<Rigidbody>().position.z, BoxCollider.bounds.min.z, BoxCollider.bounds.max.z)
        );

        GetComponent<Rigidbody>().rotation = Quaternion.Euler (0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -Tilt);
    }
}
