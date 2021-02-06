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
    public GameObject aiPrefab;

    public GameObject cellPrefab;
    public GameObject aiNavAgentPrefab;

    public int rowNumber = 10;
    public int columnNumber = 10;
    private float cellWidth = 1.0f;

    private List<List<Cell>> _grid = new List<List<Cell>>();

    public int MaxSize
    {
        get
        {
            return rowNumber * columnNumber;
        }
    }

    public Cell GetCellFromWorldPosition(Vector3 worldPosition)
    {
        float gridWidth = rowNumber * cellWidth;
        float gridHeight = columnNumber  * cellWidth;
        
        //float percentX = (worldPosition.x + (gridWidth / 2.0f)) / gridWidth; // centred at origin, mine isn't
        float percentX = worldPosition.x / gridWidth;
        percentX = Mathf.Clamp01(percentX);
        //float percentY = (worldPosition.z + (gridHeight / 2.0f)) / gridHeight; // centred at origin, mine isn't
        float percentY = worldPosition.z / gridHeight; // using z because grid is on it's side
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.FloorToInt(rowNumber * percentX);
        int y = Mathf.FloorToInt(columnNumber * percentY);

        return _grid[x][y];
    }
    public Cell GetCell(int x, int y)
    {
        return _grid[Mathf.Clamp(x, 0, rowNumber)][Mathf.Clamp(y, 0, columnNumber)];
    }

    public List<Cell> GetNeighbours(Cell cell)
    {
        List<Cell> neighbours = new List<Cell>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if(x==0 && y == 0
                   || x==-1 && y==1 || x==1 && y==1 // top diagonals ignored
                   || x==-1 && y==-1 || x==1 && y==-1) // bottom diagonals ignored
                    continue;

                int checkX = cell.gridPosition.X + x;
                int checkY = cell.gridPosition.Y + y;

                if (checkX >= 0 && checkX < rowNumber && checkY >= 0 && checkY < columnNumber)
                {
                    neighbours.Add(GetCell(checkX,checkY));
                }
            }
        }

        return neighbours;
    }

    private List<Cell> _path;

    public void SetPath(List<Cell> newPath)
    {
        ResetPath(false);
        
        _path = newPath;

        ResetPath(true);
    }

    void ResetPath(bool showPath)
    {
        if (_path == null)
            return;
        
        foreach (var cell in _path)
        {
            // if(cell.IsNextCell())
            cell.SetPathCellIndicator(showPath);
        }
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

            List<Cell> row = new List<Cell>();
            for (int j = 0; j < columnNumber; j++)
            {
                GameObject cellPrefab = Instantiate(this.cellPrefab, new Vector3(i + 0.5f, 0.0f, j + 0.5f),
                    Quaternion.identity);

                Cell prefabCell = cellPrefab.GetComponent<Cell>();

                prefabCell.centre = new Vector3(i + 0.5f, 0.075f, j + 0.5f);

                prefabCell.gridPosition = new Cell.GridPosition(i, j);

                if(i == 2 && j < 5 || i == 4 && j > 2 || i == 6 && j < 8)
                    prefabCell.SetWalkable(false);
                else
                {
                    prefabCell.SetWalkable(true);
                }

                row.Add(prefabCell);

                cellPrefab.transform.parent = rowObject.transform;

                cellPrefab.name = "" + j;

                Cell newCell = cellPrefab.GetComponent<Cell>();

                if (j == 0)
                {
                    newCell.SetBackBound(true);
                }

                if (j == rowNumber - 1)
                {
                    newCell.SetForwardBound(true);
                }

                if (i == 0)
                {
                    newCell.SetLeftBound(true);
                }

                if (i == columnNumber - 1)
                {
                    newCell.SetRightBound(true);
                }
            }

            _grid.Add(row);
        }
        
        InitiliaseAgents();
    }

    static IEnumerator Wait(float time)
    {
        yield return new WaitForSecondsRealtime(time);
    }

    void InitiliaseAgents()
    {
        //Instantiate(aiPrefab, new Vector3(1.5f, 0.0f, 0.5f), Quaternion.identity);

        // AgentController playerAgent = player.GetComponent<AgentController>();
        // playerAgent.SetMovementDirection(Vector3.forward);
        // playerAgent.SetStartingCell(0,0);
        
        
        
        AgentController aiAgent = Instantiate(aiPrefab, new Vector3(1.5f, 0.0f, 0.5f), Quaternion.identity).GetComponent<AgentController>();
        aiAgent.SetMovementDirection(Vector3.forward);
        aiAgent.SetStartingCell(0,1);

        AIAgentController aiAgentController = (AIAgentController)aiAgent;
        if (aiAgentController)
            aiAgentController.target = GameObject.Find("/Target").transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // if (gameObject.GetComponent<NavMeshSurface>() && gameObject.GetComponent<NavMeshSurface>().navMeshData != null && !AgentsInitialised)
        // {
        //     InitiliaseAgents();
        // }
    }
}
