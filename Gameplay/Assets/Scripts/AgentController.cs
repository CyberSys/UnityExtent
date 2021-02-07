using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class AgentAction
{
    public List<KeyCode> KeyBindings;

    public bool InputActive;

    public bool InputChanged;
    
    public AgentAction(List<KeyCode> keyBindings)
    {
        KeyBindings = keyBindings;
        InputActive = false;
        InputChanged = false;
    }

    public override string ToString()
    {
        return "Agent Action.";
    }

    public void Update()
    {
        if (!InputActive) // don't look for new down if already down
        {
            foreach (var keyCode in KeyBindings)
            {
                if (Input.GetKey(keyCode))
                {
                    // Debug.Log(this.ToString() +" Key Down.");
                    InputActive = true;
                    InputChanged = true;
                    break;
                }
            }
        }
        else
        {
            int keybindUp = KeyBindings.Count;
            foreach (var keyCode in KeyBindings)
            {
                if (Input.GetKey(keyCode))
                {
                    keybindUp--;
                }
            }

            if (keybindUp == KeyBindings.Count && !InputChanged)
            {
                // Debug.Log(this.ToString() +" Key Up.");
                InputActive = false;
                InputChanged = true;
            }
            else
            {
                InputChanged = false;
            }
        }
    }
}

public class MovementAction : AgentAction
{
    public enum Movement {Forward, TurnLeft, TurnRight}

    public Movement action;
    
    public MovementAction(List<KeyCode> keyBindings, Movement movementAction) : base(keyBindings)
    {
        action = movementAction;
    }
    
    public override string ToString()
    {
        switch (action)
        {
            case Movement.TurnLeft:
                return "Movement Action: Turn Left.";
            case Movement.TurnRight:
                return "Movement Action: Turn Right.";
            default:
                return "Unknown Movement Action.";
        }
    }
}

public class CombatAction : AgentAction
{
    public enum Combat {Weapon0,Weapon1}

    public Combat action;
    public CombatAction(List<KeyCode> keyBindings, Combat combatAction) : base(keyBindings)
    {
        action = combatAction;
    }
}
public class InputController
{
    public List<AgentAction> Actions = new List<AgentAction>();

    public List<MovementAction.Movement> MovementQueue = new List<MovementAction.Movement>();

    public InputController()
    {
        Actions.Add(new MovementAction(new List<KeyCode>(new KeyCode[] {KeyCode.A, KeyCode.LeftArrow}), MovementAction.Movement.TurnLeft));
        Actions.Add(new MovementAction(new List<KeyCode>(new KeyCode[] {KeyCode.D, KeyCode.RightArrow}), MovementAction.Movement.TurnRight));
    }
        
    public void Update()
    {
        foreach (var agentAction in Actions)
        {
            agentAction.Update();
                
            // process movement actions
            if (agentAction is MovementAction agentMovementAction)
            {
                if (agentMovementAction.InputActive && agentMovementAction.InputChanged)
                {
                    MovementQueue.Add(agentMovementAction.action);
                    Debug.Log("Added " + agentMovementAction.ToString() +" to the movement queue.");
                }
            }
                
            // process combat actions
            if(agentAction is CombatAction agentCombatAction)
            {
                    
            }
        }
    }

    public MovementAction.Movement ProccessMovementQueue()
    {
        int movementDirection = 0;

        foreach (var movement in MovementQueue)
        {
            if (movement == MovementAction.Movement.TurnLeft)
                movementDirection--;
            if (movement == MovementAction.Movement.TurnRight)
                movementDirection++;
        }
        
        MovementQueue.Clear();

        if (movementDirection < 0) // turn left
        {
            return MovementAction.Movement.TurnLeft;
        }

        if (movementDirection > 0) // turn right
        {
            return MovementAction.Movement.TurnRight;
        }
        
        return MovementAction.Movement.Forward;
    }
}

public class AgentController : PersistableObject
{
    private Rigidbody Rigidbody;
    private BoxCollider BoxCollider;

