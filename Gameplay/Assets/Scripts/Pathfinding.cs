using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
public class Pathfinding : MonoBehaviour
{
    private GridController _gridController;

    // Start is called before the first frame update
    void Start()
    {
        _gridController = gameObject.GetComponent<GridController>();
    }

    // Update is called once per frame
    // void Update()
    // {
    //     FindPath(seeker.position, target.position);
    // }

    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        
        List<Cell> pathCells = new List<Cell>();
        bool pathSuccess = false;
        
        Cell startCell = _gridController.GetCellFromWorldPosition(request.pathStart);
        startCell.parent = startCell;
        Cell targetCell = _gridController.GetCellFromWorldPosition(request.pathEnd);
        
        if(startCell.IsWalkable() && targetCell.IsWalkable())
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

                foreach (var neighbour in _gridController.GetNeighbours(currentCell))
                {
                    if (!neighbour.IsWalkable() || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentCell.gCost + GetDistance(currentCell, neighbour) + neighbour.movementPenalty;

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
        if (pathSuccess) {
            pathCells = RetracePath(startCell,targetCell);
            pathSuccess = pathCells.Count > 0;
        }
        callback (new PathResult (pathCells, pathSuccess, request.callback));
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

        _gridController.SetPath(path);

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
}
