using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
    public void StartPatrol()
    {
        if(GameObject.Find("Debug Console"))
            debugConsole = GameObject.Find("Debug Console").GetComponent<Text>();
        
        if (patrolTargets.Count > 0)
        {   
            patrolPaths = new List<Path>();
            startPositions = new List<Vector3>();
            startDirections = new List<Vector3>();
            
            for (int i = 0; i < patrolTargets.Count; i++)
            {
                startPositions.Add(GetCurrentCell().transform.position);
                startDirections.Add(GetMovementDirection());
                patrolPaths.Add(new Path(new List<Cell>()));
            }
            
            StartCoroutine(UpdatePath());
        }
    }

    public void StopPatrol()
    {
        StopCoroutine(UpdatePath ());
    }
    
    public void OnPathFound(List<Cell> pathCells, bool pathSuccessful, int patrolTargetIndex) {
        if (pathSuccessful) {
            patrolPaths[patrolTargetIndex] = new Path(pathCells);

            if (patrolTargetIndex == currentPatrolTarget)
            {
                ChangePath();
            }

            if (currentPatrol.pathCells.Count > 0 && patrolTargets.Count > 0 && currentPatrolTarget == patrolTargets.Count - 1)
            {
                Cell lastCell = currentPatrol.pathCells[currentPatrol.pathCells.Count - 1];

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
                        patrolPaths.Add(new Path(new List<Cell>()));

                        startPositions.Add(GetStartingCell().transform.position);
                        startDirections.Add(GetStartingMovementDirection());

                        patrolTargets.Add(GetStartingCell().transform);
                        patrolPaths.Add(new Path(new List<Cell>()));

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
        if (patrolPaths.Count > 0)
        {
            List<Vector3> points = new List<Vector3>();
            string debugOutput = "";

            foreach (var patrol in patrolPaths)
            {
                foreach (var cell in patrol.pathCells)
                {
                    points.Add(cell.transform.position);
                    debugOutput += cell.transform.position + "\n";
                }
            }

            SetDebugConsole(debugOutput);

            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }

        if(GetCurrentCell() != null && patrolPaths != null)
            base.FixedUpdate();
    }
    
    IEnumerator UpdatePath() {
        if (Time.timeSinceLevelLoad < .3f) {
            yield return new WaitForSeconds (.3f);
        }

        PathRequestManager.RequestPath (new PathRequest(base.ID,0,startDirections[0], startPositions[0], patrolTargets[0].position, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = patrolTargets[currentPatrolTarget].position;

        while (true) {
            yield return new WaitForSeconds (minPathUpdateTime);
            for (int i = 0; i < patrolTargets.Count; i++)
            {
                //print(((patrolTargets[currentPatrolTarget].position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);

                bool readyToRequest = false; // protecting against requesting a path before we have enough information
    
                if (i > 0 && patrolPaths[i - 1].pathCells.Count > 0)
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
                    PathRequestManager.RequestPath(new PathRequest(base.ID,i, startDirections[i], startPositions[i],
                        patrolTargets[i].position, OnPathFound));
                    targetPosOld = patrolTargets[i].position;
                }
            }
        }
    }

    private void ChangePath()
    {
        currentPatrol = new Path(new List<Cell>(patrolPaths[currentPatrolTarget].pathCells));

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

        if (patrolPaths[previousPatrolIndex].pathCells.Count > 1)
        {
            int numberOfLookPoints = patrolPaths[previousPatrolIndex].pathCells.Count;
            startingMovementDirection = patrolPaths[previousPatrolIndex].pathCells[numberOfLookPoints - 1]
                                            .GetCentre() - patrolPaths[previousPatrolIndex].pathCells[numberOfLookPoints - 2]
                                            .GetCentre();
        }
        else if(patrolPaths[previousPatrolIndex].pathCells.Count == 1)
        {
            int previousPreviousPatrolIndex = previousPatrolIndex - 1;
            
            if (previousPreviousPatrolIndex < 0)
            {
                previousPreviousPatrolIndex = patrolPaths.Count - 1;
            }

            if (previousPreviousPatrolIndex >= 0)
            {
                int numberOfLookPoints = patrolPaths[previousPatrolIndex].pathCells.Count;

                int numberOfPreviousLookPoints = patrolPaths[previousPreviousPatrolIndex].pathCells.Count;

                startingMovementDirection = patrolPaths[previousPatrolIndex].pathCells[numberOfLookPoints - 1]
                    .GetCentre() - patrolPaths[previousPreviousPatrolIndex].pathCells[numberOfPreviousLookPoints - 1]
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

        while (followingPath && currentPatrol.pathCells.Count > 0)
        {
            Cell currentCell = GetCurrentCell();
            float distanceToCurrentCellCentre = Vector3.Distance(currentCell.GetCentre(), transform.position);
            
            bool movingTowards = isMovingTowards(currentCell.GetCentre(), transform.position, GetComponent<Rigidbody>().velocity);

            if (distanceToCurrentCellCentre < 0.05f || !movingTowards)
            {
                for (int i = 0; i < currentPatrol.pathCells.Count; i++)
                {
                    if (currentCell.GetCentre() == currentPatrol.pathCells[i].GetCentre()
                        || currentPatrol.pathCells.Contains(currentCell))
                    {
                        currentPatrol.pathCells.RemoveAt(i);
                        i--;
                        // Debug.Log(patrolPaths[currentPatrolTarget].pathCells.Count);
                    }
                    else
                    {
                        break;
                    }
                }

                if (currentPatrol.pathCells.Count > 0)
                {
                    Vector3 directionChange = currentPatrol.pathCells[0].GetCentre() - currentCell.GetCentre();

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
