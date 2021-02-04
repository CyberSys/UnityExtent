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
            
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    // Update is called once per frame
    public override void FixedUpdate()
    {
        if (!pathUpToDate)
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
        PathRequestManager.RequestPath (new PathRequest(GetMovementDirection(),transform.position, target.position, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;

        while (true) {
            yield return new WaitForSeconds (minPathUpdateTime);
            print (((target.position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);
            if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold) {
                PathRequestManager.RequestPath (new PathRequest(GetMovementDirection(), transform.position, target.position, OnPathFound));
                targetPosOld = target.position;
            }
        }
    }
    
    IEnumerator FollowPath()
    {
        bool followingPath = true;

        while (followingPath)
        {
            Cell currentCell = GetCurrentCell();
            float distanceToCurrentCellCentre = Vector3.Distance(currentCell.centre, transform.position);
            
            if (distanceToCurrentCellCentre < 0.05f)
            {
                Vector3 directionChange = currentCell.centre - path.lookPoints[0].centre;

                if (directionChange == Vector3.zero)
                {
                    followingPath = false;
                    pathUpToDate = false;
                    yield return null;
                }
                else if (directionChange == -Vector3.right)
                {
                    if(GetMovementDirection() == Vector3.forward)
                        ChangeMovementDirection(MovementAction.Movement.TurnRight);
                    if(GetMovementDirection() == Vector3.back)
                        ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                    
                    RemoveCellFromPath();
                }
                else if (directionChange == -Vector3.left)
                {
                    if(GetMovementDirection() == Vector3.forward)
                        ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                    if(GetMovementDirection() == Vector3.back)
                        ChangeMovementDirection(MovementAction.Movement.TurnRight);
                    
                    RemoveCellFromPath();
                }
                else if (directionChange == -Vector3.forward)
                {
                    if(GetMovementDirection() == Vector3.right)
                        ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                    if(GetMovementDirection() == Vector3.left)
                        ChangeMovementDirection(MovementAction.Movement.TurnRight);
                    
                    RemoveCellFromPath();
                }
                else if (directionChange == -Vector3.back)
                {
                    if(GetMovementDirection() == Vector3.right)
                        ChangeMovementDirection(MovementAction.Movement.TurnRight);
                    if(GetMovementDirection() == Vector3.left)
                        ChangeMovementDirection(MovementAction.Movement.TurnLeft);
                    
                    RemoveCellFromPath();
                }
            }
            yield return null;
        }
    }

    private void RemoveCellFromPath()
    {
        if (path.lookPoints.Count > 1)
        {
            path.lookPoints[0].SetPathCellIndicator(false);
            path.lookPoints.RemoveAt(0);
        }
    }
}
