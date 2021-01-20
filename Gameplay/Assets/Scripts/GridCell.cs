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

    public GridCell(Vector3 centrePosition)
    {
        centre = centrePosition;
    }

    public void ToggleNextCellIndicator()
    {
        GETChildGameObjectWithName(gameObject, "Next").GetComponent<MeshRenderer>().enabled = true;
    }
}
