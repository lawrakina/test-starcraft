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

        public Systems.StateMachine.DroneStateMachine StateMachine => _stateMachine;
        public DroneState CurrentState => _stateMachine?.CurrentDroneState ?? DroneState.Idle;

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
            
            _stateMachine.ChangeState(_idleState);
        }

        private void Update()
        {
            if (_stateMachine == null)
            {
                return;
            }

            _stateMachine.Update();
            
            HandleStateTransitions();
        }

        private void HandleStateTransitions()
        {
            DroneState currentState = _stateMachine.CurrentDroneState;

            switch (currentState)
            {
                case DroneState.Idle:
                    if (_resourceService.AvailableResourceCount > 0)
                    {
                        _stateMachine.ChangeState(_searchingState);
                    }
                    break;

                case DroneState.Searching:
                    if (_drone.TargetResource != null)
                    {
                        _stateMachine.ChangeState(_movingToResourceState);
                    }
                    break;

                case DroneState.MovingToResource:
                    if (_drone.TargetResource != null)
                    {
                        float distance = Vector3.Distance(_drone.Position, _drone.TargetResource.Position);
                        if (distance <= 0.5f)
                        {
                            _stateMachine.ChangeState(_collectingState);
                        }
                    }
                    else
                    {
                        _stateMachine.ChangeState(_searchingState);
                    }
                    break;

                case DroneState.Collecting:
                    if (_drone.TargetResource == null || _drone.TargetResource.IsCollected)
                    {
                        _drone.ClearTargetResource();
                        _stateMachine.ChangeState(_returningState);
                    }
                    break;

                case DroneState.Returning:
                    if (_drone.HomeBase != null)
                    {
                        float distance = Vector3.Distance(_drone.Position, _drone.HomeBase.UnloadPoint);
                        if (distance <= 1.0f)
                        {
                            _stateMachine.ChangeState(_unloadingState);
                        }
                    }
                    break;

                case DroneState.Unloading:
                    _stateMachine.ChangeState(_searchingState);
                    break;
            }
        }
    }
}

