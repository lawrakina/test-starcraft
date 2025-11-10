using Core.Enums;
using Core.Interfaces;

namespace Systems.StateMachine.States
{
    /// <summary>
    /// Состояние ожидания дрона
    /// Дрон находится в режиме ожидания команды
    /// </summary>
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
            // При входе в состояние ожидания ничего не делаем
        }

        public void Update()
        {
            // В состоянии ожидания дрон может начать поиск ресурса
            // Логика перехода будет обрабатываться в DroneStateMachine через условия
        }

        public void Exit()
        {
            // При выходе из состояния ожидания ничего не делаем
        }
    }
}

