using Core.Enums;
using Core.Events;
using Core.Interfaces;
using UnityEngine;

namespace Systems.StateMachine.States
{
    /// <summary>
    /// Состояние сбора ресурса
    /// Дрон остается на месте 5 секунд для "сбора" ресурса
    /// </summary>
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
            
            // Останавливаем движение дрона во время сбора
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
                    
                    // Прикрепляем ресурс к дрону
                    if (resource is MonoBehaviour resourceMono && _drone is MonoBehaviour droneMono)
                    {
                        if (resource is DroneResourceCollection.Entities.Resource.Resource resourceComponent)
                        {
                            resourceComponent.AttachToDrone(droneMono.transform);
                        }
                    }
                    
                    EventBus.Instance.Publish(new ResourceCollectedEvent(_drone, resource, _drone.Faction));
                }
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
            _collectTimer = 0f;
        }
        
        /// <summary>
        /// Получить прогресс сбора (0.0 - 1.0)
        /// </summary>
        public float GetProgress()
        {
            return Mathf.Clamp01(_collectTimer / CollectDuration);
        }
    }
}

