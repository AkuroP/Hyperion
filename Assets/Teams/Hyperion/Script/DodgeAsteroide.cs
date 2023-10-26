using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class DodgeAsteroide : Action
{
    public SharedVector2 normalHit;
    public SharedFloat turnAngle;
    public SharedBool avoid;

    public override TaskStatus OnUpdate()
    {
        
        Vector2 playerForward = Hyperion.Hyperion.intance.spaceShip.LookAt;
        float dotProd = Vector2.Dot(playerForward, new Vector2(-normalHit.Value.y,normalHit.Value.x));

        // float orient = (90 * (1 - Mathf.Abs(dotProd))) * -Mathf.Sign(dotProd);
        // Hyperion.Hyperion.intance.SetOrientation(orient);
        
        Avoid(dotProd, turnAngle.Value);
       
        return TaskStatus.Success;
    }

    private void Avoid(float dot, float angle)
    {
        if (dot > 0)
        {
            Hyperion.Hyperion.intance.SetOrientation(angle);
        }
        else if (dot == 0)
        {
            Hyperion.Hyperion.intance.SetOrientation(angle);
        }
        else if (dot < 0)
        {
            Hyperion.Hyperion.intance.SetOrientation(-angle);
        }
    }
}
