using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridController : PersistableObject
{
    private PlayerAgentController _player;

    private int _agentCount = 0;
    
    public int rowNumber = 10;
    public int columnNumber = 10;
    private float _cellWidth = 1.0f;

    private List<List<Cell>> _grid = new List<List<Cell>>();
    
    private List<Cell> _playerAccessibleCells = new List<Cell>();

    private GameObject _gridContainerObject;

    public int MaxSize
    {
        get
        {
            return rowNumber * columnNumber;
        }
    }

    public Cell GetCellFromWorldPosition(Vector3 worldPosition)
    {
        float gridWidth = rowNumber * _cellWidth;
        float gridHeight = columnNumber  * _cellWidth;
        
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

    public Cell GetRandomAccessibleNeighbour(Cell cell, Vector3 currentMovementDirection, Cell problemCell)
    {
        List<Cell> accessibleNeighbours = GetAccessibleNeighbours(cell, currentMovementDirection);

        accessibleNeighbours.Remove(problemCell);

        if (accessibleNeighbours.Count < 1)
            return null;

        return accessibleNeighbours[Random.Range(0, accessibleNeighbours.Count)];
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

    public void ResetPathSearch()
    {
        foreach (var row in _grid)
        {
            foreach (var cell in row)
            {
                cell.ResetPathInfo();
            }
        }
    }

    private static GameObject GetChildGameObjectWithName(GameObject parent, string withName) {
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
        _gridContainerObject = new GameObject {name = "Grid"};

        _gridContainerObject.transform.parent = this.transform;
        return _gridContainerObject;
    }

    static IEnumerator Wait(float time)
    {
        yield return new WaitForSecondsRealtime(time);
    }

    void InitiliaseAgents()
    {
        CreatePlayerAgent(new Vector3(3.5f, 0.0f, 3.5f), Vector3.forward);
        
        Camera.main.GetComponent<CameraController>().PlayerTransform = _player.transform;

        CreateAIAgent(Vector3.right, new List<Vector3> {new Vector3(0.5f,0f,0.5f),new Vector3(2.5f,0f,5.5f)});
        
        CreateAIAgent(Vector3.forward, new List<Vector3> {new Vector3(5.5f,0f,0.5f),new Vector3(2.5f,0f,2.5f), new Vector3(2.5f,0.0f, 3.5f)});
    }

    private void CreatePlayerAgent(Vector3 position, Vector3 movementDirection)
    {
        Cell startCell = GetCellFromWorldPosition(position);
        
        _player = Instantiate(ObjectFactory.Get(3) as PlayerAgentController);
        _player.ID = _agentCount++;
        _player.SetStartingMovementDirection(movementDirection);
        _player.SetStartingCell(startCell.gridPosition.X, startCell.gridPosition.Y);
        
        Camera.main.GetComponent<CameraController>().PlayerTransform = _player.transform;
    }

    private void CreateAIAgent(Vector3 movementDirection, List<Vector3> keyPatrolTargets)
    {
        if (keyPatrolTargets.Count < 2)
        {
            throw new InvalidOperationException("Not enough patrol targets.");
        }
        Cell startCell = GetCellFromWorldPosition(keyPatrolTargets[0]);
        
        AgentController aiAgent =
            Instantiate(ObjectFactory.Get(4) as AIAgentController, keyPatrolTargets[0], Quaternion.identity)
                .GetComponent<AgentController>();
        aiAgent.ID = _agentCount++;
        aiAgent.name = "AI " + _agentCount;
        aiAgent.SetStartingMovementDirection(movementDirection);
        aiAgent.SetStartingCell(startCell.gridPosition.X, startCell.gridPosition.Y);

        LineRenderer lr = aiAgent.GetComponent<LineRenderer>();
        if (lr)
        {
            lr.material.SetColor("_EmissionColor", Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
        }

        AIAgentController aiAgentController = (AIAgentController) aiAgent;
        if (aiAgentController)
        {
            GameObject patrolList = new GameObject();
            patrolList.name = "Agent: " + aiAgent.ID + " Patrol";
            patrolList.transform.position = Vector3.zero;

            aiAgentController.SetKeyPatrolTargets(keyPatrolTargets);
            aiAgentController.StartPatrol();
        }
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
        writer.Write(_cellWidth);

        for (int x = 0; x < rowNumber; x++)
        {
            for (int y = 0; y < columnNumber; y++)
            {
                _grid[x][y].Save(writer);
            }
        }
        
        _player.Save(writer);
    }
    
    public virtual void Load (GameDataReader reader)
    {
        base.Load(reader);
        
        // Create Grid

        rowNumber = reader.ReadInt();
        columnNumber = reader.ReadInt();
        _cellWidth = reader.ReadFloat();
        
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
        _player = Instantiate(ObjectFactory.Get(3) as PlayerAgentController);
        _player.Load(reader);

        Camera.main.GetComponent<CameraController>().PlayerTransform = _player.transform;
    }
    
    public virtual void Set (GameDataReader reader)
    {
        base.Set(reader);
        
        // Create Grid

        rowNumber = reader.ReadInt();
        columnNumber = reader.ReadInt();
        _cellWidth = reader.ReadFloat();
        
        for (int x = 0; x < rowNumber; x++)
        {
            for (int y = 0; y < columnNumber; y++)
            {
                Cell newCell = _grid[x][y];
                newCell.Load(reader);
            }
        }
        
        // Create Player
        _player.Load(reader);

        Camera.main.GetComponent<CameraController>().PlayerTransform = _player.transform;
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
