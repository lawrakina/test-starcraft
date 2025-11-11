using Core.Interfaces;
using DroneResourceCollection.Systems.Steering;
using Systems.Collision;
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
        private AvoidanceBehavior _avoidanceBehavior;
        
        private IDrone _drone;
        private IDroneService _droneService;
        private CollisionResolver _collisionResolver;

        public SteeringManager SteeringManager => _steeringManager;

        public void Initialize(IDrone drone, IDroneService droneService, CollisionResolver collisionResolver = null)
        {
            _drone = drone;
            _droneService = droneService;
            _collisionResolver = collisionResolver ?? new CollisionResolver();
            
            // Создаем Steering Manager
            _steeringManager = new SteeringManager();
            
            // Добавляем behaviors с увеличенными параметрами для лучшего избежания столкновений
            _separationBehavior = new SeparationBehavior(_drone, _droneService, 1f, 1f);
            _alignmentBehavior = new AlignmentBehavior(_drone, _droneService, 1f);
            // Передаем CollisionResolver в AvoidanceBehavior
            _avoidanceBehavior = new AvoidanceBehavior(_drone, _droneService, _collisionResolver, 1f, 3f);
            
            // AvoidanceBehavior имеет высокий приоритет для разрешения конфликтов
            _avoidanceBehavior.Weight = 2f;
            
            _steeringManager.AddBehavior(_avoidanceBehavior);
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

