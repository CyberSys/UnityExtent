using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : PersistableObject, IHeapItem<Cell>
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
    private bool backBoundEnabled = false;
    private bool forwardBoundEnabled = false;
    private bool leftBoundEnabled = false;
    private bool rightBoundEnabled = false;
    
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
    
    public void SetBackBound(bool enableBackBound)
    {
        backBoundEnabled = enableBackBound;
        GETChildGameObjectWithName(gameObject, "Back Bound").GetComponent<MeshRenderer>().enabled = backBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Back Bound").GetComponent<BoxCollider>().enabled = backBoundEnabled;
    }
    
    public void SetForwardBound(bool enableForwardBound)
    {
        forwardBoundEnabled = enableForwardBound;
        GETChildGameObjectWithName(gameObject, "Forward").GetComponent<MeshRenderer>().enabled = forwardBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Forward Bound").GetComponent<MeshRenderer>().enabled = forwardBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Forward Bound").GetComponent<BoxCollider>().enabled = forwardBoundEnabled;
    }

    public void SetLeftBound(bool enableLeftBound)
    {
        leftBoundEnabled = enableLeftBound;
        GETChildGameObjectWithName(gameObject, "Left Bound").GetComponent<MeshRenderer>().enabled = leftBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Left Bound").GetComponent<BoxCollider>().enabled = leftBoundEnabled;
    }

    public void SetRightBound(bool enableRightBound)
    {
        rightBoundEnabled = enableRightBound;
        GETChildGameObjectWithName(gameObject, "Right").GetComponent<MeshRenderer>().enabled = rightBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Right Bound").GetComponent<MeshRenderer>().enabled = rightBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Right Bound").GetComponent<BoxCollider>().enabled = rightBoundEnabled;
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
    
    /////////////////////// persistable
    ///
    
    public virtual void Save (GameDataWriter writer) {
        base.Save(writer);
        
        writer.Write(centre);
        writer.Write(gridPosition.X);
        writer.Write(gridPosition.Y);
        writer.Write(nextCell);
        writer.Write(spawn);
        writer.Write(walkable);
        
        writer.Write(backBoundEnabled);
        writer.Write(forwardBoundEnabled);
        writer.Write(leftBoundEnabled);
        writer.Write(rightBoundEnabled);
    }
    
    public virtual void Load (GameDataReader reader)
    {
        base.Load(reader);

        centre = reader.ReadVector3();
        gridPosition = new GridPosition(reader.ReadInt(),reader.ReadInt());
        SetNextCellIndicator(reader.ReadBool());
        spawn = reader.ReadBool();
        SetWalkable(reader.ReadBool());
        
        SetBackBound(reader.ReadBool());
        SetForwardBound(reader.ReadBool());
        SetLeftBound(reader.ReadBool());
        SetRightBound(reader.ReadBool());
    }
}
