using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
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

    public GridCell(Vector3 centrePosition)
    {
        centre = centrePosition;
        int x = (int) Math.Floor(centrePosition.x);
        int y = (int) Math.Floor(centrePosition.y);
        gridPosition = new GridPosition(x,y);
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
    public void ToggleNextCellIndicator()
    {
        nextCell = !nextCell;
        GETChildGameObjectWithName(gameObject, "Next").GetComponent<MeshRenderer>().enabled = nextCell;
    }
}
