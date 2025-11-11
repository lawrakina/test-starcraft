using Core.Enums;
using Core.Interfaces;
using DroneResourceCollection.Entities.Drone;
using UnityEngine;

namespace Systems.StateMachine.States
{
    /// <summary>
    /// Состояние выгрузки ресурса
    /// Дрон выгружает ресурс на базе (с визуальным эффектом)
    /// </summary>
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
            Debug.Log($"[DroneUnloadingState] Drone {_drone.Id} entering Unloading state");
            _unloadTimer = 0f;
            _unloadCompleted = false;
            
            if (_drone.HomeBase != null)
            {
                Debug.Log($"[DroneUnloadingState] Drone {_drone.Id} adding resource to base");
                _drone.HomeBase.AddResource();
                
                // Уничтожаем ресурс после выгрузки
                if (_drone.TargetResource != null)
                {
                    Debug.Log($"[DroneUnloadingState] Drone {_drone.Id} destroying TargetResource {_drone.TargetResource.Id}");
                    if (_drone.TargetResource is DroneResourceCollection.Entities.Resource.Resource resourceComponent)
                    {
                        resourceComponent.DestroyAfterUnload();
                    }
                }
                else
                {
                    Debug.LogWarning($"[DroneUnloadingState] Drone {_drone.Id} in Unloading state but TargetResource is null");
                }
                
                if (_drone is MonoBehaviour droneMono)
                {
                    DroneVisuals visuals = droneMono.GetComponent<DroneVisuals>();
                    if (visuals)
                    {
                        Debug.Log($"[DroneUnloadingState] Drone {_drone.Id} playing unload effect");
                        visuals.PlayUnloadEffect();
                    }
                    else
                    {
                        Debug.LogWarning($"[DroneUnloadingState] DroneVisuals not found for drone {_drone.Id}");
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
            
            // Выгрузка завершена после небольшой задержки
            if (_unloadTimer >= UnloadDuration && !_unloadCompleted)
            {
                _unloadCompleted = true;
                Debug.Log($"[DroneUnloadingState] Drone {_drone.Id} unload completed after {_unloadTimer:F2} seconds");
            }
        }

        public void Exit()
        {
            Debug.Log($"[DroneUnloadingState] Drone {_drone.Id} exiting Unloading state");
            _unloadTimer = 0f;
            _unloadCompleted = false;
        }
        
        /// <summary>
        /// Проверяет, завершена ли выгрузка
        /// </summary>
        public bool IsUnloadCompleted => _unloadCompleted;
    }
}

