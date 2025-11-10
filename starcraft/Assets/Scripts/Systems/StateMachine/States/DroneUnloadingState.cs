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
        private bool _unloadComplete;

        public DroneState StateType => DroneState.Unloading;

        public DroneUnloadingState(IDrone drone)
        {
            _drone = drone;
        }

        public void Enter()
        {
            _unloadComplete = false;
            
            if (_drone.HomeBase != null)
            {
                _drone.HomeBase.AddResource();
                _unloadComplete = true;
                
                if (_drone is MonoBehaviour droneMono)
                {
                    DroneVisuals visuals = droneMono.GetComponent<DroneVisuals>();
                    if (visuals)
                    {
                        visuals.PlayUnloadEffect();
                    }
                }
            }
        }

        public void Update()
        {
            // Выгрузка происходит мгновенно при входе в состояние
            // Визуальные эффекты могут длиться дольше, но логически выгрузка завершена
        }

        public void Exit()
        {
            _unloadComplete = false;
        }
    }
}

