﻿using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;
using DoNotModify;

namespace Hyperion {

	public class Hyperion : BaseSpaceShipController
	{
		[Header("Behavior")]
		public BehaviorTree behaviorTree;
		private SharedVector2 closestFlag;
		private SharedVector2 playerPos;
		private SharedVector2 normalHit;
		private SharedBool isAsteroidAhead;
		private float orientation;
		
		[Space(25)]
		public float distanceDetectionAstero = 3f;
		public LayerMask asteroLayer;
		public SpaceShipView spaceShip { get; private set; }
		
		
		
		public static Hyperion intance;

		public void Awake()
		{
			intance = this;
		}

		public override void Initialize(SpaceShipView spaceship, GameData data)
		{
			this.spaceShip = spaceship;
			closestFlag = (SharedVector2)behaviorTree.GetVariable("TargetPos");
			playerPos = (SharedVector2)behaviorTree.GetVariable("PlayerPos");
			isAsteroidAhead = (SharedBool)behaviorTree.GetVariable("isAsteroidAhead");
			normalHit = (SharedVector2)behaviorTree.GetVariable("NormalHit");
		}

		public override InputData UpdateInput(SpaceShipView spaceship, GameData data)
		{
			closestFlag.Value = GetClosestFlag(spaceship, data);
			playerPos.Value = spaceship.Position;
			isAsteroidAhead.Value = AsteroidDetection(spaceship, data);
			SpaceShipView otherSpaceship = data.GetSpaceShipForOwner(1 - spaceship.Owner);
			float thrust = 1f;
			// float targetOrient = spaceship.Orientation + 90.0f;
			bool needShoot = AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);

			
			
			
			return new InputData(thrust, orientation, needShoot, false, false);
		}

		#region Navigation

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
				if (dist < minDist && data.WayPoints[i].Owner != spaceship.Owner)
				{
					tMin = t;
					minDist = dist;
				}
			}
			return tMin;
		}

		public bool AsteroidDetection(SpaceShipView spaceship, GameData data)
		{
			RaycastHit2D hit = Physics2D.Raycast(spaceship.Position, spaceship.LookAt,distanceDetectionAstero, asteroLayer);

			if (hit.collider == null){}
			else if (hit.collider != null)
			{
				normalHit.Value = hit.normal;
				print(hit.normal);
				return true;
			}

			return false;
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawRay(spaceShip.Position, spaceShip.LookAt * distanceDetectionAstero);
		}

		#endregion
	}

}
