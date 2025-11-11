using System;
using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Systems.StateMachine.States
{
    public class DroneSearchingState : IDroneState
    {
        private readonly IDrone _drone;
        private readonly IResourceService _resourceService;
        private float _lastSearchTime;
        private const float SearchInterval = 0.5f;

        public DroneState StateType => DroneState.Searching;

        public DroneSearchingState(IDrone drone, IResourceService resourceService)
        {
            _drone = drone;
            _resourceService = resourceService;
        }

        public void Enter()
        {
            _drone.ClearTargetResource();
            _lastSearchTime = 0f;
        }

        public void Update()
        {
            if (Time.time - _lastSearchTime < SearchInterval)
            {
                return;
            }
            
            _lastSearchTime = Time.time;
            var nearestResource = _resourceService.FindNearestAvailableResource(_drone.Position, _drone.Faction);
            
            if (nearestResource != null)
            {
                if (nearestResource is MonoBehaviour resourceMono)
                {
                    if (resourceMono == null)
                    {
                        return;
                    }
                }
                
                if (nearestResource is DroneResourceCollection.Entities.Resource.Resource resource)
                {
                    if (resource == null)
                    {
                        return;
                    }
                    
                    try
                    {
                        if (resource.Reserve(_drone.Id, _drone.Faction))
                        {
                            _drone.SetTargetResource(nearestResource);
                        }
                    }
                    catch (MissingReferenceException)
                    {
                        return;
                    }
                }
                else
                {
                    try
                    {
                        if (nearestResource.Reserve(_drone.Id))
                        {
                            _drone.SetTargetResource(nearestResource);
                        }
                    }
                    catch (MissingReferenceException)
                    {
                        return;
                    }
                }
            }
        }

        public void Exit()
        {
        }
    }
}

