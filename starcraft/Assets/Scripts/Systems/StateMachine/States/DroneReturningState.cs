using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Systems.StateMachine.States
{
    /// <summary>
    /// Состояние возврата на базу
    /// Дрон движется обратно на свою базу для выгрузки ресурса
    /// </summary>
    public class DroneReturningState : IDroneState
    {
        private readonly IDrone _drone;
        private const float ArrivalDistance = 2.0f; // Радиус сдачи ресурсов от базы

        public DroneState StateType => DroneState.Returning;

        public DroneReturningState(IDrone drone)
        {
            _drone = drone;
        }

        public void Enter()
        {
            Debug.Log($"[DroneReturningState] Drone {_drone.Id} entering Returning state");
            if (_drone.HomeBase != null)
            {
                Debug.Log($"[DroneReturningState] Drone {_drone.Id} setting target to base unload point: {_drone.HomeBase.UnloadPoint}");
                _drone.SetTargetPosition(_drone.HomeBase.UnloadPoint);
            }
            else
            {
                Debug.LogError($"[DroneReturningState] Drone {_drone.Id} has no HomeBase!");
            }
        }

        public void Update()
        {
            if (_drone.HomeBase != null)
            {
                float distanceToBase = Vector3.Distance(_drone.Position, _drone.HomeBase.UnloadPoint);
                
                if (distanceToBase <= ArrivalDistance)
                {
                    // Дрон достиг базы - переход к состоянию выгрузки будет обработан в StateMachine
                    Debug.Log($"[DroneReturningState] Drone {_drone.Id} reached base (distance: {distanceToBase:F2} <= {ArrivalDistance})");
                }
            }
            else
            {
                Debug.LogWarning($"[DroneReturningState] Drone {_drone.Id} in Returning state but HomeBase is null!");
            }
        }

        public void Exit()
        {
            // При выходе из состояния возврата ничего не делаем
        }
    }
}

