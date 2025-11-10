using UnityEngine;

namespace DroneResourceCollection.Systems.Steering
{
    /// <summary>
    /// Базовый класс для Steering Behaviors
    /// Реализует паттерн Strategy для различных типов поведения
    /// </summary>
    public abstract class SteeringBehavior
    {
        /// <summary>
        /// Вес поведения (влияние на итоговый вектор)
        /// </summary>
        public float Weight { get; set; } = 1.0f;

        /// <summary>
        /// Вычислить steering force для данного поведения
        /// </summary>
        /// <param name="position">Текущая позиция агента</param>
        /// <param name="velocity">Текущая скорость агента</param>
        /// <param name="maxSpeed">Максимальная скорость</param>
        /// <returns>Вектор steering force</returns>
        public abstract Vector3 Calculate(Vector3 position, Vector3 velocity, float maxSpeed);
    }
}

