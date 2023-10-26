using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Hyperion
{
    public class MoveTo : Action
    {
        public SharedVector2 target;
        public SharedBool flagTaken;
        public float endDistance = 0.5f;

        public override TaskStatus OnUpdate()
        {
            float orientation = AimingHelpers.ComputeSteeringOrient(Hyperion.intance.spaceShip, target.Value);
            Hyperion.intance.SetOrientation(orientation);

            if (Vector2.Distance(target.Value, Hyperion.intance.spaceShip.Position) < endDistance)
            {
                flagTaken.Value = true;
                return TaskStatus.Success;
            }

            //flagTaken.Value = false;
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
}
