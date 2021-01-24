using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

public class GridController : MonoBehaviour
{
    public GameObject player;

    public GameObject cellPrefab;

    public int rowNumber = 10;
    public int columnNumber = 10;

    private List<List<GridCell>> _grid = new List<List<GridCell>>();

    public GridCell GetCell(int x, int y)
    {
        if (x >= _grid.Count)
        {
            throw new InvalidOperationException("X index out of bounds.");
        }

        if (y >= _grid[x].Count)
        {   
            throw new InvalidOperationException("X index out of bounds.");
        }

        return _grid[x][y];
    }

    private static GameObject GETChildGameObjectWithName(GameObject parent, string withName) {
        for (var i = 0; i < parent.transform.childCount; i++)
        {
            if (parent.transform.GetChild(i).name == withName)
                return parent.transform.GetChild(i).gameObject;
        }
        
        return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject gridObject = new GameObject {name = "Grid"};

        gridObject.transform.parent = this.transform;

        for (int i = 0; i < rowNumber; i++)
        {
            GameObject rowObject = new GameObject();

            rowObject.transform.parent = gridObject.transform;

            rowObject.name = "" + i;
            
            List<GridCell> row = new List<GridCell>();
            for (int j = 0; j < columnNumber; j++)
            {
                GameObject cellPrefab = Instantiate(this.cellPrefab, new Vector3(i + 0.5f, 0.0f, j + 0.5f), Quaternion.identity);

                GridCell prefabGridCell = cellPrefab.GetComponent<GridCell>();

                prefabGridCell.centre = new Vector3(i + 0.5f, 0.0f, j + 0.5f);

                prefabGridCell.gridPosition = new GridCell.GridPosition(i, j);
                
                row.Add(prefabGridCell);

                cellPrefab.transform.parent = rowObject.transform;

                cellPrefab.name = "" + j;

                if (j == rowNumber - 1)
                {
                    GETChildGameObjectWithName(cellPrefab, "Forward").GetComponent<MeshRenderer>().enabled = true;
                }
                
                if (i == columnNumber - 1)
                {
                    GETChildGameObjectWithName(cellPrefab, "Right").GetComponent<MeshRenderer>().enabled = true;
                }
            }
            
            _grid.Add(row);
        }

        AgentController playerAgent = player.GetComponent<AgentController>();
        playerAgent.SetMovementDirection(Vector3.forward);
        playerAgent.SetStartingCell(0,0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
