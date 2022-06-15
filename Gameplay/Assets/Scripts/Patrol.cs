using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol
{
    private List<GameObject> _patrolTargets = new List<GameObject>();
    private Path _patrolPath;

    private int CurrentPatrolIndex = 0;
    
    private readonly List<Vector3> _startPositions;
    private readonly List<Vector3> _startDirections;
    private bool _patrolJoined = false;

    public Patrol(List<GameObject> patrolTargets, Vector3 startPosition, Vector3 startDirection)
    {
        _patrolTargets = patrolTargets;
        _startPositions = new List<Vector3>();
        _startDirections = new List<Vector3>();
        
        for (var i = 0; i < patrolTargets.Count; i++)
        {
            _startPositions.Add(startPosition);
            _startDirections.Add(startDirection);
            _patrolPath = new Path(new List<Cell>());
        }
    }

    public bool IsPathReady(int pathIndex)
    {
        return _patrolPath.pathCells.Count > 0;
    }

    public bool IsPatrolReady()
    {
        return _patrolPath.pathCells.Count > 0;
    }

    public Path GetPatrolPath()
    {
        return _patrolPath;
    }

    public void SetPatrolPath(Path newPath)
    {
        _patrolPath = newPath;
    }

    public Transform GetCurrentPatrolTarget()
    {
        return _patrolTargets[CurrentPatrolIndex].transform;
    }
    
    public Transform GetPatrolTarget(int patrolIndex)
    {
        return _patrolTargets[patrolIndex].transform;
    }

    private Vector3 CalculateStartingMovementDirection(int i)
    {
        Vector3 startingMovementDirection = Vector3.zero;

        //Vector3.forward - Vector3(0, 0, 1);
        //Vector3.back - Vector3(0, 0, -1);
        //Vector3.right - Vector3(1, 0, 0);
        //Vector3.left - Vector3(-1, 0, 0);

        if (startingMovementDirection != Vector3.zero &&
            startingMovementDirection != Vector3.forward &&
            startingMovementDirection != Vector3.back &&
            startingMovementDirection != Vector3.right &&
            startingMovementDirection != Vector3.left)
        {
            startingMovementDirection = Vector3.zero;
        }

        return startingMovementDirection;
    }

    ///
    /// Debugging / visuals
    ///

    public List<Vector3> GetPathPoints()
    {
        List<Vector3> points = new List<Vector3>();

        foreach (var cell in _patrolPath.pathCells)
        {
            points.Add(cell.transform.position);
        }

        if(_patrolPath.pathCells.Count > 0)
            points.Add(_patrolPath.pathCells[0].transform.position);

        return points;
    }
}
