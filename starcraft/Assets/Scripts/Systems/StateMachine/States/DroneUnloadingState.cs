using Core.Enums;
using Core.Interfaces;
using DroneResourceCollection.Entities.Drone;
using UnityEngine;

namespace Systems.StateMachine.States
{
    public class DroneUnloadingState : IDroneState
    {
        private readonly IDrone _drone;
        private float _unloadTimer;
        private const float UnloadDuration = 0.5f; // Небольшая задержка для завершения выгрузки
        private bool _unloadCompleted;

        public DroneState StateType => DroneState.Unloading;

        public DroneUnloadingState(IDrone drone)
        {
            _drone = drone;
        }

        public void Enter()
        {
            _unloadTimer = 0f;
            _unloadCompleted = false;
            
            if (_drone.HomeBase != null)
            {
                _drone.HomeBase.AddResource();
                
                if (_drone.TargetResource != null)
                {
                    if (_drone.TargetResource is DroneResourceCollection.Entities.Resource.Resource resourceComponent)
                    {
                        resourceComponent.DestroyAfterUnload();
                    }
                }
                
                if (_drone is MonoBehaviour droneMono)
                {
                    DroneVisuals visuals = droneMono.GetComponent<DroneVisuals>();
                    if (visuals)
                    {
                        visuals.PlayUnloadEffect();
                    }
                }
            }
            else
            {
                Debug.LogError($"[DroneUnloadingState] Drone {_drone.Id} in Unloading state but HomeBase is null!");
            }
        }

        public void Update()
        {
            _unloadTimer += Time.deltaTime;
            
            if (_unloadTimer >= UnloadDuration && !_unloadCompleted)
            {
                _unloadCompleted = true;
            }
        }

        public void Exit()
        {
            _unloadTimer = 0f;
            _unloadCompleted = false;
        }
        
        public bool IsUnloadCompleted => _unloadCompleted;
    }
}

