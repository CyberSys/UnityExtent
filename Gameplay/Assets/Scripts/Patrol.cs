using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol
{
    private List<Transform> _patrolTargets = new List<Transform>();
    private Path _patrolPath;

    private int CurrentPatrolIndex = 0;
    
    private readonly List<Vector3> _startPositions;
    private readonly List<Vector3> _startDirections;
    private bool _patrolJoined = false;

    public Patrol(List<Transform> patrolTargets, Vector3 startPosition, Vector3 startDirection)
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
        return _patrolTargets[CurrentPatrolIndex];
    }
    
    public Transform GetPatrolTarget(int patrolIndex)
    {
        return _patrolTargets[patrolIndex];
    }

    public int GetNumberOfPatrolTargets()
    {
        return _patrolTargets.Count;
    }

    public int GetCurrentPatrolIndex()
    {
        return CurrentPatrolIndex;
    }

    public void SetCurrentPatrolIndex(int newPatrolIndex)
    {
        CurrentPatrolIndex = newPatrolIndex;
    }

    public void IncrementCurrentPatrolPathhIndex()
    {
        CurrentPatrolIndex++;

        if (CurrentPatrolIndex >= GetNumberOfPatrolTargets())
            CurrentPatrolIndex = 0;
    }

    public Vector3 GetStartPosition(int patrolIndex)
    {
        if (patrolIndex < _startPositions.Count)
        {
            return _startPositions[patrolIndex];
        }
        
        return Vector3.zero;
    }

    public void SetStartPosition(int patrolIndex, Vector3 startPosition)
    {
        _startPositions[patrolIndex] = startPosition;
        _startDirections[patrolIndex] = CalculateStartingMovementDirection(patrolIndex);
    }

    public Vector3 GetStartDirection(int patrolIndex)
    {
        if (patrolIndex < _startDirections.Count)
        {
            return _startDirections[patrolIndex];
        }
        
        return Vector3.zero;
    }

    public bool GetPatrolJoined()
    {
        return _patrolJoined;
    }
    
    private Cell CheckNeighbours(Cell startingCell, List<Cell> accessibleNeighbours, bool checkRowChanged)
    {
        Cell lastCell;
        Vector3 finalMovementDirection;

        foreach (var neighbour in accessibleNeighbours)
        {
            bool neighbourValidation = checkRowChanged
                ? neighbour.gridPosition.X != startingCell.gridPosition.X
                : neighbour.gridPosition.Y != startingCell.gridPosition.Y;
            
            if (neighbourValidation)
            {
                return neighbour;
            }
        }

        return null;
    }
    
    private Vector3 CalculateFinalMovementDirection(int patrolIndex)
    {
        Vector3 finalMovementDirection = Vector3.zero;

        //Vector3.forward - Vector3(0, 0, 1);
        //Vector3.back - Vector3(0, 0, -1);
        //Vector3.right - Vector3(1, 0, 0);
        //Vector3.left - Vector3(-1, 0, 0);

        return finalMovementDirection;
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
    
    public void CheckPreviousPathConnectivity(GridController gridController, Cell startingCell, Vector3 startingMovementDirection, int patrolIndex)
    {
        // if (GetPatrolPath(patrolIndex).pathCells.Count > 0 &&
        //     _patrolTargets.Count > 0)
        // {
        //     Cell previousPathEndCell = GetPatrolPath(patrolIndex-1).pathCells[GetPatrolPath(patrolIndex-1).pathCells.Count - 1];
        //
        //     Vector3 finalMovementDirection = CalculateFinalMovementDirection(patrolIndex-1);
        //
        //     bool isJoinedUp =
        //         GetPatrolJoined() || startingCell.IsAccessibleFromCell(previousPathEndCell, finalMovementDirection)
        //                                  || (startingCell.gridPosition == previousPathEndCell.gridPosition &&
        //                                      startingMovementDirection == finalMovementDirection);
        //     if (!isJoinedUp)
        //     {
        //         List<Cell> accessibleNeighbours;
        //
        //         Cell pickedNeighbour = null;
        //
        //         if (startingMovementDirection == Vector3.forward ||
        //             startingMovementDirection == Vector3.back)
        //         {
        //             if (startingMovementDirection == Vector3.forward)
        //             {
        //                 accessibleNeighbours =
        //                     gridController.GetAccessibleNeighbours(startingCell, Vector3.left);
        //
        //                 pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, true);
        //
        //                 accessibleNeighbours =
        //                     gridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.back);
        //
        //                 pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, false);
        //             }
        //             else
        //             {
        //                 accessibleNeighbours =
        //                     gridController.GetAccessibleNeighbours(startingCell, Vector3.right);
        //
        //                 pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, true);
        //
        //                 accessibleNeighbours =
        //                     gridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.forward);
        //
        //                 pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, false);
        //             }
        //         }
        //         else
        //         {
        //             if (startingMovementDirection == Vector3.left)
        //             {
        //                 accessibleNeighbours =
        //                     gridController.GetAccessibleNeighbours(startingCell, Vector3.back);
        //
        //                 pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, false);
        //
        //                 accessibleNeighbours =
        //                     gridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.right);
        //
        //                 pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, true);
        //             }
        //             else
        //             {
        //                 accessibleNeighbours =
        //                     gridController.GetAccessibleNeighbours(startingCell, Vector3.forward);
        //
        //                 pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, false);
        //
        //                 accessibleNeighbours =
        //                     gridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.left);
        //
        //                 pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, true);
        //             }
        //         }
        //
        //         // if (pickedNeighbour != null)
        //         // {
        //         //     _startPositions.Insert(patrolIndex, previousPathEndCell.transform.position);
        //         //     _startDirections.Insert(patrolIndex, finalMovementDirection);
        //         //
        //         //     _patrolTargets.Insert(patrolIndex, pickedNeighbour.transform);
        //         //     _patrolPaths.Insert(patrolIndex, new Path(new List<Cell>()));
        //         //
        //         //     _startPositions.Insert(patrolIndex, startingCell.transform.position);
        //         //     _startDirections.Insert(patrolIndex, startingMovementDirection);
        //         //
        //         //     _patrolTargets.Insert(patrolIndex, startingCell.transform);
        //         //     _patrolPaths.Insert(patrolIndex, new Path(new List<Cell>()));
        //         // }
        //     }
        // }
    }

    public void CheckCurrentPathConnectivity(GridController gridController, Cell startingCell, Vector3 startingMovementDirection)
    {
        if (GetPatrolPath().pathCells.Count > 0 &&
            _patrolTargets.Count > 0)
        {
            Cell endCell = GetPatrolPath().pathCells[GetPatrolPath().pathCells.Count - 1];

            Vector3 finalMovementDirection = CalculateStartingMovementDirection(0);

            bool isJoinedUp =
                GetPatrolJoined() || startingCell.IsAccessibleFromCell(endCell, finalMovementDirection)
                                         || (startingCell.gridPosition == endCell.gridPosition &&
                                             startingMovementDirection == finalMovementDirection);
            if (!isJoinedUp)
            {
                List<Cell> accessibleNeighbours;

                Cell pickedNeighbour = null;

                if (startingMovementDirection == Vector3.forward ||
                    startingMovementDirection == Vector3.back)
                {
                    if (startingMovementDirection == Vector3.forward)
                    {
                        accessibleNeighbours =
                            gridController.GetAccessibleNeighbours(startingCell, Vector3.left);

                        pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, true);

                        accessibleNeighbours =
                            gridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.back);

                        pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, false);
                    }
                    else
                    {
                        accessibleNeighbours =
                            gridController.GetAccessibleNeighbours(startingCell, Vector3.right);

                        pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, true);

                        accessibleNeighbours =
                            gridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.forward);

                        pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, false);
                    }
                }
                else
                {
                    if (startingMovementDirection == Vector3.left)
                    {
                        accessibleNeighbours =
                            gridController.GetAccessibleNeighbours(startingCell, Vector3.back);

                        pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, false);

                        accessibleNeighbours =
                            gridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.right);

                        pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, true);
                    }
                    else
                    {
                        accessibleNeighbours =
                            gridController.GetAccessibleNeighbours(startingCell, Vector3.forward);

                        pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, false);

                        accessibleNeighbours =
                            gridController.GetAccessibleNeighbours(pickedNeighbour, Vector3.left);

                        pickedNeighbour = CheckNeighbours(startingCell, accessibleNeighbours, true);
                    }
                }

                if (pickedNeighbour != null)
                {
                    _startPositions.Add(endCell.transform.position);
                    _startDirections.Add(finalMovementDirection);

                    _patrolTargets.Add(pickedNeighbour.transform);
                    //_patrolPaths.Add(new Path(new List<Cell>()));

                    _startPositions.Add(startingCell.transform.position);
                    _startDirections.Add(startingMovementDirection);

                    _patrolTargets.Add(startingCell.transform);
                    //_patrolPaths.Add(new Path(new List<Cell>()));

                    _patrolJoined = true;
                }
            }
        }
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
