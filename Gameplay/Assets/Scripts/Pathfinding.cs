using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
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

        foreach (var targetCell in request.targetCells)
        {
            List<Cell> pathSection = new List<Cell>();
            
            pathSection = PathSearch(request.agentID, request.startingMovementDirection, request.startingCell.GetCentre(), targetCell.GetCentre(),  out pathSuccess);
            
            Vector3 startingMovementDirection;

            if (pathSection.Count < 2)
            {
                startingMovementDirection = pathSection[pathSection.Count - 1]
                    .GetCentre() - request.startingCell.GetCentre();
            }
            else
            {
                startingMovementDirection = pathSection[pathSection.Count - 1]
                    .GetCentre() - pathSection[pathSection.Count - 2]
                    .GetCentre();
            }

            //Vector3.forward - Vector3(0, 0, 1);
            //Vector3.back - Vector3(0, 0, -1);
            //Vector3.right - Vector3(1, 0, 0);
            //Vector3.left - Vector3(-1, 0, 0);
            
            pathCells.AddRange(pathSection);
            
            // TODO update starting direction for next path and starting cell
        }
        callback (new PathResult (pathCells, pathSuccess, request.callback));
    }

    private List<Cell> PathSearch(int agentID, Vector3 currentMovementDirection, Vector3 pathStart, Vector3 pathEnd, out bool pathSuccess)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        List<Cell> pathCells = new List<Cell>();
        pathSuccess = false;

        Cell startCell = _gridController.GetCellFromWorldPosition(pathStart);
        startCell.parent = startCell;
        Cell targetCell = _gridController.GetCellFromWorldPosition(pathEnd);

        if (startCell.IsWalkable() && targetCell.IsWalkable())
        {
            Heap<Cell> openSet = new Heap<Cell>(_gridController.MaxSize);
            HashSet<Cell> closedSet = new HashSet<Cell>();
            openSet.Add(startCell);

            while (openSet.Count > 0)
            {
                Cell currentCell = openSet.RemoveFirst();

                closedSet.Add(currentCell);

                if (currentCell == targetCell)
                {
                    sw.Stop();
                    // print("Path found: " + sw.ElapsedMilliseconds + "ms.");
                    pathSuccess = true;
                    break;
                }

                List<Cell> neighbours;

                if (
                    currentCell ==
                    startCell) // because of the way the movement works it is enough to make sure the first cell chosen is in a valid direction, because after that the path will work itself out 
                {
                    neighbours = _gridController.GetAccessibleNeighbours(currentCell, currentMovementDirection);
                }
                else
                {
                    neighbours = _gridController.GetNeighbours(currentCell);
                }

                foreach (var neighbour in neighbours)
                {
                    neighbour.IsAccessibleFromCell(currentCell, currentMovementDirection);
                    if (!neighbour.IsWalkable() || neighbour.IsOccupied(agentID) || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour =
                        currentCell.gCost + GetDistance(currentCell, neighbour) + neighbour.movementPenalty;

                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetCell);
                        neighbour.parent = currentCell;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
        }

        if (pathSuccess == true)
        {
            pathCells = RetracePath(startCell, targetCell);

            Vector3 previousPosition = pathStart;
            Vector3 previousMovementDirection = currentMovementDirection;
            
            bool validMovement = false;

            for (int i = 0; i < pathCells.Count; i++)
            {
                Vector3 directionChange = pathCells[i].GetCentre() - _gridController.GetCellFromWorldPosition(previousPosition).GetCentre();

                if (directionChange == previousMovementDirection)
                {
                    validMovement = true;
                }
                else if (directionChange == Vector3.zero)
                {
                    validMovement = false;
                }
                else if (directionChange == Vector3.right)
                {
                    if (previousMovementDirection == Vector3.forward)
                        validMovement = true;
                    else if (previousMovementDirection == Vector3.back)
                        validMovement = true;
                    else
                    {
                        validMovement = false;
                        // throw new InvalidOperationException("Illegal direction change.");
                    }
                }
                else if (directionChange == Vector3.left)
                {
                    if (previousMovementDirection == Vector3.forward)
                        validMovement = true;
                    else if (previousMovementDirection == Vector3.back)
                        validMovement = true;
                    else
                    {
                        validMovement = false;
                        // throw new InvalidOperationException("Illegal direction change.");
                    }
                }
                else if (directionChange == Vector3.forward)
                {
                    if (previousMovementDirection == Vector3.right)
                        validMovement = true;
                    else if (previousMovementDirection == Vector3.left)
                        validMovement = true;
                    else
                    {
                        validMovement = false;
                        // throw new InvalidOperationException("Illegal direction change.");
                    }
                }
                else if (directionChange == Vector3.back)
                {
                    if (previousMovementDirection == Vector3.right)
                        validMovement = true;
                    else if (previousMovementDirection == Vector3.left)
                        validMovement = true;
                    else
                    {
                        validMovement = false;
                        // throw new InvalidOperationException("Illegal direction change.");
                    }
                }

                previousPosition = pathCells[i].GetCentre();
                previousMovementDirection = directionChange;
            }

            pathSuccess = pathCells.Count > 0 && validMovement ? true : false;
        }

        return pathCells;
    }

    List<Cell> RetracePath(Cell startCell, Cell endCell)
    {
        List<Cell> path = new List<Cell>();

        Cell currentCell = endCell;

        while (currentCell != startCell)
        {
            path.Add(currentCell);

            currentCell = currentCell.parent;
        }

        //uncomment to simplify path
        //path = SimplifyPath(path);
        
        path.Reverse();

        for(int i = 0; i < path.Count; i++)
        {
            path[i].SetPathIndex(i);
        }
        
        // _gridController.SetPath(path);

        return path;
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
        int distX = Mathf.Abs(nodeA.gridPosition.X - nodeB.gridPosition.X);
        int distY = Mathf.Abs(nodeB.gridPosition.Y - nodeB.gridPosition.Y);

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
