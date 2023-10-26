using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class DodgeAsteroide : Action
{
    public SharedVector2 normalHit;
    public SharedFloat turnAngle;

    public override TaskStatus OnUpdate()
    {
        
        Vector2 playerForward = Hyperion.Hyperion.intance.spaceShip.LookAt;
        float dot = Vector2.Dot(playerForward, new Vector2(-normalHit.Value.y,normalHit.Value.x));
        
        if (dot > 0)
        {
            Debug.Log("Va à droite");
            Hyperion.Hyperion.intance.SetOrientation(turnAngle.Value);
            return TaskStatus.Success;
        }
        else if (dot == 0)
        {
            Debug.Log("tkt");
            return TaskStatus.Success;
        }
        else if (dot < 0)
        {
            Debug.Log("Va à gauche");
            Hyperion.Hyperion.intance.SetOrientation(-turnAngle.Value);
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
}
