using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Systems.StateMachine.States
{
    public class DroneReturningState : IDroneState
    {
        private readonly IDrone _drone;
        private const float ArrivalDistance = 2.0f;
        private float _lastDistanceCheckTime;
        private const float DistanceCheckInterval = 0.2f;

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
            else
            {
                Debug.LogError($"[DroneReturningState] Drone {_drone.Id} has no HomeBase!");
            }
            _lastDistanceCheckTime = 0f;
        }

        public void Update()
        {
            if (_drone.HomeBase == null)
            {
                return;
            }

            if (Time.time - _lastDistanceCheckTime < DistanceCheckInterval)
            {
                return;
            }

            _lastDistanceCheckTime = Time.time;
            float distanceToBase = Vector3.Distance(_drone.Position, _drone.HomeBase.UnloadPoint);
            
            if (distanceToBase <= ArrivalDistance)
            {
            }
        }

        public void Exit()
        {
        }
    }
}

