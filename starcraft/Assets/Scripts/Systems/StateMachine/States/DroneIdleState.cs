using Core.Enums;
using Core.Interfaces;

namespace Systems.StateMachine.States
{
    public class DroneIdleState : IDroneState
    {
        private readonly IDrone _drone;
        private readonly IResourceService _resourceService;

        public DroneState StateType => DroneState.Idle;

        public DroneIdleState(IDrone drone, IResourceService resourceService)
        {
            _drone = drone;
            _resourceService = resourceService;
        }

        public void Enter()
        {
        }

        public void Update()
        {
        }

        public void Exit()
        {
        }
    }
}

