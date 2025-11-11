using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Systems.StateMachine.States
{
    public class DroneMovingToResourceState : IDroneState
    {
        private readonly IDrone _drone;
        private const float ArrivalDistance = 1.5f;
        private float _lastUpdateTime;
        private Vector3 _lastResourcePosition;
        private const float PositionUpdateInterval = 0.2f;
        private const float PositionChangeThreshold = 0.1f;

        public DroneState StateType => DroneState.MovingToResource;

        public DroneMovingToResourceState(IDrone drone)
        {
            _drone = drone;
        }

        public void Enter()
        {
            if (IsTargetResourceValid() && IsTargetResourceAvailable())
            {
                _lastResourcePosition = _drone.TargetResource.Position;
                _drone.SetTargetPosition(_lastResourcePosition);
            }
            _lastUpdateTime = 0f;
        }

        public void Update()
        {
            if (!IsTargetResourceValid() || !IsTargetResourceAvailable())
            {
                return;
            }

            float distanceToResource = Vector3.Distance(_drone.Position, _drone.TargetResource.Position);
            
            if (distanceToResource <= ArrivalDistance)
            {
                _drone.SetTargetPosition(_drone.Position);
                return;
            }

            if (Time.time - _lastUpdateTime >= PositionUpdateInterval)
            {
                _lastUpdateTime = Time.time;
                Vector3 currentResourcePosition = _drone.TargetResource.Position;
                
                if (Vector3.Distance(currentResourcePosition, _lastResourcePosition) > PositionChangeThreshold)
                {
                    _lastResourcePosition = currentResourcePosition;
                    _drone.SetTargetPosition(currentResourcePosition);
                }
            }
        }
        
        private bool IsTargetResourceValid()
        {
            if (_drone.TargetResource == null)
            {
                return false;
            }
            
            if (_drone.TargetResource is MonoBehaviour resourceMono)
            {
                return resourceMono != null;
            }
            
            return true;
        }
        
        private bool IsTargetResourceAvailable()
        {
            if (_drone.TargetResource == null)
            {
                return false;
            }
            
            if (_drone.TargetResource.IsCollected)
            {
                return false;
            }
            
            if (_drone.TargetResource is DroneResourceCollection.Entities.Resource.Resource resource)
            {
                return resource.IsAvailableForDrone(_drone.Id, _drone.Faction);
            }
            
            return !_drone.TargetResource.IsReserved;
        }

        public void Exit()
        {
        }
    }
}

