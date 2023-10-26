using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Hyperion
{
    public class CoolDownMine : Action
    {
        private float time;
        public float totalTime = 4f;

        public override TaskStatus OnUpdate()
        {
            time += Time.deltaTime;
            if (time >= 4)
            {
                time = 0;
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }
    }
}

