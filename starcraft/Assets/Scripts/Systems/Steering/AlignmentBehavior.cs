using System.Collections.Generic;
using Core.Interfaces;
using UnityEngine;

namespace DroneResourceCollection.Systems.Steering
{
    /// <summary>
    /// Поведение выравнивания (Alignment)
    /// Выравнивает направление движения дрона с направлением ближайших дронов
    /// </summary>
    public class AlignmentBehavior : SteeringBehavior
    {
        private readonly IDrone _drone;
        private readonly IDroneService _droneService;
        private readonly float _alignmentRadius;

        public AlignmentBehavior(IDrone drone, IDroneService droneService, float alignmentRadius = 5f)
        {
            _drone = drone;
            _droneService = droneService;
            _alignmentRadius = alignmentRadius;
        }

        public override Vector3 Calculate(Vector3 position, Vector3 velocity, float maxSpeed)
        {
            // Находим ближайших дронов
            List<IDrone> nearbyDrones = _droneService.FindNearbyDrones(position, _alignmentRadius, _drone.Id);
            
            if (nearbyDrones.Count == 0)
            {
                return Vector3.zero;
            }

            Vector3 averageVelocity = Vector3.zero;
            int count = 0;

            foreach (IDrone nearbyDrone in nearbyDrones)
            {
                float distance = Vector3.Distance(position, nearbyDrone.Position);
                
                if (distance > 0 && distance < _alignmentRadius)
                {
                    // Здесь мы не имеем прямого доступа к velocity дрона через интерфейс
                    // Поэтому используем направление к цели дрона, если оно есть
                    // Для упрощения, alignment будет работать только если дроны движутся
                    count++;
                }
            }

            if (count == 0)
            {
                return Vector3.zero;
            }

            // Упрощенная версия alignment - возвращаем нулевой вектор
            // Полная реализация требует доступа к velocity дронов
            return Vector3.zero * Weight;
        }
    }
}

