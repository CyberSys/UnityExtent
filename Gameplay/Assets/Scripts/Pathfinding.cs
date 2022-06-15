using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
using System.Linq;

public class Pathfinding : MonoBehaviour
{
    private GridController _gridController;

    // Start is called before the first frame update
    void Awake()
    {
        _gridController = gameObject.GetComponent<GridController>();
    }

    public void FindPatrol()
    {
        
    }
    
    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        List<Cell> pathCells = new List<Cell>();
        bool pathSuccess = false;

        pathCells = PathSearch(request.agentID, request.startingMovementDirection, request.keyPatrolTargets, out pathSuccess);
        
        callback (new PathResult (pathCells, pathSuccess, request.callback));
    }

    private List<Cell> PathSearch(int agentID, Vector3 startingMovementDirection, List<Vector3> keyPatrolTargets, out bool pathSuccess)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        List<Cell> pathCells = new List<Cell>();
        pathSuccess = false;
        
        Vector3 currentMovementDirection = startingMovementDirection;
        Cell originalStartCell = _gridController.GetCellFromWorldPosition(keyPatrolTargets[0]);
        originalStartCell.parent = originalStartCell;
        Cell newStartCell = originalStartCell;
        int targetsFound = 0; // starting at 1 because 0 is starting cell
        
        if (keyPatrolTargets.Count > 1)
        {
            Cell targetCell = _gridController.GetCellFromWorldPosition(keyPatrolTargets[targetsFound]);
            
            if (originalStartCell.IsWalkable() && targetCell.IsWalkable())
            {
                Heap<Cell> openSet = new Heap<Cell>(_gridController.MaxSize);
                HashSet<Cell> closedSet = new HashSet<Cell>();
                openSet.Add(originalStartCell);

                Cell previousCell = originalStartCell;
                
                while (openSet.Count > 0)
                {
                    Cell currentCell = openSet.RemoveFirst();

                    Vector3 directionChange = currentCell.GetCentre() - previousCell.GetCentre();

                    // if (directionChange.magnitude > 1)
                    // {
                    //     throw new InvalidOperationException("Something went wrong with path calculation.");
                    // }
                    
                    bool validMovement = false;

                    if (directionChange != Vector3.zero)
                    {
                        CalculateNewMovementDirection(ref currentMovementDirection, ref directionChange, ref validMovement);
                    }

                    closedSet.Add(currentCell);

                    if (currentCell == targetCell)
                    {
                        List<Cell> pathSectionCells = new List<Cell>();

                        RetracePath(newStartCell, targetCell, pathSectionCells);

                        if (pathSectionCells.Count < 1)
                        {
                            pathSectionCells.Add(currentCell);
                        }
                        
                        pathCells.AddRange(pathSectionCells);
                        
                        openSet = new Heap<Cell>(_gridController.MaxSize);
                        closedSet = new HashSet<Cell>();
                        
                        //previousCell = currentCell;
                        
                        bool validSection = false;

                        if (pathCells.Count > 0)
                        {
                            currentMovementDirection = startingMovementDirection;
                            ValidateMovement(ref currentMovementDirection, out validSection, originalStartCell, pathCells);
                        }
                        else
                        {
                            validSection = true;
                        }

                        if (!validSection && pathCells.Count != 0)
                        {
                            throw new InvalidOperationException("Invalid path.");
                        }

                        int test = keyPatrolTargets.Count - 1;

                        if (targetsFound == test) // start position in index 0, so ignore 1
                        {
                            if (pathCells[0] == pathCells[pathCells.Count - 1])
                            {
                                sw.Stop();
                                // print("Path found: " + sw.ElapsedMilliseconds + "ms.");
                                pathSuccess = true;
                                break;
                            }
                        }

                        targetsFound++;
                        
                        targetCell = _gridController.GetCellFromWorldPosition(keyPatrolTargets[targetsFound]);
                        
                        newStartCell = currentCell;
                        newStartCell.parent = newStartCell;
                        openSet.Add(currentCell);
                    }

                    List<Cell> neighbours = _gridController.GetAccessibleNeighbours(currentCell, currentMovementDirection);

                    foreach (var neighbour in neighbours)
                    {
                        // neighbour.IsAccessibleFromCell(currentCell, currentMovementDirection);
                        if (!neighbour.IsWalkable() || neighbour.IsOccupiedByAnotherAgent(agentID) || closedSet.Contains(neighbour))
                        {
                            continue;
                        }

                        var cellDistance = GetDistance(currentCell, neighbour);

                        var newMovementCostToNeighbour =
                            currentCell.gCost + cellDistance + neighbour.movementPenalty;

                        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetCell);
                            neighbour.parent = currentCell;
                            
                            if (!openSet.Contains(neighbour))
                            {
                                openSet.Add(neighbour);
                            }
                            else
                                openSet.UpdateItem(neighbour);
                        }
                    }

                    previousCell = currentCell;
                }
            }
            else
            {
                throw new InvalidOperationException("Condition not handled.");
            }

            if (pathSuccess == true)
            {
                currentMovementDirection = startingMovementDirection;
                ValidateMovement(ref currentMovementDirection, out pathSuccess, originalStartCell, pathCells);
                // pathCells.Insert(0, originalStartCell);
            }
        }

        return pathCells;
    }

    private void ValidateMovement(ref Vector3 currentMovementDirection, out bool pathSuccess, Cell startCell, List<Cell> pathCells)
    {
        Vector3 previousPosition = startCell.GetCentre();

        bool validMovement = false;

        for (int i = 0; i < pathCells.Count; i++)
        {
            Vector3 directionChange = pathCells[i].GetCentre() -
                                      _gridController.GetCellFromWorldPosition(previousPosition).GetCentre();

            if (directionChange != Vector3.zero && directionChange != currentMovementDirection)
            {
                CalculateNewMovementDirection(ref currentMovementDirection, ref directionChange, ref validMovement);
            }
            else
            {
                validMovement = true;
            }

            if (validMovement == false)
            {
                break;
            }
            
            previousPosition = pathCells[i].GetCentre();
        }

        pathSuccess = pathCells.Count > 0 && validMovement;
    }

    private static void CalculateNewMovementDirection(ref Vector3 currentMovementDirection, ref Vector3 directionChange,
        ref bool validMovement)
    {
        if (directionChange == currentMovementDirection)
        {
            validMovement = true;
        }
        else if (directionChange == Vector3.zero)
        {
            validMovement = false;
        }
        else if (directionChange == Vector3.right)
        {
            if (currentMovementDirection == Vector3.forward)
                validMovement = true;
            else if (currentMovementDirection == Vector3.back)
                validMovement = true;
            else
            {
                validMovement = false;
                throw new InvalidOperationException("Illegal direction change.");
            }
        }
        else if (directionChange == Vector3.left)
        {
            if (currentMovementDirection == Vector3.forward)
                validMovement = true;
            else if (currentMovementDirection == Vector3.back)
                validMovement = true;
            else
            {
                validMovement = false;
                throw new InvalidOperationException("Illegal direction change.");
            }
        }
        else if (directionChange == Vector3.forward)
        {
            if (currentMovementDirection == Vector3.right)
                validMovement = true;
            else if (currentMovementDirection == Vector3.left)
                validMovement = true;
            else
            {
                validMovement = false;
                throw new InvalidOperationException("Illegal direction change.");
            }
        }
        else if (directionChange == Vector3.back)
        {
            if (currentMovementDirection == Vector3.right)
                validMovement = true;
            else if (currentMovementDirection == Vector3.left)
                validMovement = true;
            else
            {
                validMovement = false;
                throw new InvalidOperationException("Illegal direction change.");
            }
        }

        currentMovementDirection = new Vector3(directionChange.x, directionChange.y, directionChange.z);
    }

    void RetracePath(Cell startCell, Cell endCell, List<Cell> path)
    {
        Cell currentCell = endCell;
        
        while (currentCell != startCell)
        {
            path.Add(currentCell);

            currentCell = currentCell.parent;
        }

        //uncomment to simplify path
        //path = SimplifyPath(path);
        
        path.Reverse();
        
        // path.Insert(0,startCell);

        // for(int i = 0; i < path.Count; i++)
        // {
        //     path[i].SetPathIndex(i);
        // }
        
        // _gridController.SetPath(path);
        _gridController.ResetPathSearch();
    }
    
    List<Cell> SimplifyPath(List<Cell> path) {
        List<Cell> waypoints = new List<Cell>();
        Vector2 directionOld = Vector2.zero;
		
        for (int i = 1; i < path.Count; i ++) {
            Vector2 directionNew = new Vector2(path[i-1].gridPosition.X - path[i].gridPosition.X,path[i-1].gridPosition.Y - path[i].gridPosition.Y);
            if (directionNew != directionOld) {
                waypoints.Add(path[i]);
            }
            directionOld = directionNew;
        }
        return waypoints;
    }

    int GetDistance(Cell nodeA, Cell nodeB)
    {
        int distX = nodeA.gridPosition.X - nodeB.gridPosition.X;
        int distY = nodeA.gridPosition.Y - nodeB.gridPosition.Y;

        if (distX < 0)
            distX *= -1;
        
        if (distY < 0)
            distY *= -1;
        
        /* enable diagonals
        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);*/

        return distX + distY;
    }

    Vector3 GetDirection(Cell currentCell, Cell nextCell)
    {
        return nextCell.GetCentre() - currentCell.GetCentre();
    }
}
