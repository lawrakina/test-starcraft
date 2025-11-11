using Core.Enums;
using Core.Interfaces;
using Systems.StateMachine.States;
using UnityEngine;

namespace Entities.Drone
{
    public class DroneStateMachine : MonoBehaviour, IUpdatable
    {
        [SerializeField] private int updatePriority = 100;
        
        private Systems.StateMachine.DroneStateMachine _stateMachine;
        private DroneIdleState _idleState;
        private DroneSearchingState _searchingState;
        private DroneMovingToResourceState _movingToResourceState;
        private DroneCollectingState _collectingState;
        private DroneReturningState _returningState;
        private DroneUnloadingState _unloadingState;
        
        private IDrone _drone;
        private IResourceService _resourceService;

        public Systems.StateMachine.DroneStateMachine StateMachine => _stateMachine;
        public DroneState CurrentState => _stateMachine?.CurrentDroneState ?? DroneState.Idle;
        public int UpdatePriority => updatePriority;
        
        public void ForceSearchingState()
        {
            if (_stateMachine != null)
            {
                if (_searchingState != null)
                {
                    _stateMachine.ChangeState(_searchingState);
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
            
            _stateMachine.ChangeState(_searchingState);
        }

        private DroneState _lastCheckedState;
        private float _lastTransitionCheckTime;
        private const float TransitionCheckInterval = 0.1f;
        
        private void OnEnable()
        {
            var updateManager = Core.DI.DependencyHelper.GetUpdateManager();
            if (updateManager != null)
            {
                updateManager.RegisterUpdatable(this);
            }
        }
        
        private void OnDisable()
        {
            var updateManager = Core.DI.DependencyHelper.GetUpdateManager();
            if (updateManager != null)
            {
                updateManager.UnregisterUpdatable(this);
            }
        }

        public void OnUpdate(float deltaTime)
        {
            if (_stateMachine == null)
            {
                return;
            }

            DroneState previousState = CurrentState;
            _stateMachine.Update();
            
            if (Time.time - _lastTransitionCheckTime >= TransitionCheckInterval || CurrentState != _lastCheckedState)
            {
                _lastTransitionCheckTime = Time.time;
                _lastCheckedState = CurrentState;
                HandleStateTransitions();
            }
        }
        
        private bool IsTargetResourceValid()
        {
            if (_drone.TargetResource == null)
            {
                return false;
            }
            
            if (_drone.TargetResource is MonoBehaviour resourceMono)
            {
                return resourceMono != null;
            }
            
            return true;
        }
        
        private bool IsTargetResourceAvailable()
        {
            if (_drone.TargetResource == null)
            {
                return false;
            }
            
            if (_drone.TargetResource.IsCollected)
            {
                return false;
            }
            
            if (_drone.TargetResource is DroneResourceCollection.Entities.Resource.Resource resource)
            {
                return resource.IsAvailableForDrone(_drone.Id, _drone.Faction);
            }
            
            return !_drone.TargetResource.IsReserved;
        }
        
        private int _cachedAvailableResourceCount;
        private float _lastResourceCountCacheTime;
        private const float ResourceCountCacheInterval = 0.5f;

        private void HandleStateTransitions()
        {
            DroneState currentState = _stateMachine.CurrentDroneState;

            if (Time.time - _lastResourceCountCacheTime >= ResourceCountCacheInterval)
            {
                _lastResourceCountCacheTime = Time.time;
                _cachedAvailableResourceCount = _resourceService.AvailableResourceCount;
            }

            switch (currentState)
            {
                case DroneState.Idle:
                    if (_cachedAvailableResourceCount > 0)
                    {
                        _stateMachine.ChangeState(_searchingState);
                    }
                    break;

                case DroneState.Searching:
                    if (IsTargetResourceValid())
                    {
                        _stateMachine.ChangeState(_movingToResourceState);
                    }
                    else if (_cachedAvailableResourceCount == 0)
                    {
                        _stateMachine.ChangeState(_idleState);
                    }
                    break;

                case DroneState.MovingToResource:
                    if (IsTargetResourceValid() && IsTargetResourceAvailable())
                    {
                        float distance = Vector3.Distance(_drone.Position, _drone.TargetResource.Position);
                        if (distance <= 1.5f)
                        {
                            _stateMachine.ChangeState(_collectingState);
                        }
                    }
                    else
                    {
                        _drone.ClearTargetResource();
                        _stateMachine.ChangeState(_searchingState);
                    }
                    break;

                case DroneState.Collecting:
                    if (IsTargetResourceValid() && _drone.TargetResource.IsCollected)
                    {
                        _drone.ClearTargetResource();
                        _stateMachine.ChangeState(_returningState);
                    }
                    else if (!IsTargetResourceValid() || !IsTargetResourceAvailable())
                    {
                        _drone.ClearTargetResource();
                        _stateMachine.ChangeState(_searchingState);
                    }
                    break;

                case DroneState.Returning:
                    if (_drone.HomeBase == null)
                    {
                        Debug.LogWarning($"[DroneStateMachine] Drone {_drone.Id} in Returning state but HomeBase is null!");
                    }
                    break;

                case DroneState.Unloading:
                    if (_stateMachine.CurrentState is DroneUnloadingState unloadingState && unloadingState.IsUnloadCompleted)
                    {
                        _stateMachine.ChangeState(_searchingState);
                    }
                    break;
            }
        }
    }
}

