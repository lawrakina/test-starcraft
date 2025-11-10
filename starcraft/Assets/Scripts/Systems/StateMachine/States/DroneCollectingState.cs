using Core.Enums;
using Core.Events;
using Core.Interfaces;
using UnityEngine;

namespace Systems.StateMachine.States
{
    /// <summary>
    /// Состояние сбора ресурса
    /// Дрон остается на месте 2 секунды для "сбора" ресурса
    /// </summary>
    public class DroneCollectingState : IDroneState
    {
        private readonly IDrone _drone;
        private float _collectTimer;
        private const float CollectDuration = 2.0f; // Время сбора ресурса в секундах

        public DroneState StateType => DroneState.Collecting;

        public DroneCollectingState(IDrone drone)
        {
            _drone = drone;
        }

        public void Enter()
        {
            _collectTimer = 0f;
        }

        public void Update()
        {
            _collectTimer += Time.deltaTime;
            
            if (_collectTimer >= CollectDuration)
            {
                if (_drone.TargetResource != null)
                {
                    var resource = _drone.TargetResource;
                    resource.Collect();
                    
                    EventBus.Instance.Publish(new ResourceCollectedEvent(_drone, resource, _drone.Faction));
                }
            }
        }

        public void Exit()
        {
            _collectTimer = 0f;
        }
    }
}

