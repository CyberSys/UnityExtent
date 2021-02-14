using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAgentController : AgentController
{
    private InputController _inputController;
    
    // Start is called before the first frame update
    public override void Start()
    {
        _inputController = new InputController();
        
        base.Start();
    }
    
    public override void FixedUpdate ()
    {
        if (GetCurrentCell() != null)
        {
            _inputController.Update();

            float distanceToCurrentCellCentre = Vector3.Distance(GetCurrentCell().GetCentre(), transform.position);

            if (distanceToCurrentCellCentre < 0.05f && _inputController.MovementQueue.Count > 0)
            {
                MovementAction.Movement movement = _inputController.ProccessMovementQueue();

                if (movement != MovementAction.Movement.Forward)
                {
                    ChangeMovementDirection(movement);
                }
            }
        }
        base.FixedUpdate();
    }
    
    /////////////////////// persistable
    ///
    
    public virtual void Save (GameDataWriter writer) {
        base.Save(writer);
        
        writer.Write(GetMovementDirection());
        // writer.Write(gridPosition.X);
        // writer.Write(gridPosition.Y);
        // writer.Write(nextCell);
        // writer.Write(spawn);
        // writer.Write(walkable);
        //
        // writer.Write(backBoundEnabled);
        // writer.Write(forwardBoundEnabled);
        // writer.Write(leftBoundEnabled);
        // writer.Write(rightBoundEnabled);
    }
    
    public virtual void Load (GameDataReader reader)
    {
        base.Load(reader);

        SetMovementDirection(reader.ReadVector3());
        // gridPosition = new GridPosition(reader.ReadInt(),reader.ReadInt());
        // SetNextCellIndicator(reader.ReadBool());
        // spawn = reader.ReadBool();
        // SetWalkable(reader.ReadBool());
        //
        // SetBackBound(reader.ReadBool());
        // SetForwardBound(reader.ReadBool());
        // SetLeftBound(reader.ReadBool());
        // SetRightBound(reader.ReadBool());
    }
    
    public virtual void Set (GameDataReader reader)
    {
        Load(reader);
    }
    
    public static int SizeOf()
    {
        // base class + Vector3 movement direction
        return PersistableObject.SizeOf() + (sizeof(float) * 3);
    }
}
