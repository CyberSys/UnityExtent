using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgentController : AgentController
{
    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;
    
    public Transform target;
    public float speed = 2;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 1;
    
    Path path;
    
    private bool pathUpToDate = true;
    
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        StartCoroutine (UpdatePath ());
    }
    
    public void OnPathFound(List<Cell> waypoints, bool pathSuccessful) {
        if (pathSuccessful) {
            path = new Path(waypoints, transform.position, turnDst, stoppingDst);
            
            // StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    // Update is called once per frame
    public override void FixedUpdate()
    {
        if (!pathUpToDate && CurrentCellChanged())
        {
            pathUpToDate = true;
            ResetCurrentCellChanged();
            StartCoroutine(UpdatePath());
        }

        if(GetCurrentCell() != null)
            base.FixedUpdate();
    }
    
    IEnumerator UpdatePath() {

        if (Time.timeSinceLevelLoad < .3f) {
            yield return new WaitForSeconds (.3f);
        }
        PathRequestManager.RequestPath (new PathRequest(transform.position, target.position, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;

        while (true) {
            yield return new WaitForSeconds (minPathUpdateTime);
            print (((target.position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);
            if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold) {
                PathRequestManager.RequestPath (new PathRequest(transform.position, target.position, OnPathFound));
                targetPosOld = target.position;
            }
        }
    }
    
    IEnumerator FollowPath() {

        bool followingPath = true;
        int pathIndex = 0;
        
        while (followingPath)
        {
        
            float distanceToCurrentCellCentre = Vector3.Distance(GetCurrentCell().centre, transform.position);

            if (distanceToCurrentCellCentre < 0.05f)
            {
                Vector3 directionChange = GetCurrentCell().centre - path.lookPoints[0].centre;

                if (directionChange == Vector3.zero)
                {
                    for (int i = 1; i < path.lookPoints.Count; i++)
                    {
                        directionChange = GetCurrentCell().centre - path.lookPoints[i].centre;
                        
                        if (directionChange != Vector3.zero)
                            break;
                    }
                }

                if (directionChange == -Vector3.right)
                {
                    if(GetMovementDirection() == Vector3.forward)
                        ChangeMovementDirection(MovementAction.Movement.TurnRight);
                    if(GetMovementDirection() == Vector3.back)
                        ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                }
                
                if (directionChange == -Vector3.left)
                {
                    if(GetMovementDirection() == Vector3.forward)
                        ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                    if(GetMovementDirection() == Vector3.back)
                        ChangeMovementDirection(MovementAction.Movement.TurnRight);
                }
                
                if (directionChange == -Vector3.forward)
                {
                    if(GetMovementDirection() == Vector3.right)
                        ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                    if(GetMovementDirection() == Vector3.left)
                        ChangeMovementDirection(MovementAction.Movement.TurnRight);
                }
                
                if (directionChange == -Vector3.back)
                {
                    if(GetMovementDirection() == Vector3.right)
                        ChangeMovementDirection(MovementAction.Movement.TurnRight);
                    if(GetMovementDirection() == Vector3.left)
                        ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                }

                pathUpToDate = false;
                
                yield break;

        // float speedPercent = 1;
        //
        // while (followingPath) {
        //     Vector2 pos2D = new Vector2 (transform.position.x, transform.position.z);
        //     while (path.turnBoundaries [pathIndex].HasCrossedLine (pos2D)) {
        //         if (pathIndex == path.finishLineIndex) {
        //             followingPath = false;
        //             break;
        //         } else {
        //             pathIndex++;
        //         }
        //     }
        //
        //     if (followingPath) {
        //
        //         if (pathIndex >= path.slowDownIndex && stoppingDst > 0) {
        //             speedPercent = Mathf.Clamp01 (path.turnBoundaries [path.finishLineIndex].DistanceFromPoint (pos2D) / stoppingDst);
        //             if (speedPercent < 0.01f) {
        //                 followingPath = false;
        //             }
        //         }
        //
        //         Quaternion targetRotation = Quaternion.LookRotation (path.lookPoints [pathIndex].centre - transform.position);
        //         transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        //         transform.Translate (Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }

            yield return null;

        }
    }
}
