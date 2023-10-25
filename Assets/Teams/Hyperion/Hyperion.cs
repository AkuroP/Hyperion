﻿using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;
using DoNotModify;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.CodeDom;

namespace Hyperion {

	public class Hyperion : BaseSpaceShipController
	{
		public BehaviorTree behaviorTree;
		private SharedVector2 closestFlag;
		private SharedVector2 playerPos;
		private float orientation;
		public SpaceShipView spaceShip { get; private set; }

        [SerializeField] private Vector2 _middleScreen;

        //[SerializeField] private int _wpIndex = 0;

        public List<Vector2> _topLeftRegion;
        public List<Vector2> _topRightRegion;
        public List<Vector2> _bottomLeftRegion;
        public List<Vector2> _bottomRightRegion;

		[SerializeField]private List<Vector2> _hasPassedPoint = new List<Vector2>();


		[Tooltip("0 = TL, 1 = TR, 2 = BL, 3 = BR")]
		public List<int> _regionsBestValue = new List<int>(4);
        
        public List<Vector2> _bestWP;



        public static Hyperion intance;

		public void Awake()
		{
			intance = this;
		}

		public override void Initialize(SpaceShipView spaceship, GameData data)
        {
			this.spaceShip = spaceship;

            DispatchEachPoints(data);
        }

		public override InputData UpdateInput(SpaceShipView spaceship, GameData data)
		{
			closestFlag = (SharedVector2)behaviorTree.GetVariable("TargetPos");
			playerPos = (SharedVector2)behaviorTree.GetVariable("PlayerPos");
            SpaceShipView otherSpaceship = data.GetSpaceShipForOwner(1 - spaceship.Owner);
			float thrust = 1f;
			// float targetOrient = spaceship.Orientation + 90.0f;
			bool needShoot = AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);

			
			closestFlag.Value = GetClosestFlag(spaceship, data);

            if (!_hasPassedPoint.Contains(closestFlag.Value)) _hasPassedPoint.Add(closestFlag.Value);

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

			

			for (int i = 0; i < _bestWP.Count; i++)
			{
				Vector2 t = _bestWP[i];
				float dist = Vector2.Distance(t, currentPos);
				
				int bestWPOwner = data.WayPoints.Find(x => x.Position == _bestWP[i]).Owner;
				if (bestWPOwner == spaceship.Owner) continue;
				if (dist < minDist)
				{
					tMin = t;
					minDist = dist;
				}
			}

			if (_hasPassedPoint.Count >= _bestWP.Count)
			{
				ResetAll(data);	
			}

			return tMin;
		}

        private void DispatchEachPoints(GameData data)
        {
            _middleScreen = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2, Screen.height / 2));

            foreach (WayPointView point in data.WayPoints)
            {
				if (point.Position.x <= _middleScreen.x && point.Position.y >= _middleScreen.y)
				{
					if (point.Owner == spaceShip.Owner)continue;
					else if (point.Owner != spaceShip.Owner) _regionsBestValue[0] += 2;
					else _regionsBestValue[0] += 1;
					_topLeftRegion.Add(point.Position);


				}

				if (point.Position.x > _middleScreen.x && point.Position.y >= _middleScreen.y)
				{
                    if (point.Owner == spaceShip.Owner) continue;
                    else if (point.Owner != spaceShip.Owner) _regionsBestValue[1] += 2;
                    else _regionsBestValue[1] += 1;
					//Debug.Log($"{point.Owner} VS {spaceShip.Owner}");
                    _topRightRegion.Add(point.Position);
					
				}
				if (point.Position.x <= _middleScreen.x && point.Position.y < _middleScreen.y)
				{
                    if (point.Owner == spaceShip.Owner) continue;
                    else if (point.Owner != spaceShip.Owner) _regionsBestValue[2] += 2;
                    else _regionsBestValue[2] += 1;
                    _bottomLeftRegion.Add(point.Position);
					
				}
				if (point.Position.x > _middleScreen.x && point.Position.y < _middleScreen.y)
				{
                    if (point.Owner == spaceShip.Owner) continue;
                    else if (point.Owner != spaceShip.Owner) _regionsBestValue[3] += 2;
                    else _regionsBestValue[3] += 1;
                    _bottomRightRegion.Add(point.Position);
                }
            }

			AssignBestWP();
        }

        private void AssignBestWP()
        {
            int bestValue = 0;
			int index = 0;
            for (int i = 0; i < 4; i++)
            {
				if (_regionsBestValue[i] >= bestValue)
				{
					bestValue = _regionsBestValue[i];
					index = i;
				}
            }
			
			//Debug.Log($"REGION {index}");
			
			switch(index)
			{
				case 0:
					_bestWP = _topLeftRegion;
				break;

                case 1:
                    _bestWP = _topRightRegion;
                break;

                case 2:
                    _bestWP = _bottomLeftRegion;
                break;

                case 3:
                    _bestWP = _bottomRightRegion;
                break;
            }
        }

		private void ResetAll(GameData data)
		{
			for (int i = 0; i < _regionsBestValue.Count; i++) _regionsBestValue[i] = 0;
			_bestWP.Clear();
			_topLeftRegion.Clear();
			_topRightRegion.Clear();
			_bottomLeftRegion.Clear();
			_bottomRightRegion.Clear();
			_hasPassedPoint.Clear();
			//print("RESET");
            DispatchEachPoints(data);

        }
    }


}
