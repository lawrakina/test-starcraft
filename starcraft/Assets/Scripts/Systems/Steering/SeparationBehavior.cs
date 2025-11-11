using System.Collections.Generic;
using Core.Interfaces;
using UnityEngine;

namespace DroneResourceCollection.Systems.Steering
{
    /// <summary>
    /// Поведение разделения (Separation)
    /// Отталкивает дрона от ближайших дронов для избежания столкновений
    /// </summary>
    public class SeparationBehavior : SteeringBehavior
    {
        private readonly IDrone _drone;
        private readonly IDroneService _droneService;
        private readonly float _separationRadius;
        private readonly float _separationStrength;

        public SeparationBehavior(IDrone drone, IDroneService droneService, float separationRadius = 5f, float separationStrength = 5f)
        {
            _drone = drone;
            _droneService = droneService;
            _separationRadius = separationRadius;
            _separationStrength = separationStrength;
        }

        public override Vector3 Calculate(Vector3 position, Vector3 velocity, float maxSpeed)
        {
            // Находим ближайших дронов
            List<IDrone> nearbyDrones = _droneService.FindNearbyDrones(position, _separationRadius, _drone.Id);
            
            if (nearbyDrones.Count == 0)
            {
                return Vector3.zero;
            }

            Vector3 separationForce = Vector3.zero;
            const float minSeparationDistance = 1.5f; // Минимальное расстояние между дронами

            foreach (IDrone nearbyDrone in nearbyDrones)
            {
                Vector3 direction = position - nearbyDrone.Position;
                float distance = direction.magnitude;

                if (distance > 0 && distance < _separationRadius)
                {
                    // Если дроны очень близко, применяем экстренное отталкивание
                    if (distance < minSeparationDistance)
                    {
                        // Сильное отталкивание при очень близком расстоянии
                        direction.Normalize();
                        separationForce += direction * (_separationStrength * 3f) / (distance + 0.1f);
                    }
                    else
                    {
                        // Обычное отталкивание
                        direction.Normalize();
                        separationForce += direction / distance;
                    }
                }
            }

            // Применяем силу разделения
            if (separationForce.magnitude > 0.01f)
            {
                separationForce = separationForce.normalized * _separationStrength * Weight;
            }
            
            return separationForce;
        }
    }
}

