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

            float distanceToCurrentCellCentre = Vector3.Distance(GetCurrentCell().centre, transform.position);

            if (distanceToCurrentCellCentre < 0.05f)
            {
                MovementAction.Movement movement = _inputController.ProccessMovementQueue();

                if (movement != MovementAction.Movement.Forward)
                {
                    ChangeMovementDirection(movement);
                }
            }

            base.FixedUpdate();
        }
    }
}
