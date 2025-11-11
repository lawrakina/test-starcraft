using Core.Enums;
using Core.Events;
using Core.Interfaces;
using UnityEngine;

namespace Systems.StateMachine.States
{
    public class DroneCollectingState : IDroneState
    {
        private readonly IDrone _drone;
        private float _collectTimer;
        private const float CollectDuration = 5.0f; // Время сбора ресурса в секундах

        public DroneState StateType => DroneState.Collecting;

        public DroneCollectingState(IDrone drone)
        {
            _drone = drone;
        }

        public void Enter()
        {
            _collectTimer = 0f;
            
            if (_drone is MonoBehaviour droneMono)
            {
                _drone.SetTargetPosition(_drone.Position);
            }
        }

        public void Update()
        {
            _collectTimer += Time.deltaTime;
            
            if (_collectTimer >= CollectDuration)
            {
                if (IsTargetResourceValid() && IsTargetResourceAvailable())
                {
                    var resource = _drone.TargetResource;
                    resource.Collect();
                    
                    if (resource is MonoBehaviour resourceMono && _drone is MonoBehaviour droneMono)
                    {
                        if (resource is DroneResourceCollection.Entities.Resource.Resource resourceComponent)
                        {
                            resourceComponent.AttachToDrone(droneMono.transform);
                        }
                    }
                    
                    var eventBus = Core.DI.DependencyHelper.GetEventBus();
                    eventBus?.Publish(new ResourceCollectedEvent(_drone, resource, _drone.Faction));
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
            _collectTimer = 0f;
        }
        
        public float GetProgress()
        {
            return Mathf.Clamp01(_collectTimer / CollectDuration);
        }
    }
}

