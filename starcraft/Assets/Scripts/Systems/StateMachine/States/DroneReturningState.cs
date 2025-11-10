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
        private const float ArrivalDistance = 1.0f;

        public DroneState StateType => DroneState.Returning;

        public DroneReturningState(IDrone drone)
        {
            _drone = drone;
        }

        public void Enter()
        {
            if (_drone.HomeBase != null)
            {
                _drone.SetTargetPosition(_drone.HomeBase.UnloadPoint);
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
                }
            }
        }

        public void Exit()
        {
            // При выходе из состояния возврата ничего не делаем
        }
    }
}

