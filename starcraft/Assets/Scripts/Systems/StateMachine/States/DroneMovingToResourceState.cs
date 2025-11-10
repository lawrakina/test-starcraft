using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Systems.StateMachine.States
{
    /// <summary>
    /// Состояние движения к ресурсу
    /// Дрон движется к зарезервированному ресурсу
    /// </summary>
    public class DroneMovingToResourceState : IDroneState
    {
        private readonly IDrone _drone;
        private const float ArrivalDistance = 0.5f;

        public DroneState StateType => DroneState.MovingToResource;

        public DroneMovingToResourceState(IDrone drone)
        {
            _drone = drone;
        }

        public void Enter()
        {
            if (_drone.TargetResource != null)
            {
                _drone.SetTargetPosition(_drone.TargetResource.Position);
            }
        }

        public void Update()
        {
            if (_drone.TargetResource != null)
            {
                float distanceToResource = Vector3.Distance(_drone.Position, _drone.TargetResource.Position);
                
                if (distanceToResource <= ArrivalDistance)
                {
                    // Дрон достиг ресурса - переход к состоянию сбора будет обработан в StateMachine
                }
            }
            else
            {
                // Ресурс исчез или был собран другим дроном - возвращаемся к поиску
            }
        }

        public void Exit()
        {
            // При выходе из состояния движения ничего не делаем
        }
    }
}

