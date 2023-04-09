using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using System.Security.Policy;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AIAgentController : AgentController
{
    // public int currentPatrolTarget = 0;
    // public List<Transform> patrolTargets = new List<Transform>();
    public float speed = 2;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 1;

    private Patrol patrol;
    private Path activePath;

    // private Path currentPatrol;
    // private List<Path> patrolPaths;
    private List<Vector3> keyPatrolTargets;
    private List<Vector3> originalKeyPatrolTargets;
    // private List<Vector3> startDirections;
    // private bool patrolJoined = false;
    
    private bool patrolling = false;
    private bool waitingForPath = false;

    private Text debugConsole;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    private void SetDebugConsole(string output)
    {
        if (debugConsole != null)
            debugConsole.text = output;
    }

    public bool CreateNewPatrol(List<GameObject> keyPatrolTargets)
    {
        if(GameObject.Find("Debug Console"))
            debugConsole = GameObject.Find("Debug Console").GetComponent<Text>();
        
        if (keyPatrolTargets.Count > 0)
        {
            patrol = new Patrol(keyPatrolTargets, GetStartingCell().transform.position, GetStartingMovementDirection());
            
            StartCoroutine(UpdatePath());

            return true;
        }

        return false;
    }
    public void StartPatrol()
    {
        originalKeyPatrolTargets = keyPatrolTargets;
        patrolling = true;
    }

    public void StopPatrol()
    {
        StopCoroutine(UpdatePath ());
    }

    public void Redirect(Cell problemCell)
    {
        if (waitingForPath)
            return;

        waitingForPath = true;
        
        List<Vector3> targetPositions = new List<Vector3>(originalKeyPatrolTargets);

        for (int i = 0; i < targetPositions.Count; i++)
        {
            Cell targetCell = GridController.GetCellFromWorldPosition(targetPositions[i]);
            if (targetCell != null &&
                targetCell.IsOccupiedByAnotherAgent(base.ID))
            {
                targetPositions[i] = GridController.GetRandomAccessibleNeighbour(targetCell, GetMovementDirection(), null).GetCentre();
                break;
                // i--;
            }
        }

        Vector3 originalStartingPosition = targetPositions[0];

        targetPositions[0] = GetCurrentCell().transform.position;
        
        Cell newCellDivertion = GridController.GetRandomAccessibleNeighbour(GetCurrentCell(), GetMovementDirection(), problemCell);

        if (newCellDivertion != null)
        {
            targetPositions.Insert(1,newCellDivertion.transform.position);
            targetPositions.Add(GetCurrentCell().transform.position);

            SetKeyPatrolTargets(targetPositions);
            Vector3 newStartingDirection = GetMovementDirection();
            SetStartingMovementDirection(newStartingDirection);
            UpdatePath();
        }
        else
        {
            throw new InvalidOperationException("Cannot find diversion.");
        }
    }
    
    public void OnPathFound(List<Cell> pathCells, bool pathSuccessful) {
        if (pathSuccessful)
        {
            waitingForPath = false;
            patrol.SetPatrolPath(new Path(pathCells));
            ChangePath();
        }
        else
        {
            throw new InvalidOperationException("Failed to find successful path.");
        }
    }

    

    // Update is called once per frame
    public override void FixedUpdate()
    {
        if (patrolling)
        {
            List<Vector3> points = patrol.GetPathPoints();

            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }

        if(GetCurrentCell() != null && patrol != null)
            base.FixedUpdate();
    }

    public void SetKeyPatrolTargets(List<Vector3> _keyPatrolTargets)
    {
        keyPatrolTargets = _keyPatrolTargets;

        List<GameObject> keyPatrolTargetObjects = new List<GameObject>();
        
        for (int i = 0; i < keyPatrolTargets.Count; i++)
        {
            GameObject pathPosition = new GameObject();
            pathPosition.transform.parent = this.transform;
            pathPosition.transform.position = GridController.GetCellFromWorldPosition(keyPatrolTargets[i]).GetCentre();
            keyPatrolTargetObjects.Add(pathPosition);
        }
        
        CreateNewPatrol(keyPatrolTargetObjects);
    }

    IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }

        waitingForPath = true;
        PathRequestManager.RequestPath(new PathRequest(base.ID, this.GetStartingMovementDirection(), keyPatrolTargets, OnPathFound));
    }

    private Path ClonePatrol(Path patrolPath)
    {
        List<Cell> copiedCells = new List<Cell>();

        foreach (var cell in patrolPath.pathCells)
        {
            copiedCells.Add(cell);
        }
        
        return new Path(copiedCells);
    }

    private void ChangePath()
    {
        activePath = ClonePatrol(patrol.GetPatrolPath());
        
        StopCoroutine("FollowPath");
        StartCoroutine("FollowPath");
    }

    IEnumerator FollowPath()
    {
        bool followingPath = true;

        while (followingPath && activePath.pathCells.Count > 0)
        {
            Cell currentCell = GetCurrentCell();
            float distanceToCurrentCellCentre = Vector3.Distance(currentCell.GetCentre(), transform.position);
            
            bool movingTowards = isMovingTowards(currentCell.GetCentre(), transform.position, GetComponent<Rigidbody>().velocity);

            if (distanceToCurrentCellCentre < 0.05f || !movingTowards)
            {
                if (currentCell.GetCentre() == activePath.pathCells[0].GetCentre())
                {
                    Cell reorderedCell = activePath.pathCells[0];
                    activePath.pathCells.RemoveAt(0);
                    activePath.pathCells.Add(reorderedCell);
                }

                if (activePath.pathCells.Count > 0)
                {
                    Vector3 directionChange = activePath.pathCells[0].GetCentre() - currentCell.GetCentre();

                    Vector3 currentMovementDirection = GetMovementDirection();
                    
                    if (directionChange != Vector3.zero && directionChange != currentMovementDirection)
                    {
                        if (directionChange == Vector3.right)
                        {
                            if (currentMovementDirection == Vector3.forward)
                                ChangeMovementDirection(MovementAction.Movement.TurnRight);
                            else if (currentMovementDirection == Vector3.back)
                                ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                            else
                            {
                                throw new InvalidOperationException("Illegal direction change.");
                            }
                        }
                        else if (directionChange == Vector3.left)
                        {
                            if (currentMovementDirection == Vector3.forward)
                                ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                            else if (currentMovementDirection == Vector3.back)
                                ChangeMovementDirection(MovementAction.Movement.TurnRight);
                            else
                            {
                                throw new InvalidOperationException("Illegal direction change.");
                            }
                        }
                        else if (directionChange == Vector3.forward)
                        {
                            if (currentMovementDirection == Vector3.right)
                                ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                            else if (currentMovementDirection == Vector3.left)
                                ChangeMovementDirection(MovementAction.Movement.TurnRight);
                            else
                            {
                                throw new InvalidOperationException("Illegal direction change.");
                            }
                        }
                        else if (directionChange == Vector3.back)
                        {
                            if (currentMovementDirection == Vector3.right)
                                ChangeMovementDirection(MovementAction.Movement.TurnRight);
                            else if (currentMovementDirection == Vector3.left)
                                ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                            else
                            {
                                throw new InvalidOperationException("Illegal direction change.");
                            }
                        }
                    }
                }
                else
                {
                    ChangePath();
                }
            }
            
            for (int i = 0; i < activePath.pathCells.Count; i++)
            {
                Cell pathCell = activePath.pathCells[i];
                if (pathCell != null)
                {
                    pathCell.SetAIAgentETA(i, this);
                }
                else
                {
                    int k = 0;
                }
            }

            yield return null;
        }
    }
}
