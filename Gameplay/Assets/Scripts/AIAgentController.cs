using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    Path path;
    
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    public void StartPatrol()
    {
        if(patrolTargets.Count > 0)
            StartCoroutine (UpdatePath ());
    }
    
    public void OnPathFound(List<Cell> waypoints, bool pathSuccessful) {
        if (pathSuccessful) {
            path = new Path(waypoints, transform.position, turnDst, stoppingDst);
            
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    // Update is called once per frame
    public override void FixedUpdate()
    {
        if(GetCurrentCell() != null && path != null)
            base.FixedUpdate();
    }
    
    IEnumerator UpdatePath() {
        if (Time.timeSinceLevelLoad < .3f) {
            yield return new WaitForSeconds (.3f);
        }
        PathRequestManager.RequestPath (new PathRequest(GetMovementDirection(),transform.position, patrolTargets[currentPatrolTarget].position, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = patrolTargets[currentPatrolTarget].position;

        while (true) {
            yield return new WaitForSeconds (minPathUpdateTime);
            if (patrolTargets.Count > 0)
            {
                print(((patrolTargets[currentPatrolTarget].position - targetPosOld).sqrMagnitude) + "    " +
                      sqrMoveThreshold);

                if (GetCurrentCell().transform.position == patrolTargets[currentPatrolTarget].position)
                {
                    currentPatrolTarget++;
                    if (currentPatrolTarget >= patrolTargets.Count)
                        currentPatrolTarget = 0;
                }
                
                if ((patrolTargets[currentPatrolTarget].position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
                {
                    PathRequestManager.RequestPath(new PathRequest(GetMovementDirection(), transform.position,
                        patrolTargets[currentPatrolTarget].position, OnPathFound));
                    targetPosOld = patrolTargets[currentPatrolTarget].position;
                }
            }
        }
    }
    
    IEnumerator FollowPath()
    {
        bool followingPath = true;

        while (followingPath && path.lookPoints.Count > 0)
        {
            Cell currentCell = GetCurrentCell();
            float distanceToCurrentCellCentre = Vector3.Distance(currentCell.GetCentre(), transform.position);
            
            bool movingTowards = isMovingTowards(currentCell.GetCentre(), transform.position, GetComponent<Rigidbody>().velocity);

            if (distanceToCurrentCellCentre < 0.05f || !movingTowards)
            {
                for (int i = 0; i < path.lookPoints.Count; i++)
                {
                    if (currentCell.GetCentre() == path.lookPoints[i].GetCentre())
                    {
                        path.lookPoints.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }

                if (path.lookPoints.Count > 0)
                {
                    Vector3 directionChange = path.lookPoints[0].GetCentre() - currentCell.GetCentre();

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
