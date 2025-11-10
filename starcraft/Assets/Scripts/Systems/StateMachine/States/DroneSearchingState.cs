using Core.Enums;
using Core.Interfaces;

namespace Systems.StateMachine.States
{
    /// <summary>
    /// Состояние поиска ресурса
    /// Дрон ищет ближайший доступный ресурс
    /// </summary>
    public class DroneSearchingState : IDroneState
    {
        private readonly IDrone _drone;
        private readonly IResourceService _resourceService;

        public DroneState StateType => DroneState.Searching;

        public DroneSearchingState(IDrone drone, IResourceService resourceService)
        {
            _drone = drone;
            _resourceService = resourceService;
        }

        public void Enter()
        {
            _drone.ClearTargetResource();
        }

        public void Update()
        {
            var nearestResource = _resourceService.FindNearestAvailableResource(_drone.Position);
            
            if (nearestResource is { IsReserved: false })
            {
                // Резервируем ресурс и устанавливаем его как цель
                if (nearestResource.Reserve(_drone.Id))
                {
                    _drone.SetTargetResource(nearestResource);
                }
            }
        }

        public void Exit()
        {
            // При выходе из состояния поиска ничего не делаем
        }
    }
}

