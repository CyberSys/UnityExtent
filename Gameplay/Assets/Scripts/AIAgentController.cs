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
    private List<Vector3> keyPatrolTargets;
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

    public bool CreateNewPatrol(List<Vector3> keyPatrolTargets)
    {
        if(GameObject.Find("Debug Console"))
            debugConsole = GameObject.Find("Debug Console").GetComponent<Text>();
        
        if (keyPatrolTargets.Count > 0)
        {
            List<Transform> keyPatrolTargetsTransforms = new List<Transform>();

            foreach (var patrolTarget in keyPatrolTargets)
            {
                GameObject pathPosition = new GameObject();
                pathPosition.transform.position = GridController.GetCellFromWorldPosition(patrolTarget).GetCentre();
                keyPatrolTargetsTransforms.Add(pathPosition.transform);
            }
            
            patrol = new Patrol(keyPatrolTargetsTransforms, GetStartingCell().transform.position, GetStartingMovementDirection());
            
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
    
    public void OnPathFound(List<Cell> pathCells, bool pathSuccessful) {
        if (pathSuccessful)
        {
            patrol.SetPatrolPath(new Path(pathCells));
        
            // if (patrolTargetIndex == patrol.GetCurrentPatrolIndex())
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

    public void SetKeyPatrolTargets(List<Vector3> _keyPatrolTargets)
    {
        keyPatrolTargets = _keyPatrolTargets;
        
        CreateNewPatrol(keyPatrolTargets);
    }

    IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }
        
        // Following call is only ever calculating a path the the first target, not every target in the list, it's broken atm!
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
                for (int i = 0; i < activePath.pathCells.Count; i++)
                {
                    if (currentCell.GetCentre() == activePath.pathCells[i].GetCentre())
                        //|| activePath.pathCells.Contains(currentCell))
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
                else
                {
                    ChangePath();
                }
            }

            yield return null;
        }
    }
}
