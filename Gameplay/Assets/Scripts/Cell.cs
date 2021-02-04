using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour, IHeapItem<Cell>
{
    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Cell cellToCompare)
    {
        int compare = fCost.CompareTo(cellToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(cellToCompare.hCost);
        }

        return -compare;
    }

    private static GameObject GETChildGameObjectWithName(GameObject parent, string withName) {
        for (var i = 0; i < parent.transform.childCount; i++)
        {
            if (parent.transform.GetChild(i).name == withName)
                return parent.transform.GetChild(i).gameObject;
        }
        
        return null;
    }
    public struct GridPosition
    {
        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public override string ToString() => $"({X}, {Y})";
    }

    public GridPosition gridPosition;
    
    public Vector3 centre;

    public List<AgentController> agentsInCell;

    private bool nextCell = false;

    private bool spawn = false;
    
    
    private bool walkable = true;
    public int movementPenalty;

    public int gCost;
    public int hCost;
    public Cell parent;
    private int heapIndex;

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public Cell(Vector3 centrePosition, bool _walkable = true)
    {
        centre = centrePosition;
        int x = (int) Math.Floor(centrePosition.x);
        int y = (int) Math.Floor(centrePosition.y);
        gridPosition = new GridPosition(x,y);
 
        walkable = _walkable;
    }

    public void SetSpawn(bool isSpawn)
    {
        spawn = isSpawn;
    }

    public bool IsSpawn()
    {
        return spawn;
    }
    public bool IsNextCell()
    {
        return nextCell;
    }
    public void SetNextCellIndicator(bool isNextCell)
    {
        nextCell = isNextCell;
        GETChildGameObjectWithName(gameObject, "Next").GetComponent<MeshRenderer>().enabled = nextCell;
    }
    
    public void SetPathCellIndicator(bool inPath)
    {
        GETChildGameObjectWithName(gameObject, "Path").GetComponent<MeshRenderer>().enabled = inPath;
    }

    private void SetBlocker(bool isWalkable)
    {
        GETChildGameObjectWithName(gameObject, "Left Blocker").GetComponent<MeshRenderer>().enabled = !isWalkable;
        GETChildGameObjectWithName(gameObject, "Right Blocker").GetComponent<MeshRenderer>().enabled = !isWalkable;
        GETChildGameObjectWithName(gameObject, "Forward Blocker").GetComponent<MeshRenderer>().enabled = !isWalkable;
        GETChildGameObjectWithName(gameObject, "Back Blocker").GetComponent<MeshRenderer>().enabled = !isWalkable;
    }

    public void SetWalkable(bool isWalkable)
    {
        walkable = isWalkable;
        SetBlocker(walkable);
    }

    public bool IsWalkable()
    {
        return walkable;
    }
}
