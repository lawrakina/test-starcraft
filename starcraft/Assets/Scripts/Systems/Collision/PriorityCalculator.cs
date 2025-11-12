using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Systems.Collision
{
    /// <summary>
    /// Калькулятор приоритетов для дронов
    /// </summary>
    public static class PriorityCalculator
    {
        private const float StandingVelocityThreshold = 0.1f;
        private const int StandingBonus = 10000;
        private const int CollectingBonus = 5000;
        
        /// <summary>
        /// Вычисляет эффективный приоритет дрона
        /// </summary>
        /// <param name="drone">Дрон</param>
        /// <param name="velocity">Текущая скорость дрона</param>
        /// <returns>Эффективный приоритет (чем выше, тем важнее)</returns>
        public static int CalculateEffectivePriority(IDrone drone, Vector3 velocity)
        {
            if (drone == null)
            {
                return 0;
            }
            
            int effectivePriority = drone.Priority;
            
            float speed = velocity.magnitude;
            if (speed < StandingVelocityThreshold)
            {
                effectivePriority += StandingBonus;
            }
            
            if (drone.CurrentState == DroneState.Collecting)
            {
                effectivePriority += CollectingBonus;
            }
            
            return effectivePriority;
        }
        
        /// <summary>
        /// Сравнивает приоритеты двух дронов
        /// </summary>
        /// <param name="drone1">Первый дрон</param>
        /// <param name="velocity1">Скорость первого дрона</param>
        /// <param name="drone2">Второй дрон</param>
        /// <param name="velocity2">Скорость второго дрона</param>
        /// <returns>
        /// -1 если drone1 имеет меньший приоритет (должен уступить),
        /// 1 если drone1 имеет больший приоритет,
        /// 0 если приоритеты равны (используется ID как tiebreaker)
        /// </returns>
        public static int ComparePriorities(
            IDrone drone1, Vector3 velocity1,
            IDrone drone2, Vector3 velocity2)
        {
            if (drone1 == null && drone2 == null) return 0;
            if (drone1 == null) return -1;
            if (drone2 == null) return 1;
            
            int priority1 = CalculateEffectivePriority(drone1, velocity1);
            int priority2 = CalculateEffectivePriority(drone2, velocity2);
            
            if (priority1 < priority2)
            {
                return -1; // drone1 должен уступить
            }
            
            if (priority1 > priority2)
            {
                return 1; // drone2 должен уступить
            }
            
            if (drone1.Id < drone2.Id)
            {
                return 1;
            }
            
            if (drone1.Id > drone2.Id)
            {
                return -1;
            }
            
            return 0;
        }
        
        /// <summary>
        /// Определяет, должен ли первый дрон уступить дорогу второму
        /// </summary>
        /// <param name="drone1">Первый дрон</param>
        /// <param name="velocity1">Скорость первого дрона</param>
        /// <param name="drone2">Второй дрон</param>
        /// <param name="velocity2">Скорость второго дрона</param>
        /// <returns>true если drone1 должен уступить</returns>
        public static bool ShouldYield(
            IDrone drone1, Vector3 velocity1,
            IDrone drone2, Vector3 velocity2)
        {
            return ComparePriorities(drone1, velocity1, drone2, velocity2) < 0;
        }
        
    /// <summary>
    /// Проверяет, является ли дрон стоящим
    /// </summary>
        /// <param name="velocity">Скорость дрона</param>
        /// <returns>true если дрон стоит</returns>
        public static bool IsStanding(Vector3 velocity)
        {
            return velocity.magnitude < StandingVelocityThreshold;
        }
    }
}

