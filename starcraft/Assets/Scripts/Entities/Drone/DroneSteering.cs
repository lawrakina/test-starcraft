using Core.Interfaces;
using DroneResourceCollection.Systems.Steering;
using UnityEngine;

namespace DroneResourceCollection.Entities.Drone
{
    /// <summary>
    /// Компонент Steering Behaviors для дрона
    /// Управляет поведением избежания столкновений
    /// </summary>
    public class DroneSteering : MonoBehaviour
    {
        private SteeringManager _steeringManager;
        private SeparationBehavior _separationBehavior;
        private AlignmentBehavior _alignmentBehavior;
        
        private IDrone _drone;
        private IDroneService _droneService;

        public SteeringManager SteeringManager => _steeringManager;

        public void Initialize(IDrone drone, IDroneService droneService)
        {
            _drone = drone;
            _droneService = droneService;
            
            // Создаем Steering Manager
            _steeringManager = new SteeringManager();
            
            // Добавляем behaviors
            _separationBehavior = new SeparationBehavior(_drone, _droneService, 3f, 2f);
            _alignmentBehavior = new AlignmentBehavior(_drone, _droneService, 5f);
            
            _steeringManager.AddBehavior(_separationBehavior);
            _steeringManager.AddBehavior(_alignmentBehavior);
        }

        private void OnDestroy()
        {
            if (_steeringManager != null)
            {
                _steeringManager.Clear();
            }
        }
    }
}

