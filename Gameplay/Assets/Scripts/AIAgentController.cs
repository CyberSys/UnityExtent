using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class AIAgentController : AgentController
{
    const float minPathUpdateTime = .4f;
    const float pathUpdateMoveThreshold = .5f;

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
    // private List<Vector3> startPositions;
    // private List<Vector3> startDirections;
    // private bool patrolJoined = false;
    
    private bool patrolling = false;

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

    public bool CreateNewPatrol(List<Transform> patrolTargets)
    {
        if(GameObject.Find("Debug Console"))
            debugConsole = GameObject.Find("Debug Console").GetComponent<Text>();
        
        if (patrolTargets.Count > 0)
        {
            patrol = new Patrol(patrolTargets, GetStartingCell().transform.position, GetStartingMovementDirection());
            
            StartCoroutine(UpdatePath());

            return true;
        }

        return false;
    }
    public void StartPatrol()
    {
        patrolling = true;
    }

    public void StopPatrol()
    {
        StopCoroutine(UpdatePath ());
    }
    
    public void OnPathFound(List<Cell> pathCells, bool pathSuccessful, int patrolTargetIndex) {
        if (pathSuccessful)
        {
            patrol.SetPatrolPath(patrolTargetIndex, new Path(pathCells));

            if (patrolTargetIndex > 0)
            {
                Cell startingCell = GridController.GetCellFromWorldPosition(patrol.GetStartPosition(patrolTargetIndex));

                Vector3 startingMovementDirection = patrol.GetStartDirection(patrolTargetIndex);
                
                patrol.CheckPreviousPathConnectivity(GridController, startingCell, startingMovementDirection, patrolTargetIndex);
            }
            
            if(patrol.IsPatrolReady())
                patrol.CheckCurrentPathConnectivity(GridController, GetStartingCell(), GetStartingMovementDirection());

            if (patrolTargetIndex == patrol.GetCurrentPatrolIndex())
            {
                ChangePath();
            }
        }
        // else
        // {
        //     if (patrolTargetIndex == currentPatrolTarget)
        //     {
        //         NextPatrol();
        //         
        //         PathRequestManager.RequestPath(new PathRequest(base.ID, currentPatrolTarget, GetMovementDirection(),
        //             GetCurrentCell().GetCentre(),
        //             patrolTargets[currentPatrolTarget].position, OnPathFound));
        //     }
        // }
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
    
    IEnumerator UpdatePath() {
        if (Time.timeSinceLevelLoad < .3f) {
            yield return new WaitForSeconds (.3f);
        }

        PathRequestManager.RequestPath (new PathRequest(base.ID,0, GetStartingMovementDirection(), GetStartingCell().transform.position, patrol.GetPatrolTarget(0).position, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 currentTargetPosition = patrol.GetCurrentPatrolTarget().position;

        while (true) {
            yield return new WaitForSeconds (minPathUpdateTime);
            for (var i = 0; i < patrol.GetNumberOfPatrolTargets(); i++)
            {
                //print(((patrol.GetCurrentTarget().position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);

                bool readyToRequest = false; // protecting against requesting a path before we have enough information
    
                if (i > 0 && patrol.IsPathReady(i-1))
                {
                    patrol.SetStartPosition(i, patrol.GetPatrolTarget(i-1).transform.position);
                    
                    readyToRequest = true;
                }
                else if (i == 0)
                {
                    readyToRequest = true;
                }

                Cell currentCell = GetCurrentCell();

                Cell currentPatrolTargetCell =
                    GridController.GetCellFromWorldPosition(patrol.GetCurrentPatrolTarget().position);

                if(i == patrol.GetCurrentPatrolIndex() && currentCell.gridPosition == currentPatrolTargetCell.gridPosition)
                {
                    NextPatrol();

                    ChangePath();
                }
                
                if (readyToRequest && (patrol.GetPatrolTarget(i).position - currentTargetPosition).sqrMagnitude > sqrMoveThreshold)
                {
                    if (i != patrol.GetCurrentPatrolIndex())
                    {
                        PathRequestManager.RequestPath(new PathRequest(base.ID, i, patrol.GetStartDirection(i),
                            patrol.GetStartPosition(i), patrol.GetPatrolTarget(i).position, OnPathFound));
                    }
                    else
                    {
                        PathRequestManager.RequestPath(new PathRequest(base.ID, i, GetMovementDirection(),
                            patrol.GetStartPosition(i),
                            GetCurrentCell().GetCentre(),
                            patrol.GetPatrolTarget(i).position, OnPathFound));
                    }
                    currentTargetPosition = patrol.GetPatrolTarget(i).position;
                }
            }
        }
    }

    private void NextPatrol()
    {
        patrol.IncrementCurrentPatrolPathhIndex();
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
        activePath = ClonePatrol(patrol.GetCurrentPatrolPath());
        
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
                for (int i = 0; i < activePath.pathCells.Count; i++)
                {
                    if (currentCell.GetCentre() == activePath.pathCells[i].GetCentre()
                        || activePath.pathCells.Contains(currentCell))
                    {
                        activePath.pathCells.RemoveAt(i);
                        i--;
                        // Debug.Log(patrolPaths[currentPatrolTarget].pathCells.Count);
                    }
                    else
                    {
                        break;
                    }
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
            }

            yield return null;
        }
    }
}
