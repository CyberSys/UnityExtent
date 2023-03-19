using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class Cell : PersistableObject, IHeapItem<Cell>
{
    // Copy constructor.
    public Cell(Cell previousCell)
    {
        this.transform.position = previousCell.transform.position;
        this.transform.parent = previousCell.transform.parent;
        SetNextCellIndicator(previousCell.IsNextCell());
        SetSpawn(previousCell.IsSpawn());
        SetCentre(previousCell.GetCentre());
        SetWalkable(previousCell.IsWalkable());
        name = previousCell.name;
    }

    public void ResetPathInfo()
    {
        gCost = 0;
        hCost = 0;
        this.parent = null;
    }
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

        public int X { get; set; }
        public int Y { get; set; }
        
        public override int GetHashCode()
        {
            return (X << 2) ^ Y;
        }
        
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else {
                GridPosition p = (GridPosition) obj;
                return (X == p.X) && (Y == p.Y);
            }
        }
        
        public static bool operator ==(GridPosition pos1, GridPosition pos2)
        {
            return pos1.Equals(pos2);
        }
        public static bool operator !=(GridPosition pos1, GridPosition pos2)
        {
            return !pos1.Equals(pos2);
        }
        public override string ToString() => $"({X}, {Y})";
    }

    public GridPosition gridPosition;
    
    private Vector3 centre;

    public List<AgentController> agentsInCell;

    private bool nextCell = false;
    private bool spawn = false;
    public bool walkable = true;
    private bool backBoundEnabled = false;
    private bool forwardBoundEnabled = false;
    private bool leftBoundEnabled = false;
    private bool rightBoundEnabled = false;
    private bool accessible = false; // can player enter

    private Dictionary<AgentController, List<int>> agentETAs;

    public void SetAIAgentETA(int numberOfCellsAway, AIAgentController agent)
    {
        if (agentETAs.Count > 0)
        {
            foreach (var agentWithEtA in agentETAs)
            {
                // if(agentETAs)
                foreach (var distance in agentWithEtA.Value)
                {
                    int distanceBetweenEtAs = Math.Abs(distance - numberOfCellsAway);
                    if (distanceBetweenEtAs < 2 && agentWithEtA.Key.ID != agent.ID)
                    {
                        agent.Redirect(this);
                        agentETAs.Remove(agent);
                    }
                }
            }
        }
        // else
        // {
        
        if (agentETAs.ContainsKey(agent))
        {
            var newDistances = agentETAs[agent];
            if (!newDistances.Contains(numberOfCellsAway))
            {
                newDistances.Add(numberOfCellsAway);
                agentETAs[agent] = newDistances;
            }
        }
        else
        {
            List<int> newDistance = new List<int> { numberOfCellsAway };
            agentETAs[agent] = newDistance;
        }
        // }

        // agentETAs[agent] = numberOfCellsAway;

        // if (agentETAs.Values.Distinct().Count() < agentETAs.Count)
        // {
        //     agent.Redirect(this);
        // }
    }
    
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
        SetCentre(centrePosition);
 
        walkable = _walkable;
    }

    public void SetCentre(Vector3 centrePosition)
    {
        centre = centrePosition;
        int x = (int) Math.Floor(centrePosition.x);
        int y = (int) Math.Floor(centrePosition.z);
        gridPosition = new GridPosition(x,y);
        agentETAs = new Dictionary<AgentController, List<int>>();
    }

    public Vector3 GetCentre()
    {
        return centre;
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
        //GETChildGameObjectWithName(gameObject, "Next").GetComponent<MeshRenderer>().enabled = nextCell;
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
        GETChildGameObjectWithName(gameObject, "Back").GetComponent<MeshRenderer>().enabled = !backBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Back Bound").GetComponent<MeshRenderer>().enabled = backBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Back Bound").GetComponent<BoxCollider>().enabled = backBoundEnabled;
    }
    
    public void SetForwardBound(bool enableForwardBound)
    {
        forwardBoundEnabled = enableForwardBound;
        GETChildGameObjectWithName(gameObject, "Forward").GetComponent<MeshRenderer>().enabled = !forwardBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Forward Bound").GetComponent<MeshRenderer>().enabled = forwardBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Forward Bound").GetComponent<BoxCollider>().enabled = forwardBoundEnabled;
    }

    public void SetLeftBound(bool enableLeftBound)
    {
        leftBoundEnabled = enableLeftBound;
        GETChildGameObjectWithName(gameObject, "Left").GetComponent<MeshRenderer>().enabled = !leftBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Left Bound").GetComponent<MeshRenderer>().enabled = leftBoundEnabled;
        GETChildGameObjectWithName(gameObject, "Left Bound").GetComponent<BoxCollider>().enabled = leftBoundEnabled;
    }

    public void SetRightBound(bool enableRightBound)
    {
        rightBoundEnabled = enableRightBound;
        GETChildGameObjectWithName(gameObject, "Right").GetComponent<MeshRenderer>().enabled = !rightBoundEnabled;
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

    public bool IsOccupiedByAnotherAgent(int yourAgentID)
    {
        bool occupied = false;

        for (int i = 0; i < agentsInCell.Count; i++)
        {
            if (agentsInCell[i].ID != yourAgentID)
                occupied = true;
        }

        return occupied;
    }

    public void SetAccessible(bool canAccess)
    {
        accessible = canAccess;
        GETChildGameObjectWithName(gameObject, "Centre").GetComponent<MeshRenderer>().enabled = accessible;
    }

    // public void SetPathIndex(int index)
    // {
    //     GameObject canvas = GETChildGameObjectWithName(gameObject, "AI Canvas");
    //     if (canvas != null)
    //     {
    //         canvas.GetComponent<Canvas>().enabled = true;
    //         GameObject pathText = GETChildGameObjectWithName(canvas, "Path Index");
    //         Text text = pathText.GetComponent<Text>();
    //         if (text != null)
    //         {
    //             text.text += index.ToString() + "\n";
    //         }
    //     }
    // }

    public bool IsAccessibleFromCell(Cell currentCell, Vector3 currentMovementDirection)
    {
        float distanceFromCurrentCell = Vector3.Distance(currentCell.GetCentre(), GetCentre());

        if (distanceFromCurrentCell > 1.0f)
            return false;

        int x = gridPosition.X - currentCell.gridPosition.X;
        int y = gridPosition.Y - currentCell.gridPosition.Y;
        
        if (x == 0 && y == 0
            || x == -1 && y == 1 || x == 1 && y == 1 // top diagonals ignored
            || x == -1 && y == -1 || x == 1 && y == -1) // bottom diagonals ignored
            return false;
                
        if (currentMovementDirection == Vector3.forward
            && x == 0 && y == -1) // if moving forward cannot get to cells behind without turning
            return false;
        if (currentMovementDirection == Vector3.back
            && x == 0 && y == 1) // if moving backward cannot get to cells forward without turning
            return false;
        if (currentMovementDirection == Vector3.left
            && x == 1 && y == 0) // if moving left cannot get to cells to the right without turning
            return false;
        if (currentMovementDirection == Vector3.right
            && x == -1 && y == 0) // if moving right cannot get to cells to the left without turning
            return false;

        return true;
    }

    public void Update()
    {
        SetWalkable(walkable);

        GameObject canvas = GETChildGameObjectWithName(gameObject, "AI Canvas");
        if (canvas != null)
        {
            canvas.GetComponent<Canvas>().enabled = true;

            AICanvasManager aiCanvasManager = canvas.GetComponent<AICanvasManager>();
            Text text = aiCanvasManager.debugText;

            while (text.transform.parent.childCount > 1) {
                DestroyImmediate(text.transform.parent.GetChild(1).gameObject);
            }

            text.text = "";

            int numberInList = 0;

            foreach (var etaInfo in agentETAs)
            {
                foreach (var eta in etaInfo.Value)
                {
                    text = Instantiate(text, text.transform.parent);
                    RectTransform rectTransform = text.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x,
                        rectTransform.anchoredPosition.y - rectTransform.rect.height);
                    // rectTransform.rect.Set(rectTransform.rect.x, rectTransform.rect.y - (numberInList * rectTransform.rect.height),rectTransform.rect.width, rectTransform.rect.height);
                    text.text = "";
                    text.color = etaInfo.Key.Color;
                    text.text = "" + etaInfo.Key.ID + " : " + eta;
                    text.name = text.text;
                    numberInList += 1;
                }
            }
        }
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
    
    public virtual void Set (GameDataReader reader)
    {
        base.Load(reader);

        centre = reader.ReadVector3();
        gridPosition.X = reader.ReadInt();
        gridPosition.Y = reader.ReadInt();
        SetNextCellIndicator(reader.ReadBool());
        spawn = reader.ReadBool();
        SetWalkable(reader.ReadBool());
        SetBackBound(reader.ReadBool());
        SetForwardBound(reader.ReadBool());
        SetLeftBound(reader.ReadBool());
        SetRightBound(reader.ReadBool());
    }
    
    public static int SizeOf()
    {
        // base class + Vector3 centre
        // + GridPosition gridposition
        // + bool nextCell
        // + bool spawn
        // + bool walkable
        // + bool back bound
        // + bool forward bound
        // + bool left bound
        // + bool right bound
        return PersistableObject.SizeOf()
               + (sizeof(float) * 3)
               + (sizeof(int) * 2)
               + sizeof(bool)
               + sizeof(bool)
               + sizeof(bool)
               + sizeof(bool)
               + sizeof(bool)
               + sizeof(bool)
               + sizeof(bool);
    }
}