    private Vector3 MovementDirection = Vector3.zero;
    
    private Vector3 NextMovementDirection = Vector3.zero;

    public float Speed = 1.0f;
    public float Tilt = 1.0f;

    private GridController GridController;
    
    // private InputController _inputController;
    
    private Cell.GridPosition _startCell = new Cell.GridPosition(0,0);

    public Text DebugText;

    public Cell.GridPosition GetStartingCell()
    {
        return _startCell;
    }
    
    public virtual void SetStartingCell(int x, int y)
    {
        if (GridController == null)
        {
            GridController = GameObject.FindObjectOfType<GridController>();
        }
        transform.position = GridController.GetCell(x,y).GetCentre();

        SetCurrentCell(x, y);
        _currentCell.SetSpawn(true);
    }

    private Cell _currentCell;
    private bool currentCellChanged = false;

    public virtual Cell GetCurrentCell()
    {
        return _currentCell;
    }

    public bool CurrentCellChanged()
    {
        return currentCellChanged;
    }

    public void ResetCurrentCellChanged()
    {
        currentCellChanged = false;
    }

    public virtual void SetCurrentCell(int x, int y)
    {
        if (_currentCell == null || _currentCell.gridPosition.X != x && _currentCell.gridPosition.Y != y)
        {
            _currentCell = GridController.GetCell(x, y);
            _currentCell.agentsInCell.Add(this);

            if (MovementDirection == Vector3.left)
                SetNextCell(x - 1, y);
            if (MovementDirection == Vector3.forward)
                SetNextCell(x, y + 1);
            if (MovementDirection == Vector3.right)
                SetNextCell(x + 1, y);
            if (MovementDirection == Vector3.back)
                SetNextCell(x, y - 1);

            currentCellChanged = true;
        }
    }
    
    public virtual void SetCurrentCell(Cell currentCell)
    {
        if (_currentCell == null 
            || _currentCell.gridPosition.X != currentCell.gridPosition.X
            || _currentCell.gridPosition.Y != currentCell.gridPosition.Y)
        {
            if (_currentCell != null)
            {
                if (_currentCell.IsSpawn())
                {
                    _currentCell.SetSpawn(false);
                }

                SetPreviousCell(_currentCell);
            }
            
            _currentCell = currentCell;
            _currentCell.agentsInCell.Add(this);

            if (MovementDirection == Vector3.left)
                SetNextCell(_currentCell.gridPosition.X - 1, _currentCell.gridPosition.Y);
            if (MovementDirection == Vector3.forward)
                SetNextCell(_currentCell.gridPosition.X, _currentCell.gridPosition.Y + 1);
            if (MovementDirection == Vector3.right)
                SetNextCell(_currentCell.gridPosition.X + 1, _currentCell.gridPosition.Y);
            if (MovementDirection == Vector3.back)
                SetNextCell(_currentCell.gridPosition.X, _currentCell.gridPosition.Y - 1);

            currentCellChanged = true;
        }
    }
    
    private Cell _nextCell;

    public virtual Cell GetNextCell()
    {
        return _nextCell;
    }

    public virtual void SetNextCell(int x, int y)
    {
        // reset previous next cell
        if(_nextCell!= null)
            _nextCell.SetNextCellIndicator(false);
        
        _nextCell = GridController.GetCell(x, y);
        _nextCell.SetNextCellIndicator(true);
    }
    
    public virtual void SetNextCell(Cell nextCell)
    {
        // reset previous next cell
        if(_nextCell!= null)
            _nextCell.SetNextCellIndicator(false);
        
        _nextCell = nextCell;
        _nextCell.SetNextCellIndicator(true);
    }
    
    private Cell _previousCell;

    public virtual Cell GetPreviousCell()
    {
        return _previousCell;
    }

    public virtual void SetPreviousCell(int x, int y)
    {
        _previousCell = GridController.GetCell(x, y);
    }
    
