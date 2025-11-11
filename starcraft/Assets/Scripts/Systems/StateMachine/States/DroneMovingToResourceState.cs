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
        private const float ArrivalDistance = 1.5f; // Расстояние начала сбора ресурсов

        public DroneState StateType => DroneState.MovingToResource;

        public DroneMovingToResourceState(IDrone drone)
        {
            _drone = drone;
        }

        public void Enter()
        {
            if (IsTargetResourceValid() && IsTargetResourceAvailable())
            {
                _drone.SetTargetPosition(_drone.TargetResource.Position);
            }
        }

        public void Update()
        {
            if (IsTargetResourceValid() && IsTargetResourceAvailable())
            {
                // Постоянно обновляем цель движения к актуальной позиции ресурса
                _drone.SetTargetPosition(_drone.TargetResource.Position);
                
                float distanceToResource = Vector3.Distance(_drone.Position, _drone.TargetResource.Position);
                
                if (distanceToResource <= ArrivalDistance)
                {
                    // Дрон достиг ресурса - переход к состоянию сбора будет обработан в StateMachine
                    // Останавливаем движение, чтобы дрон не продолжал двигаться
                    _drone.SetTargetPosition(_drone.Position);
                }
            }
            else
            {
                // Ресурс исчез, был собран другим дроном или занят - возвращаемся к поиску
            }
        }
        
        /// <summary>
        /// Проверяет, является ли целевой ресурс валидным (не null и не уничтожен)
        /// </summary>
        private bool IsTargetResourceValid()
        {
            if (_drone.TargetResource == null)
            {
                return false;
            }
            
            // Если ресурс - MonoBehaviour, проверяем, что он не уничтожен
            if (_drone.TargetResource is MonoBehaviour resourceMono)
            {
                return resourceMono != null;
            }
            
            // Для других реализаций считаем валидным, если не null
            return true;
        }
        
        /// <summary>
        /// Проверяет, доступен ли целевой ресурс для этого дрона
        /// Ресурс должен быть не собран и либо не зарезервирован, либо зарезервирован именно этим дроном
        /// </summary>
        private bool IsTargetResourceAvailable()
        {
            if (_drone.TargetResource == null)
            {
                return false;
            }
            
            // Если ресурс собран, он недоступен
            if (_drone.TargetResource.IsCollected)
            {
                return false;
            }
            
            // Для Resource компонента используем специальную проверку
            if (_drone.TargetResource is DroneResourceCollection.Entities.Resource.Resource resource)
            {
                return resource.IsAvailableForDrone(_drone.Id, _drone.Faction);
            }
            
            // Для других реализаций проверяем только, что ресурс не зарезервирован
            return !_drone.TargetResource.IsReserved;
        }

        public void Exit()
        {
            // При выходе из состояния движения ничего не делаем
        }
    }
}

