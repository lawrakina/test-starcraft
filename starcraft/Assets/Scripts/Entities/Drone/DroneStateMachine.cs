using Core.Enums;
using Core.Interfaces;
using Systems.StateMachine.States;
using UnityEngine;

namespace Entities.Drone
{
    /// <summary>
    /// Компонент State Machine для дрона
    /// Управляет переходами между состояниями
    /// </summary>
    public class DroneStateMachine : MonoBehaviour
    {
        private Systems.StateMachine.DroneStateMachine _stateMachine;
        private DroneIdleState _idleState;
        private DroneSearchingState _searchingState;
        private DroneMovingToResourceState _movingToResourceState;
        private DroneCollectingState _collectingState;
        private DroneReturningState _returningState;
        private DroneUnloadingState _unloadingState;
        
        private IDrone _drone;
        private IResourceService _resourceService;
        
        // Для ограничения частоты логирования в Returning state
        private float _lastReturningLogTime = 0f;
        private const float ReturningLogInterval = 2f; // Логируем каждые 2 секунды

        public Systems.StateMachine.DroneStateMachine StateMachine => _stateMachine;
        public DroneState CurrentState => _stateMachine?.CurrentDroneState ?? DroneState.Idle;
        
        /// <summary>
        /// Принудительно переключает дрон на состояние поиска ресурсов
        /// Используется для переключения после сдачи ресурсов на базе
        /// </summary>
        public void ForceSearchingState()
        {
            Debug.Log($"[DroneStateMachine] ForceSearchingState() called for drone {_drone.Id}");
            Debug.Log($"[DroneStateMachine] Current state before change: {CurrentState}");
            Debug.Log($"[DroneStateMachine] StateMachine is null: {_stateMachine == null}");
            Debug.Log($"[DroneStateMachine] SearchingState is null: {_searchingState == null}");
            
            if (_stateMachine != null)
            {
                if (_searchingState != null)
                {
                    _stateMachine.ChangeState(_searchingState);
                    Debug.Log($"[DroneStateMachine] State changed to Searching. New state: {CurrentState}");
                }
                else
                {
                    Debug.LogError($"[DroneStateMachine] SearchingState is null! Cannot change state for drone {_drone.Id}");
                }
            }
            else
            {
                Debug.LogError($"[DroneStateMachine] StateMachine is null! Cannot change state for drone {_drone.Id}");
            }
        }

        public void Initialize(IDrone drone, IResourceService resourceService)
        {
            _drone = drone;
            _resourceService = resourceService;
            
            _stateMachine = new Systems.StateMachine.DroneStateMachine(drone);
            
            _idleState = new DroneIdleState(drone, resourceService);
            _searchingState = new DroneSearchingState(drone, resourceService);
            _movingToResourceState = new DroneMovingToResourceState(drone);
            _collectingState = new DroneCollectingState(drone);
            _returningState = new DroneReturningState(drone);
            _unloadingState = new DroneUnloadingState(drone);
            
            // Дроны сразу начинают работу - переходят в состояние поиска ресурсов
            _stateMachine.ChangeState(_searchingState);
        }