    public virtual void SetPreviousCell(Cell previousCell)
    {
        _previousCell = previousCell;
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        // _inputController = new InputController();
        
        Rigidbody = gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
        BoxCollider = gameObject.GetComponent(typeof(BoxCollider)) as BoxCollider;
        
        SetMovementDirection(Vector3.forward);
        
        GridController = GameObject.FindObjectOfType<GridController>();

        if (this.gameObject.layer == 9) // AI
        {
            if(gameObject.GetComponent<NavMeshAgent>())
                gameObject.GetComponent<NavMeshAgent>().SetDestination(new Vector3(0.5f, 0.0f, 0.5f));
        }
    }

    public Vector3 GetMovementDirection()
    {
        return MovementDirection;
    }

    public virtual void AdjustPosition()
    {
        if (MovementDirection == Vector3.right)
            GetComponent<Rigidbody>().position = new Vector3(GetComponent<Rigidbody>().position.x, GetComponent<Rigidbody>().position.y, _currentCell.GetCentre().z);
        if(MovementDirection == Vector3.left)
            GetComponent<Rigidbody>().position = new Vector3(GetComponent<Rigidbody>().position.x, GetComponent<Rigidbody>().position.y, _currentCell.GetCentre().z);
        if(MovementDirection == Vector3.forward)
            GetComponent<Rigidbody>().position = new Vector3(_currentCell.GetCentre().x, GetComponent<Rigidbody>().position.y, GetComponent<Rigidbody>().position.z);
        if(MovementDirection == Vector3.back)
            GetComponent<Rigidbody>().position = new Vector3(_currentCell.GetCentre().x, GetComponent<Rigidbody>().position.y, GetComponent<Rigidbody>().position.z);
    }
    public virtual void ChangeMovementDirection(MovementAction.Movement newMovement)
    {
        Cell.GridPosition currentGridPosition = GridController.GetCellFromWorldPosition(transform.position).gridPosition;
        if (MovementDirection == Vector3.forward)
        {
            if (newMovement == MovementAction.Movement.TurnLeft)
            {
                SetMovementDirection(Vector3.left);
                SetNextCell(currentGridPosition.X-1, currentGridPosition.Y);
            }

            if (newMovement == MovementAction.Movement.TurnRight)
            {
                SetMovementDirection(Vector3.right);
                SetNextCell(currentGridPosition.X+1, currentGridPosition.Y);
            }
            return;
        }
        
        if (MovementDirection == Vector3.back)
        {
            if (newMovement == MovementAction.Movement.TurnLeft)
            {
                SetMovementDirection(Vector3.right);
                SetNextCell(currentGridPosition.X+1, currentGridPosition.Y);
            }

            if (newMovement == MovementAction.Movement.TurnRight)
            {
                SetMovementDirection(Vector3.left);
                SetNextCell(currentGridPosition.X-1, currentGridPosition.Y);
            }
            return;
        }
        
        if (MovementDirection == Vector3.left)
        {
            if (newMovement == MovementAction.Movement.TurnLeft)
            {
                SetMovementDirection(Vector3.back);
                SetNextCell(currentGridPosition.X, currentGridPosition.Y-1);
            }

            if (newMovement == MovementAction.Movement.TurnRight)
            {
                SetMovementDirection(Vector3.forward);
                SetNextCell(currentGridPosition.X, currentGridPosition.Y+1);
            }
            return;
        }
        
        if (MovementDirection == Vector3.right)
        {
            if (newMovement == MovementAction.Movement.TurnLeft)
            {
                SetMovementDirection(Vector3.forward);
                SetNextCell(currentGridPosition.X, currentGridPosition.Y+1);
            }

            if (newMovement == MovementAction.Movement.TurnRight)
            {
                SetMovementDirection(Vector3.back);
                SetNextCell(currentGridPosition.X, currentGridPosition.Y-1);
            }
            return;
        }
    }
    public virtual void SetMovementDirection(Vector3 movement)
    {
        if (MovementDirection != movement)
        {
            MovementDirection = movement;
        }
    }
    
