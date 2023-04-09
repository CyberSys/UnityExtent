using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerAgentController : AgentController
{
    [BoxGroup("Combat Info")] [ShowInInspector]
    public float WeaponRange = 5.0f;
    [ShowInInspector]
    public static List<AIAgentController> enemiesInRange = new List<AIAgentController>();
    
    
    void UpdateEnemiesInRange()
    {
        enemiesInRange.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, WeaponRange);
        foreach (var hitCollider in hitColliders)
        {
            AIAgentController enemyInRange = hitCollider.gameObject.GetComponent<AIAgentController>();
            if (enemyInRange != null && enemyInRange.gameObject.layer == 9) // AI
            {
                enemiesInRange.Add(enemyInRange);
            }
        }
    }
    
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
        
        // Combat
        return;

        UpdateEnemiesInRange();

        // TODO remove to turn player range indicator back on
        Transform combat_debug_object;
        combat_debug_object = this.transform.GetChild(1);

        if (combat_debug_object != null)
        {
            LineRenderer line_renderer = combat_debug_object.GetComponent<LineRenderer>();

            if (line_renderer != null)
            {
                var points = new Vector3[enemiesInRange.Count * 2];
                
                for (int i = 0; i < enemiesInRange.Count; i++)
                {
                    int pointOffset = i * 2;
                    points[pointOffset] = transform.position;
                    points[pointOffset + 1] = enemiesInRange[i].transform.position;
                }
                
                line_renderer.SetPositions(points);
                line_renderer.positionCount = enemiesInRange.Count * 2;
            }
        }
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
