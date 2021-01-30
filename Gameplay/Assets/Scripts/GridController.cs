using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class GridController : MonoBehaviour
{
    public GameObject player;

    public GameObject cellPrefab;
    public GameObject aiPrefab;

    public int rowNumber = 10;
    public int columnNumber = 10;

    private List<List<GridCell>> _grid = new List<List<GridCell>>();

    private bool AgentsInitialised = false;

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
                GameObject cellPrefab = Instantiate(this.cellPrefab, new Vector3(i + 0.5f, 0.0f, j + 0.5f),
                    Quaternion.identity);

                GridCell prefabGridCell = cellPrefab.GetComponent<GridCell>();

                prefabGridCell.centre = new Vector3(i + 0.5f, 0.075f, j + 0.5f);

                prefabGridCell.gridPosition = new GridCell.GridPosition(i, j);

                row.Add(prefabGridCell);

                cellPrefab.transform.parent = rowObject.transform;

                cellPrefab.name = "" + j;

                if (j == 0)
                {
                    GETChildGameObjectWithName(cellPrefab, "Back Bound").GetComponent<MeshRenderer>().enabled = true;
                    GETChildGameObjectWithName(cellPrefab, "Back Bound").GetComponent<BoxCollider>().enabled = true;
                }

                if (j == rowNumber - 1)
                {
                    GETChildGameObjectWithName(cellPrefab, "Forward").GetComponent<MeshRenderer>().enabled = true;
                    GETChildGameObjectWithName(cellPrefab, "Forward Bound").GetComponent<MeshRenderer>().enabled = true;
                    GETChildGameObjectWithName(cellPrefab, "Forward Bound").GetComponent<BoxCollider>().enabled = true;
                }

                if (i == 0)
                {
                    GETChildGameObjectWithName(cellPrefab, "Left Bound").GetComponent<MeshRenderer>().enabled = true;
                    GETChildGameObjectWithName(cellPrefab, "Left Bound").GetComponent<BoxCollider>().enabled = true;
                }

                if (i == columnNumber - 1)
                {
                    GETChildGameObjectWithName(cellPrefab, "Right").GetComponent<MeshRenderer>().enabled = true;
                    GETChildGameObjectWithName(cellPrefab, "Right Bound").GetComponent<MeshRenderer>().enabled = true;
                    GETChildGameObjectWithName(cellPrefab, "Right Bound").GetComponent<BoxCollider>().enabled = true;
                }
            }

            _grid.Add(row);
        }
        
        gameObject.GetComponent<NavMeshSurface>().BuildNavMesh ();
    }

    static IEnumerator Wait(float time)
    {
        yield return new WaitForSecondsRealtime(time);
    }

    void InitiliaseAgents()
    {
        Instantiate(aiPrefab, new Vector3(0.5f, 0.0f, 1.5f), Quaternion.identity);

        AgentController playerAgent = player.GetComponent<AgentController>();
        playerAgent.SetMovementDirection(Vector3.forward);
        playerAgent.SetStartingCell(0,0);

        AgentsInitialised = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameObject.GetComponent<NavMeshSurface>().navMeshData != null && !AgentsInitialised)
        {
            InitiliaseAgents();
        }
    }
}
