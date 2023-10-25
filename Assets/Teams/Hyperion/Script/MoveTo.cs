using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class MoveTo : Action
{
    public SharedVector2 target;
    
    public override TaskStatus OnUpdate()
    {
        float orientation = AimingHelpers.ComputeSteeringOrient(Hyperion.Hyperion.intance.spaceShip, target.Value);
        Hyperion.Hyperion.intance.SetOrientation(orientation);
        if (AlmostEqual(target.Value, Hyperion.Hyperion.intance.spaceShip.Position, float.Epsilon))
        {
            return TaskStatus.Success;
        }

        return TaskStatus.Running;

    }
    
    public bool AlmostEqual(Vector3 v1, Vector3 v2, float tolerance)
    {
        
        if (Mathf.Abs(v1.x - v2.x) > tolerance) return false;
        if (Mathf.Abs(v1.y - v2.y) > tolerance) return false;
        if (Mathf.Abs(v1.z - v2.z) > tolerance) return false;
        return true;
    }
}
