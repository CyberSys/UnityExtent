using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class GridController : PersistableObject
{
    private PlayerAgentController player;
    
    public int rowNumber = 10;
    public int columnNumber = 10;
    private float cellWidth = 1.0f;

    private List<List<Cell>> _grid = new List<List<Cell>>();
    
    private List<Cell> playerAccessibleCells = new List<Cell>();

    private GameObject gridContainerObject;

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
        return _grid[Mathf.Clamp(x, 0, rowNumber-1)][Mathf.Clamp(y, 0, columnNumber-1)];
    }

    public List<Cell> GetAllNeighbours(Cell cell)
    {
        List<Cell> neighbours = new List<Cell>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if(x==0 && y == 0)
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
    
    public List<Cell> GetAccessibleNeighbours(Cell cell, Vector3 currentMovementDirection)
    {
        List<Cell> neighbours = new List<Cell>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0
                    || x == -1 && y == 1 || x == 1 && y == 1 // top diagonals ignored
                    || x == -1 && y == -1 || x == 1 && y == -1) // bottom diagonals ignored
                    continue;
                
                if (currentMovementDirection == Vector3.forward
                    && x == 0 && y == -1) // if moving forward cannot get to cells behind without turning
                    continue;
                if (currentMovementDirection == Vector3.back
                    && x == 0 && y == 1) // if moving backward cannot get to cells forward without turning
                    continue;
                if (currentMovementDirection == Vector3.left
                    && x == 1 && y == 0) // if moving left cannot get to cells to the right without turning
                    continue;
                if (currentMovementDirection == Vector3.right
                    && x == -1 && y == 0) // if moving right cannot get to cells to the left without turning
                    continue;
                
                int checkX = cell.gridPosition.X + x;
                int checkY = cell.gridPosition.Y + y;

                if (checkX >= 0 && checkX < rowNumber && checkY >= 0 && checkY < columnNumber)
                {
                    neighbours.Add(GetCell(checkX, checkY));
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
    public void GenerateGrid(Cell cellPrefab)
    {
        var gridObject = CreateGridObject();

        for (int i = 0; i < rowNumber; i++)
        {
            var rowObject = CreateRowObject(gridObject, i);

            List<Cell> row = new List<Cell>();
            for (int j = 0; j < columnNumber; j++)
            {
                Cell newCell = Instantiate(cellPrefab, new Vector3(i + 0.5f, 0.075f, j + 0.5f), Quaternion.identity);
                
                newCell.SetNextCellIndicator(false);
                newCell.SetSpawn(false);
                
                newCell.SetCentre(new Vector3(i + 0.5f, 0.075f, j + 0.5f));

                // if(i == 2 && j < 5 || i == 4 && j > 2 || i == 6 && j < 8)
                //     newCell.SetWalkable(false);
                // else
                // {
                //     newCell.SetWalkable(true);
                // }
                
                newCell.SetWalkable(true);
                
                newCell.transform.parent = rowObject.transform;

                newCell.name = "(" + i +", " + j + ")";

                if (j == 0)
                {
                    newCell.SetBackBound(true);
                }
                else
                {
                    newCell.SetBackBound(false);
                }

                if (j == rowNumber - 1)
                {
                    newCell.SetForwardBound(true);
                }
                else
                {
                    newCell.SetForwardBound(false);
                }

                if (i == 0)
                {
                    newCell.SetLeftBound(true);
                }
                else
                {
                    newCell.SetLeftBound(false);
                }

                if (i == columnNumber - 1)
                {
                    newCell.SetRightBound(true);
                }
                else
                {
                    newCell.SetRightBound(false);
                }
                
                row.Add(newCell);
            }

            _grid.Add(row);
        }
        
        InitiliaseAgents();
    }

    private static GameObject CreateRowObject(GameObject gridObject, int i)
    {
        GameObject rowObject = new GameObject();

        rowObject.transform.parent = gridObject.transform;

        rowObject.name = "" + i;
        return rowObject;
    }

    private GameObject CreateGridObject()
    {
        gridContainerObject = new GameObject {name = "Grid"};

        gridContainerObject.transform.parent = this.transform;
        return gridContainerObject;
    }

    static IEnumerator Wait(float time)
    {
        yield return new WaitForSecondsRealtime(time);
    }

    void InitiliaseAgents()
    {
        //Instantiate(aiPrefab, new Vector3(1.5f, 0.0f, 0.5f), Quaternion.identity);

        player = Instantiate(ObjectFactory.Get(3) as PlayerAgentController);
        // player.SetMovementDirection(Vector3.forward);
        player.SetStartingCell(2,2);
        
        Camera.main.GetComponent<CameraController>().PlayerTransform = player.transform;

        AgentController aiAgent = Instantiate(ObjectFactory.Get(4) as AIAgentController, new Vector3(0.5f, 0.0f, 6.5f), Quaternion.identity).GetComponent<AgentController>();
        aiAgent.SetStartingMovementDirection(Vector3.back);
        aiAgent.SetStartingCell(0,8);
        
        AIAgentController aiAgentController = (AIAgentController)aiAgent;
        if (aiAgentController)
        {
            // original
            // aiAgentController.patrolTargets.Add(_grid[0][0].transform);
            // aiAgentController.patrolTargets.Add(_grid[5][0].transform);
            // aiAgentController.patrolTargets.Add(_grid[5][4].transform);
            // aiAgentController.patrolTargets.Add(_grid[3][8].transform);
            // aiAgentController.patrolTargets.Add(_grid[0][8].transform);
            
            aiAgentController.patrolTargets.Add(_grid[0][0].transform);
            // aiAgentController.patrolTargets.Add(_grid[2][0].transform);
            // aiAgentController.patrolTargets.Add(_grid[1][0].transform);
            // aiAgentController.patrolTargets.Add(_grid[3][8].transform);
            // aiAgentController.patrolTargets.Add(_grid[0][5].transform);
            // aiAgentController.patrolTargets.Add(_grid[0][8].transform);
            
            aiAgentController.StartPatrol();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (var cell in playerAccessibleCells)
        {
            cell.SetAccessible(false);
        }
        
        playerAccessibleCells = GetAccessibleNeighbours(player.GetCurrentCell(), player.GetMovementDirection());

        foreach (var cell in playerAccessibleCells)
        {
            cell.SetAccessible(true);
        }
        bool test1 = _grid[0][1].IsAccessibleFromCell(_grid[0][0], Vector3.forward);
        bool test2 = _grid[2][0].IsAccessibleFromCell(_grid[3][0], Vector3.forward);
        int i = 0;
    }

    private void OnDestroy()
    {
        //Destroy(player.gameObject);
    }

    /////////////////////// persistable
    ///
    
    private ObjectFactory ObjectFactory;

    public void SetObjectFactory(ObjectFactory objectFactory)
    {
        ObjectFactory = objectFactory;
    }
    
    public virtual void Save (GameDataWriter writer) {
        base.Save(writer);
        
        writer.Write(rowNumber);
        writer.Write(columnNumber);
        writer.Write(cellWidth);

        for (int x = 0; x < rowNumber; x++)
        {
            for (int y = 0; y < columnNumber; y++)
            {
                _grid[x][y].Save(writer);
            }
        }
        
        player.Save(writer);
    }
    
    public virtual void Load (GameDataReader reader)
    {
        base.Load(reader);
        
        // Create Grid

        rowNumber = reader.ReadInt();
        columnNumber = reader.ReadInt();
        cellWidth = reader.ReadFloat();
        
        _grid = new List<List<Cell>>(rowNumber);
        
        var gridObject = CreateGridObject();
        
        for (int x = 0; x < rowNumber; x++)
        {
            var rowObject = CreateRowObject(gridObject, x);
            _grid.Add(new List<Cell>(columnNumber));
            for (int y = 0; y < columnNumber; y++)
            {
                Cell newCell = Instantiate(ObjectFactory.Get(2) as Cell, rowObject.transform);
                newCell.name = "(" + x +", " + y + ")";
                newCell.Load(reader);
                _grid[x].Add(newCell);
            }
        }
        
        // Create Player
        player = Instantiate(ObjectFactory.Get(3) as PlayerAgentController);
        player.Load(reader);

        Camera.main.GetComponent<CameraController>().PlayerTransform = player.transform;
    }
    
    public virtual void Set (GameDataReader reader)
    {
        base.Set(reader);
        
        // Create Grid

        rowNumber = reader.ReadInt();
        columnNumber = reader.ReadInt();
        cellWidth = reader.ReadFloat();
        
        for (int x = 0; x < rowNumber; x++)
        {
            for (int y = 0; y < columnNumber; y++)
            {
                Cell newCell = _grid[x][y];
                newCell.Load(reader);
            }
        }
        
        // Create Player
        player.Load(reader);

        Camera.main.GetComponent<CameraController>().PlayerTransform = player.transform;
    }
    
    public static int SizeOf(int numberOfRows, int numberOfColumns)
    {
        var value = PersistableObject.SizeOf();

        value += sizeof(int) + sizeof(int) + sizeof(int);
        value += numberOfRows * numberOfColumns * Cell.SizeOf();
        value += PlayerAgentController.SizeOf();

        return value;
    }
}