    //Detect collisions between the GameObjects with Colliders attached
    
    // TODO this needs changing
    // it is too quickly moving from current cell to next cell
    // there should be a check to make sure that the player is more than 
    // half way into next slide, before changing next to current
    // and current to previous
    void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Player entered: " + other.gameObject.name);
        
        // should also check based on direction travelling
        // at the moment turning close to the corner of a cell
        // could mean entering 3 cells and this would be wrong
        // if (_nextCell != null && _nextCell.IsNextCell())
        // {
        //     _nextCell.SetNextCellIndicator();
        //     _nextCell.agentsInCell.Remove(this);
        // }
        //
        // SetNextCell(other.gameObject.GetComponent<Cell>());
        
        // bool movingTowards = isMovingTowards(_currentCell.GetCentre(), transform.position, GetComponent<Rigidbody>().velocity);
        // enter new cells
    }

    void OnTriggerExit(Collider other)
    {
        // Debug.Log("Player left: " + other.gameObject.name);

        // Cell triggerCell = other.gameObject.GetComponent<Cell>();
        //
        // if (triggerCell == _currentCell && !triggerCell.IsSpawn())
        // {
        //     throw new InvalidOperationException("Player leaving grid.");
        // }
    }
    
    private bool isMovingTowards(Vector3 testPoint, Vector3 objectPosition, Vector3 objectVelocty)
    {    
        Vector3 toPoint = testPoint - objectPosition; //a vector going from your obect to the point
        float result = Vector3.Dot(toPoint, objectVelocty);
        return result >= 0;
    }
    public virtual void FixedUpdate ()
    {
        SetCurrentCell(GridController.GetCellFromWorldPosition(transform.position));
        // if (this.tag == "Player" && _currentCell != null)
        // {
        //     _inputController.Update();
        // }

        GetComponent<Rigidbody>().velocity = MovementDirection * Speed;

        GetComponent<Rigidbody>().position = new Vector3
        (
            Mathf.Clamp(GetComponent<Rigidbody>().position.x, BoxCollider.bounds.min.x, BoxCollider.bounds.max.x),
            0.075f,
            Mathf.Clamp(GetComponent<Rigidbody>().position.z, BoxCollider.bounds.min.z, BoxCollider.bounds.max.z)
        );

        //GetComponent<Rigidbody>().rotation = Quaternion.Euler (0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -Tilt);

        if (MovementDirection == Vector3.right)
            transform.rotation = Quaternion.Euler(0, 90, 0);
        if (MovementDirection == Vector3.left)
            transform.rotation = Quaternion.Euler(0, -90, 0);
        if (MovementDirection == Vector3.forward)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        if (MovementDirection == Vector3.back)
            transform.rotation = Quaternion.Euler(0, 180, 0);

        float distanceToCurrentCellCentre = Vector3.Distance(_currentCell.GetCentre(), transform.position);
        float distanceToNextCellCentre = Vector3.Distance(_nextCell.GetCentre(), transform.position);
        
        AdjustPosition();

        /*if (distanceToNextCellCentre < distanceToCurrentCellCentre)
        {
            // leave previous current and next
            // this should be shifted into the update where there is a check to see
            // if the player is more than half way into the cell
            // and if the player is traveling in it's direction
            if (_currentCell != null)
            {
                if (_currentCell.IsSpawn())
                {
                    _currentCell.SetSpawn(false);
                }

                SetPreviousCell(_currentCell);
            }

            SetCurrentCell(_nextCell);
        }*/

        if (DebugText != null)
        {
            // DebugText.text = GetComponent<Rigidbody>().position.ToString();
            // DebugText.text += "\n " + _currentCell.GetCentre();

            // DebugText.text = "( " + _currentCell.GetCentre().x + ", " + _currentCell.GetCentre().y + ", " + _currentCell.GetCentre().z + " )";
            DebugText.text = _currentCell.gridPosition.ToString();
        }
    }
}
