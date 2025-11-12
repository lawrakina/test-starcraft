using System.Collections.Generic;
using UnityEngine;

namespace DroneResourceCollection.Systems.Steering
{
    /// <summary>
    /// Менеджер Steering Behaviors
    /// </summary>
    public class SteeringManager
    {
        private readonly List<SteeringBehavior> _behaviors = new List<SteeringBehavior>();

    /// <summary>
    /// Добавляет поведение
    /// </summary>
        public void AddBehavior(SteeringBehavior behavior)
        {
            if (behavior != null && !_behaviors.Contains(behavior))
            {
                _behaviors.Add(behavior);
            }
        }

    /// <summary>
    /// Удаляет поведение
    /// </summary>
        public void RemoveBehavior(SteeringBehavior behavior)
        {
            _behaviors.Remove(behavior);
        }

    /// <summary>
    /// Вычисляет итоговый steering force
    /// </summary>
        public Vector3 CalculateSteering(Vector3 position, Vector3 velocity, float maxSpeed)
        {
            Vector3 totalForce = Vector3.zero;

            foreach (SteeringBehavior behavior in _behaviors)
            {
                totalForce += behavior.Calculate(position, velocity, maxSpeed);
            }

            if (totalForce.magnitude > maxSpeed)
            {
                totalForce = totalForce.normalized * maxSpeed;
            }

            return totalForce;
        }

    /// <summary>
    /// Очищает все поведения
    /// </summary>
        public void Clear()
        {
            _behaviors.Clear();
        }

    /// <summary>
    /// Получает поведение определенного типа
    /// </summary>
        public T GetBehavior<T>() where T : SteeringBehavior
        {
            foreach (var behavior in _behaviors)
            {
                if (behavior is T typedBehavior)
                {
                    return typedBehavior;
                }
            }
            return null;
        }
    }
}

