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

    PlayerAgentController()
    {
        // This and touch input controller will need to change for multiplayer
        _inputController.AgentBeingControlled = this;
        _inputController.Actions.Add(new MovementAction(new List<KeyCode>(new KeyCode[] {KeyCode.A, KeyCode.LeftArrow}), 
            new List<TouchGesture>(new TouchGesture[] { TouchGesture.SwipeLeft }), MovementAction.Movement.TurnLeft));
        _inputController.Actions.Add(new MovementAction(new List<KeyCode>(new KeyCode[] {KeyCode.D, KeyCode.RightArrow}),
            new List<TouchGesture>(new TouchGesture[] { TouchGesture.SwipeRight }), MovementAction.Movement.TurnRight));
    }
    
    
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
    
    
    
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }
    
    public override void FixedUpdate ()
    {
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