        private void Update()
        {
            if (_stateMachine == null)
            {
                return;
            }

            DroneState previousState = CurrentState;
            _stateMachine.Update();
            
            HandleStateTransitions();
            
            // Логируем изменение состояния
            if (CurrentState != previousState)
            {
                Debug.Log($"[DroneStateMachine] Drone {_drone.Id} state changed: {previousState} -> {CurrentState}");
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
        
        private void HandleStateTransitions()
        {
            DroneState currentState = _stateMachine.CurrentDroneState;

            switch (currentState)
            {
                case DroneState.Idle:
                    if (_resourceService.AvailableResourceCount > 0)
                    {
                        Debug.Log($"[DroneStateMachine] Drone {_drone.Id} transitioning from Idle to Searching (resources available: {_resourceService.AvailableResourceCount})");
                        _stateMachine.ChangeState(_searchingState);
                    }
                    break;

                case DroneState.Searching:
                    if (IsTargetResourceValid())
                    {
                        Debug.Log($"[DroneStateMachine] Drone {_drone.Id} transitioning from Searching to MovingToResource (found resource)");
                        _stateMachine.ChangeState(_movingToResourceState);
                    }
                    // Если нет доступных ресурсов, переходим в Idle
                    else if (_resourceService.AvailableResourceCount == 0)
                    {
                        Debug.Log($"[DroneStateMachine] Drone {_drone.Id} transitioning from Searching to Idle (no resources available)");
                        _stateMachine.ChangeState(_idleState);
                    }
                    break;

                case DroneState.MovingToResource:
                    if (IsTargetResourceValid() && IsTargetResourceAvailable())
                    {
                        float distance = Vector3.Distance(_drone.Position, _drone.TargetResource.Position);
                        // Начинаем сбор ресурсов на расстоянии 1.5 единицы
                        if (distance <= 1.5f)
                        {
                            Debug.Log($"[DroneStateMachine] Drone {_drone.Id} transitioning from MovingToResource to Collecting (distance: {distance:F2})");
                            _stateMachine.ChangeState(_collectingState);
                        }
                    }
                    else
                    {
                        // Ресурс исчез, был собран другим дроном или занят - возвращаемся к поиску
                        Debug.Log($"[DroneStateMachine] Drone {_drone.Id} transitioning from MovingToResource to Searching (resource invalid or unavailable)");
                        _drone.ClearTargetResource();
                        _stateMachine.ChangeState(_searchingState);
                    }
                    break;

                case DroneState.Collecting:
                    // Проверяем, собран ли ресурс (после 5 секунд сбора)
                    if (IsTargetResourceValid() && _drone.TargetResource.IsCollected)
                    {
                        Debug.Log($"[DroneStateMachine] Drone {_drone.Id} transitioning from Collecting to Returning (resource collected)");
                        _drone.ClearTargetResource();
                        _stateMachine.ChangeState(_returningState);
                    }
                    // Если ресурс исчез, был собран другим дроном или занят, возвращаемся к поиску
                    else if (!IsTargetResourceValid() || !IsTargetResourceAvailable())
                    {
                        Debug.Log($"[DroneStateMachine] Drone {_drone.Id} transitioning from Collecting to Searching (resource invalid or unavailable)");
                        _drone.ClearTargetResource();
                        _stateMachine.ChangeState(_searchingState);
                    }
                    break;

                case DroneState.Returning:
                    // Сдача ресурсов теперь происходит через триггер-коллайдер на базе
                    // (BaseUnloadTrigger), поэтому здесь не нужно проверять расстояние
                    // Дрон будет автоматически переключен на Searching после сдачи ресурсов
                    // Логируем состояние для отладки (ограниченная частота)
                    if (Time.time - _lastReturningLogTime >= ReturningLogInterval)
                    {
                        _lastReturningLogTime = Time.time;
                        if (_drone.HomeBase != null)
                        {
                            float distanceToBase = Vector3.Distance(_drone.Position, _drone.HomeBase.UnloadPoint);
                            bool hasResource = _drone.TargetResource != null || 
                                              (_drone is MonoBehaviour droneMono && 
                                               droneMono.GetComponentInChildren<DroneResourceCollection.Entities.Resource.Resource>() != null);
                            //Debug.Log($"[DroneStateMachine] Drone {_drone.Id} in Returning state - Distance to base: {distanceToBase:F2}, HasResource: {hasResource}");
                        }
                        else
                        {
                            Debug.LogWarning($"[DroneStateMachine] Drone {_drone.Id} in Returning state but HomeBase is null!");
                        }
                    }
                    break;

                case DroneState.Unloading:
                    // Переходим к поиску только после завершения выгрузки
                    if (_stateMachine.CurrentState is DroneUnloadingState unloadingState && unloadingState.IsUnloadCompleted)
                    {
                        Debug.Log($"[DroneStateMachine] Drone {_drone.Id} transitioning from Unloading to Searching (unload completed)");
                        _stateMachine.ChangeState(_searchingState);
                    }
                    break;
            }
        }
    }
}

