using System.Collections.Generic;
using UnityEngine;

namespace DroneResourceCollection.Systems.Steering
{
    /// <summary>
    /// Менеджер Steering Behaviors
    /// Комбинирует несколько behaviors для получения итогового steering force
    /// </summary>
    public class SteeringManager
    {
        private readonly List<SteeringBehavior> _behaviors = new List<SteeringBehavior>();

        /// <summary>
        /// Добавить поведение
        /// </summary>
        public void AddBehavior(SteeringBehavior behavior)
        {
            if (behavior != null && !_behaviors.Contains(behavior))
            {
                _behaviors.Add(behavior);
            }
        }

        /// <summary>
        /// Удалить поведение
        /// </summary>
        public void RemoveBehavior(SteeringBehavior behavior)
        {
            _behaviors.Remove(behavior);
        }

        /// <summary>
        /// Вычислить итоговый steering force, комбинируя все поведения
        /// </summary>
        /// <param name="position">Текущая позиция</param>
        /// <param name="velocity">Текущая скорость</param>
        /// <param name="maxSpeed">Максимальная скорость</param>
        /// <returns>Итоговый steering force</returns>
        public Vector3 CalculateSteering(Vector3 position, Vector3 velocity, float maxSpeed)
        {
            Vector3 totalForce = Vector3.zero;

            foreach (SteeringBehavior behavior in _behaviors)
            {
                totalForce += behavior.Calculate(position, velocity, maxSpeed);
            }

            // Ограничиваем максимальную силу
            if (totalForce.magnitude > maxSpeed)
            {
                totalForce = totalForce.normalized * maxSpeed;
            }

            return totalForce;
        }

        /// <summary>
        /// Очистить все поведения
        /// </summary>
        public void Clear()
        {
            _behaviors.Clear();
        }
    }
}

