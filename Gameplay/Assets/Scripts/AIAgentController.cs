using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class AIAgentController : AgentController
{
    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;

    public Transform startPosition;

    public int currentPatrolTarget = 0;
    public List<Transform> patrolTargets = new List<Transform>();
    public float speed = 2;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 1;

    private Path currentPatrol;
    private List<Path> patrolPaths;
    private List<Vector3> startPositions;
    private List<Vector3> startDirections;
    private bool patrolJoined = false;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    public void StartPatrol()
    {
        if (patrolTargets.Count > 0)
        {   
            patrolPaths = new List<Path>();
            startPositions = new List<Vector3>();
            startDirections = new List<Vector3>();
            
            for (int i = 0; i < patrolTargets.Count; i++)
            {
                startPositions.Add(GetCurrentCell().transform.position);
                startDirections.Add(GetMovementDirection());
                patrolPaths.Add(new Path(new List<Cell>(),startPositions[i], 0.0f,0.0f));
            }
            
            StartCoroutine(UpdatePath());
        }
    }

    public void StopPatrol()
    {
        StopCoroutine(UpdatePath ());
    }
    
    public void OnPathFound(List<Cell> waypoints, bool pathSuccessful, int patrolTargetIndex) {
        if (pathSuccessful) {
            patrolPaths[patrolTargetIndex] = new Path(waypoints, startPositions[patrolTargetIndex], turnDst, stoppingDst);

            if (patrolTargetIndex == currentPatrolTarget)
            {
                ChangePath();
            }

            if (currentPatrol.lookPoints.Count > 0 && patrolTargets.Count > 0 && currentPatrolTarget == patrolTargets.Count - 1)
            {
                Cell lastCell = currentPatrol.lookPoints[currentPatrol.lookPoints.Count - 1];

                Vector3 finalMovementDirection = CalculateStartingMovementDirection(0);

                bool isJoinedUp = patrolJoined || GetStartingCell().IsAccessibleFromCell(lastCell, finalMovementDirection) 
                                  || (GetStartingCell().gridPosition == lastCell.gridPosition && GetStartingMovementDirection() == finalMovementDirection);

                if (!isJoinedUp)
                {

                    List<Cell> accessibleNeighbours;

                    Cell pickedNeighbour = null;

                    if (GetStartingMovementDirection() == Vector3.forward ||
                        GetStartingMovementDirection() == Vector3.back)
                    {
                        if (GetStartingMovementDirection() == Vector3.forward)
                        {
                            accessibleNeighbours =
                                GridController.GetAccessibleNeighbours(GetStartingCell(), Vector3.left);

                            pickedNeighbour = CheckNeighbours(accessibleNeighbours, true);
                            
                            accessibleNeighbours =
                                GridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.back);
                            
                            pickedNeighbour = CheckNeighbours(accessibleNeighbours, false);
                        }
                        else
                        {
                            accessibleNeighbours =
                                GridController.GetAccessibleNeighbours(GetStartingCell(), Vector3.right);
                            
                            pickedNeighbour = CheckNeighbours(accessibleNeighbours, true);
                            
                            accessibleNeighbours =
                                GridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.forward);
                            
                            pickedNeighbour = CheckNeighbours(accessibleNeighbours, false);
                        }
                    }
                    else
                    {
                        if (GetStartingMovementDirection() == Vector3.left)
                        {
                            accessibleNeighbours =
                                GridController.GetAccessibleNeighbours(GetStartingCell(), Vector3.back);
                            
                            pickedNeighbour = CheckNeighbours(accessibleNeighbours, false);
                            
                            accessibleNeighbours =
                                GridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.right);
                            
                            pickedNeighbour = CheckNeighbours(accessibleNeighbours, true);
                        }
                        else
                        {
                            accessibleNeighbours =
                                GridController.GetAccessibleNeighbours(GetStartingCell(), Vector3.forward);
                            
                            pickedNeighbour = CheckNeighbours(accessibleNeighbours, false);
                            
                            accessibleNeighbours =
                                GridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.left);
                            
                            pickedNeighbour = CheckNeighbours(accessibleNeighbours, true);
                        }
                    }
                    
                    if (pickedNeighbour != null)
                    {
                        startPositions.Add(lastCell.transform.position);
                        startDirections.Add(finalMovementDirection);

                        patrolTargets.Add(pickedNeighbour.transform);
                        patrolPaths.Add(new Path(new List<Cell>(), startPositions[startPositions.Count - 1], 0.0f, 0.0f));

                        startPositions.Add(GetStartingCell().transform.position);
                        startDirections.Add(GetStartingMovementDirection());

                        patrolTargets.Add(GetStartingCell().transform);
                        patrolPaths.Add(new Path(new List<Cell>(), startPositions[startPositions.Count - 1], 0.0f, 0.0f));

                        patrolJoined = true;
                    }
                }
            }
        }
    }

    private Cell CheckNeighbours(List<Cell> accessibleNeighbours, bool checkRowChanged)
    {
        Cell lastCell;
        Vector3 finalMovementDirection;

        foreach (var neighbour in accessibleNeighbours)
        {
            bool neighbourValidation = checkRowChanged
                ? neighbour.gridPosition.X != GetStartingCell().gridPosition.X
                : neighbour.gridPosition.Y != GetStartingCell().gridPosition.Y;
            
            if (neighbourValidation)
            {
                return neighbour;
            }
        }

        return null;
    }

    // Update is called once per frame
    public override void FixedUpdate()
    {
        for (int i = 0; i < patrolPaths.Count; i++)
        {
            if (patrolPaths[i].lookPoints.Count > 0)
            {
                LineRenderer lineRenderer = GetComponent<LineRenderer>();

                int totalPoints = 0;

                foreach (var path in patrolPaths)
                {
                    totalPoints += path.lookPoints.Count;
                }

                Vector3[] points = new Vector3[totalPoints];

                int currentPoint = 0;
                
                foreach (var path in patrolPaths)
                {
                    for (int j = 0; j < path.lookPoints.Count; j++)
                    {
                        points[currentPoint] = path.lookPoints[j].transform.position;
                        currentPoint++;
                    }
                }
                lineRenderer.positionCount = points.Length;
                lineRenderer.SetPositions(points);
            }
        }

        if(GetCurrentCell() != null && patrolPaths != null)
            base.FixedUpdate();
    }
    
    IEnumerator UpdatePath() {
        if (Time.timeSinceLevelLoad < .3f) {
            yield return new WaitForSeconds (.3f);
        }

        PathRequestManager.RequestPath (new PathRequest(0,startDirections[0], startPositions[0], patrolTargets[0].position, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = patrolTargets[currentPatrolTarget].position;

        while (true) {
            yield return new WaitForSeconds (minPathUpdateTime);
            for (int i = 0; i < patrolTargets.Count; i++)
            {
                //print(((patrolTargets[currentPatrolTarget].position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);

                bool readyToRequest = false; // protecting against requesting a path before we have enough information
    
                if (i > 0 && patrolPaths[i - 1].lookPoints.Count > 0)
                {
                    startPositions[i] = patrolTargets[i - 1].transform.position;
                    startDirections[i] = CalculateStartingMovementDirection(i);
                    
                    readyToRequest = true;
                }
                else if (i == 0)
                {
                    readyToRequest = true;
                }

                Cell currentCell = GetCurrentCell();

                Cell currentPatrolTargetCell =
                    GridController.GetCellFromWorldPosition(patrolTargets[currentPatrolTarget].position);

                if(i == currentPatrolTarget && currentCell.gridPosition == currentPatrolTargetCell.gridPosition)
                {
                    currentPatrolTarget++;
                    
                    if (currentPatrolTarget >= patrolTargets.Count)
                        currentPatrolTarget = 0;
                    
                    ChangePath();
                }
                
                if (readyToRequest && (patrolTargets[i].position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
                {
                    PathRequestManager.RequestPath(new PathRequest(i, startDirections[i], startPositions[i],
                        patrolTargets[i].position, OnPathFound));
                    targetPosOld = patrolTargets[i].position;
                }
            }
        }
    }

    private void ChangePath()
    {
        currentPatrol = patrolPaths[currentPatrolTarget];

        StopCoroutine("FollowPath");
        StartCoroutine("FollowPath");
    }

    private Vector3 CalculateStartingMovementDirection(int i)
    {
        Vector3 startingMovementDirection = Vector3.zero;
        
        int previousPatrolIndex = i - 1;

        if (previousPatrolIndex < 0)
        {
            previousPatrolIndex = patrolPaths.Count - 1;
        }

        if (patrolPaths[previousPatrolIndex].lookPoints.Count > 1)
        {
            int numberOfLookPoints = patrolPaths[previousPatrolIndex].lookPoints.Count;
            startingMovementDirection = patrolPaths[previousPatrolIndex].lookPoints[numberOfLookPoints - 1]
                                            .GetCentre() - patrolPaths[previousPatrolIndex].lookPoints[numberOfLookPoints - 2]
                                            .GetCentre();
        }
        else if(patrolPaths[previousPatrolIndex].lookPoints.Count == 1)
        {
            int previousPreviousPatrolIndex = previousPatrolIndex - 1;
            
            if (previousPreviousPatrolIndex < 0)
            {
                previousPreviousPatrolIndex = patrolPaths.Count - 1;
            }

            if (previousPreviousPatrolIndex >= 0)
            {
                int numberOfLookPoints = patrolPaths[previousPatrolIndex].lookPoints.Count;

                int numberOfPreviousLookPoints = patrolPaths[previousPreviousPatrolIndex].lookPoints.Count;

                startingMovementDirection = patrolPaths[previousPatrolIndex].lookPoints[numberOfLookPoints - 1]
                    .GetCentre() - patrolPaths[previousPreviousPatrolIndex].lookPoints[numberOfPreviousLookPoints - 1]
                    .GetCentre();
            }
        }

        //Vector3.forward - Vector3(0, 0, 1);
        //Vector3.back - Vector3(0, 0, -1);
        //Vector3.right - Vector3(1, 0, 0);
        //Vector3.left - Vector3(-1, 0, 0);

        return startingMovementDirection;
    }

    IEnumerator FollowPath()
    {
        bool followingPath = true;

        while (followingPath && currentPatrol.lookPoints.Count > 0)
        {
            Cell currentCell = GetCurrentCell();
            float distanceToCurrentCellCentre = Vector3.Distance(currentCell.GetCentre(), transform.position);
            
            bool movingTowards = isMovingTowards(currentCell.GetCentre(), transform.position, GetComponent<Rigidbody>().velocity);

            if (distanceToCurrentCellCentre < 0.05f || !movingTowards)
            {
                for (int i = 0; i < currentPatrol.lookPoints.Count; i++)
                {
                    if (currentCell.GetCentre() == currentPatrol.lookPoints[i].GetCentre()
                        || currentPatrol.lookPoints.Contains(currentCell))
                    {
                        currentPatrol.lookPoints.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        break;
                    }
                }

                if (currentPatrol.lookPoints.Count > 0)
                {
                    Vector3 directionChange = currentPatrol.lookPoints[0].GetCentre() - currentCell.GetCentre();

                    if (directionChange != Vector3.zero)
                    {
                        if (directionChange == Vector3.right)
                        {
                            if (GetMovementDirection() == Vector3.forward)
                                ChangeMovementDirection(MovementAction.Movement.TurnRight);
                            if (GetMovementDirection() == Vector3.back)
                                ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                        }
                        else if (directionChange == Vector3.left)
                        {
                            if (GetMovementDirection() == Vector3.forward)
                                ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                            if (GetMovementDirection() == Vector3.back)
                                ChangeMovementDirection(MovementAction.Movement.TurnRight);
                        }
                        else if (directionChange == Vector3.forward)
                        {
                            if (GetMovementDirection() == Vector3.right)
                                ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                            if (GetMovementDirection() == Vector3.left)
                                ChangeMovementDirection(MovementAction.Movement.TurnRight);
                        }
                        else if (directionChange == Vector3.back)
                        {
                            if (GetMovementDirection() == Vector3.right)
                                ChangeMovementDirection(MovementAction.Movement.TurnRight);
                            if (GetMovementDirection() == Vector3.left)
                                ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                        }
                    }
                }
            }

            yield return null;
        }
    }
}
