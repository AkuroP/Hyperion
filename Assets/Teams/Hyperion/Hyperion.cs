using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;
using DoNotModify;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.CodeDom;
using Microsoft.Win32.SafeHandles;

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

		//Detection
		public float _shootingRange = 3f;
		public float _shockwaveRange = 1f;
		public float _shootingAngle = 40f;
		public LayerMask _mineLayer;

		/*public bool _enemyForwardShoot;
		public bool _mineForwardShoot;
		public bool _enemySideShoot;
		public bool _enemtSideMine;*/


		public SpaceShipView spaceShip { get; private set; }

		[Space(25)]
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

			closestFlag = (SharedVector2)behaviorTree.GetVariable("TargetPos");
			playerPos = (SharedVector2)behaviorTree.GetVariable("PlayerPos");
			isAsteroidAhead = (SharedBool)behaviorTree.GetVariable("isAsteroidAhead");
			normalHit = (SharedVector2)behaviorTree.GetVariable("NormalHit");
		}
		

		public override InputData UpdateInput(SpaceShipView spaceship, GameData data)
		{
            SpaceShipView otherSpaceship = data.GetSpaceShipForOwner(1 - spaceship.Owner);
	
            DispatchEachPoints(data);
            
			float thrust = 1f;
			// float targetOrient = spaceship.Orientation + 90.0f;
			bool needToShoot = ((SharedBool)behaviorTree.GetVariable("OutShoot")).Value;
			bool needToMine = ((SharedBool)behaviorTree.GetVariable("OutMine")).Value;
			bool needToShock = ((SharedBool)behaviorTree.GetVariable("OutShockWave")).Value;

			//Shoot
			if (needToShoot) behaviorTree.SetVariableValue("OutShoot", false);
			//Mine
			if (needToMine) behaviorTree.SetVariableValue("OutMine", false);
			//ShockWave
			if (needToMine) behaviorTree.SetVariableValue("OutShockWave", false);
			
			
			closestFlag.Value = GetClosestFlag(spaceship, data);
			isAsteroidAhead.Value = AsteroidDetection(spaceship, data);

            if (!_hasPassedPoint.Contains(closestFlag.Value)) _hasPassedPoint.Add(closestFlag.Value);
			
			ShootingDetection(spaceship, data);
            
			playerPos.Value = spaceship.Position;
			
			return new InputData(thrust, orientation, needToShoot, needToMine, needToShock);
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


		public bool AsteroidDetection(SpaceShipView spaceship, GameData data)
		{
			RaycastHit2D hit = Physics2D.Raycast(spaceship.Position, spaceship.LookAt,distanceDetectionAstero, asteroLayer);

			if (hit.collider == null){}
			else if (hit.collider != null)
			{
				normalHit.Value = hit.normal;
				
				return true;
			}

			return false;
		}

		public void ShootingDetection(SpaceShipView spaceship, GameData data)
		{
			RaycastHit2D hit = Physics2D.CircleCast(spaceShip.Position, _shootingRange, spaceship.LookAt);
			if (hit.collider != null)
			{
                float angle = Vector2.Angle(spaceship.LookAt, hit.point);
				Debug.DrawRay(spaceShip.Position, hit.point);
				Debug.DrawRay(spaceship.Position, (Vector2)hit.transform.forward * 1000, Color.green);
                if (angle < _shootingAngle)
                {
					if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Player") && Vector2.Distance(hit.transform.position, spaceShip.Position) > float.Epsilon)
					{
						float orientation = AimingHelpers.ComputeSteeringOrient(spaceShip, hit.point);
						SetOrientation(orientation);
						behaviorTree.SetVariableValue("OutShoot", true);
						Debug.Log("SHOOT ZBI");
                    }
                    if (hit.transform.tag == "Mine")
					{
                        float orientation = AimingHelpers.ComputeSteeringOrient(spaceShip, hit.point);
                        SetOrientation(orientation);
                        behaviorTree.SetVariableValue("OutShoot", true);
						Debug.Log("SHOOT MINE");
                    }
                }
				else
				{
					if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Player") && (Vector2)hit.transform.position != spaceShip.Position)
					{
						float dist = Vector2.Distance(spaceShip.Position, hit.point);
						if (dist < _shockwaveRange)
						{
							behaviorTree.SetVariableValue("OutShockWave", true);
							//Debug.Log("SHOCK WAVE");
						}
						else
						{
							behaviorTree.SetVariableValue("OutMine", true);
							//Debug.Log("MINE");
						}
					}

                }

            }
			
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawRay(spaceShip.Position, spaceShip.LookAt * distanceDetectionAstero);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(spaceShip.Position, _shootingRange);

			Gizmos.color = Color.black;
			Gizmos.DrawWireSphere(spaceShip.Position, _shockwaveRange);
			
		}

		#endregion
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

