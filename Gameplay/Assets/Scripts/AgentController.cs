using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    
    private GridCell.GridPosition _startCell = new GridCell.GridPosition(0,0);

    public GridCell.GridPosition GetStartingCell()
    {
        return _startCell;
    }
    
    public void SetStartingCell(int x, int y)
    {
        if (GridController == null)
        {
            GridController = GameObject.FindObjectOfType<GridController>();
        }
        transform.position = GridController.GetCell(x,y).centre;

        SetCurrentCell(x, y);
    }

    private GridCell _currentCell;

    public GridCell GetCurrentCell()
    {
        return _currentCell;
    }

    public void SetCurrentCell(int x, int y)
    {
        if(_currentCell != null && _currentCell.agentsInCell.Count > 0)
            _currentCell.agentsInCell.Remove(this);
        _currentCell = GridController.GetCell(x, y);
        _currentCell.agentsInCell.Add(this);
        
        if(MovementDirection == Vector3.left)
            SetNextCell(x-1, y);
        if(MovementDirection == Vector3.forward)
            SetNextCell(x, y+1);
        if(MovementDirection == Vector3.right)
            SetNextCell(x+1, y);
        if(MovementDirection == Vector3.down)
            SetNextCell(x, y-1);
        
        //_nextCell
    }
    
    private GridCell _nextCell;

    public GridCell GetNextCell()
    {
        return _nextCell;
    }

    public void SetNextCell(int x, int y)
    {
        _nextCell = GridController.GetCell(x, y);
        _nextCell.ToggleNextCellIndicator();
    }

    // Start is called before the first frame update
    private void Start()
    {
        Rigidbody = gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
        BoxCollider = gameObject.GetComponent(typeof(BoxCollider)) as BoxCollider;
        
        SetMovementDirection(Vector3.forward);
        
        GridController = GameObject.FindObjectOfType<GridController>();
    }

    
    public void SetMovementDirection(Vector3 movement)
    {
        MovementDirection = movement;
    }
    
    Vector3 GetInputTranslationDirection()
    {
        Vector3 newMovementDirection = MovementDirection;
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
                newMovementDirection = Vector3.down;
            if(MovementDirection == Vector3.forward)
                newMovementDirection = Vector3.left;
            if(MovementDirection == Vector3.right)
                newMovementDirection = Vector3.forward;
            if(MovementDirection == Vector3.down)
                newMovementDirection = Vector3.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            if(MovementDirection == Vector3.left)
                newMovementDirection = Vector3.forward;
            if(MovementDirection == Vector3.forward)
                newMovementDirection = Vector3.right;
            if(MovementDirection == Vector3.right)
                newMovementDirection = Vector3.down;
            if(MovementDirection == Vector3.down)
                newMovementDirection = Vector3.left;
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
    
    //Detect collisions between the GameObjects with Colliders attached
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player entered: " + other.gameObject.name);
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Player left: " + other.gameObject.name);
    }

    void FixedUpdate ()
    {
        if (GetInputTranslationDirection() != Vector3.zero)
            SetMovementDirection(GetInputTranslationDirection());
        
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

        GetComponent<Rigidbody>().rotation = Quaternion.Euler (0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -Tilt);
    }
}
