using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;
using DoNotModify;

namespace Hyperion {

	public class Hyperion : BaseSpaceShipController
	{
		public BehaviorTree behaviorTree;
		private SharedVector2 closestFlag;
		private SharedVector2 playerPos;
		private float orientation;
		public SpaceShipView spaceShip { get; private set; }
		
		
		
		public static Hyperion intance;

		public void Awake()
		{
			intance = this;
		}

		public override void Initialize(SpaceShipView spaceship, GameData data)
		{
			this.spaceShip = spaceship;
		}

		public override InputData UpdateInput(SpaceShipView spaceship, GameData data)
		{
			closestFlag = (SharedVector2)behaviorTree.GetVariable("TargetPos");
			playerPos = (SharedVector2)behaviorTree.GetVariable("PlayerPos");
			SpaceShipView otherSpaceship = data.GetSpaceShipForOwner(1 - spaceship.Owner);
			float thrust = 1.0f;
			float targetOrient = spaceship.Orientation + 90.0f;
			bool needShoot = AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);

			closestFlag.Value = GetClosestFlag(spaceship, data);
			playerPos.Value = spaceship.Position;
			
			
			
			return new InputData(thrust, orientation, needShoot, false, false);
		}

		public void SetOrientation(float value)
		{
			orientation = value;
		}
		
		Vector2 GetClosestFlag(SpaceShipView spaceship, GameData data)
		{
			Vector2 tMin = Vector2.zero;
			float minDist = Mathf.Infinity;
			Vector2 currentPos = spaceship.Position;

			for (int i = 0; i < data.WayPoints.Count; i++)
			{
				Vector2 t = data.WayPoints[i].Position;
				float dist = Vector2.Distance(t, currentPos);
				if (dist < minDist)
				{
					print(data.WayPoints[i]._waypoint.gameObject.name);
					tMin = t;
					minDist = dist;
				}
			}
			return tMin;
		}
	}

}
