using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AgentController : MonoBehaviour
{
    private Rigidbody Rigidbody;
    private BoxCollider BoxCollider;

    private Vector3 MovementDirection = Vector3.zero;

    public float Speed = 1.0f;
    public float Tilt = 1.0f;

    private GridController GridController;

    public enum Direction
    {
        Left,
        Right,
        Forward,
        Back
    }

    public struct MovementAction
    {
        public MovementAction(Vector3 currentDirection, KeyCode keyPressed)
        {
            NewDirection = Vector3.zero;
            
            if (currentDirection == Vector3.left)
            {
                if (keyPressed == KeyCode.A)
                {
                    NewDirection = Vector3.down;
                }

                if (keyPressed == KeyCode.D)
                {
                    NewDirection = Vector3.up;
                }
            }
            if(currentDirection == Vector3.forward)
            {
                if (keyPressed == KeyCode.A)
                {
                    NewDirection = Vector3.left;
                }

                if (keyPressed == KeyCode.D)
                {
                    NewDirection = Vector3.right;
                }
            }
            if(currentDirection == Vector3.right)
            {
                if (keyPressed == KeyCode.A)
                {
                    NewDirection = Vector3.forward;
                }

                if (keyPressed == KeyCode.D)
                {
                    NewDirection = Vector3.down;
                }
            }
            if(currentDirection == Vector3.down)
            {
                if (keyPressed == KeyCode.A)
                {
                    NewDirection = Vector3.right;
                }

                if (keyPressed == KeyCode.D)
                {
                    NewDirection = Vector3.left;
                }
            }
        }

        public Vector3 NewDirection { get; }
    }

    private List<MovementAction> movementActions;
    
    public struct Cell
    {
        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public override string ToString() => $"({X}, {Y})";
    }

    public Cell startCell = new Cell(0,0);

    // Start is called before the first frame update
    private void Start()
    {
        Rigidbody = gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
        BoxCollider = gameObject.GetComponent(typeof(BoxCollider)) as BoxCollider;
        
        SetMovementDirection(Vector3.forward);
        
        GridController = GameObject.FindObjectOfType<GridController>();
    }

    public void SetStartingCell(int x, int y)
    {
        if (GridController == null)
        {
            GridController = GameObject.FindObjectOfType<GridController>();
        }
        transform.position = GridController.GetCell(x,y).centre;
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
            if(MovementDirection == Vector3.left)
                newMovementDirection += Vector3.down;
            if(MovementDirection == Vector3.forward)
                newMovementDirection += Vector3.left;
            if(MovementDirection == Vector3.right)
                newMovementDirection += Vector3.forward;
            if(MovementDirection == Vector3.down)
                newMovementDirection += Vector3.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            if(MovementDirection == Vector3.left)
                newMovementDirection += Vector3.forward;
            if(MovementDirection == Vector3.forward)
                newMovementDirection += Vector3.right;
            if(MovementDirection == Vector3.right)
                newMovementDirection += Vector3.down;
            if(MovementDirection == Vector3.down)
                newMovementDirection += Vector3.left;
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
        
        if(MovementDirection == Vector3.right)
            transform.rotation = Quaternion.Euler(0,90,0);
        if(MovementDirection == Vector3.left)
            transform.rotation = Quaternion.Euler(0,-90,0);
        if(MovementDirection == Vector3.forward)
            transform.rotation = Quaternion.Euler(0,0,0);
        if(MovementDirection == Vector3.down)
            transform.rotation = Quaternion.Euler(0,180,0);
        
        
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

        //GetComponent<Rigidbody>().rotation = Quaternion.Euler (0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -Tilt);
    }
}
