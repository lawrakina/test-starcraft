using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Systems.Navigation
{
    /// <summary>
    /// Менеджер временных препятствий для NavMesh
    /// Управляет динамическим добавлением/удалением препятствий (например, ресурсов)
    /// </summary>
    public class TemporaryObstacleManager
    {
        private readonly Dictionary<GameObject, NavMeshObstacle> _obstacles = new Dictionary<GameObject, NavMeshObstacle>();
        private readonly float _obstacleRadius;

        public TemporaryObstacleManager(float obstacleRadius = 0.5f)
        {
            _obstacleRadius = obstacleRadius;
        }

        /// <summary>
        /// Добавить временное препятствие
        /// </summary>
        /// <param name="obstacle">GameObject препятствия</param>
        public void AddObstacle(GameObject obstacle)
        {
            if (!obstacle || _obstacles.ContainsKey(obstacle))
            {
                return;
            }

            var navObstacle = obstacle.GetComponent<NavMeshObstacle>();
            if (!navObstacle)
            {
                navObstacle = obstacle.AddComponent<NavMeshObstacle>();
            }

            navObstacle.shape = NavMeshObstacleShape.Capsule;
            navObstacle.radius = _obstacleRadius;
            navObstacle.height = 2f;
            navObstacle.carving = true; // Препятствие "вырезает" NavMesh
            navObstacle.carveOnlyStationary = false;

            _obstacles[obstacle] = navObstacle;
        }

        /// <summary>
        /// Удалить временное препятствие
        /// </summary>
        /// <param name="obstacle">GameObject препятствия</param>
        public void RemoveObstacle(GameObject obstacle)
        {
            if (!obstacle || !_obstacles.TryGetValue(obstacle, out var navObstacle))
            {
                return;
            }

            // Отключаем carving перед удалением, чтобы NavMesh обновился
            if (navObstacle)
            {
                navObstacle.carving = false;
            }

            _obstacles.Remove(obstacle);

            if (navObstacle)
            {
                Object.Destroy(navObstacle);
            }
        }

        /// <summary>
        /// Очистить все препятствия
        /// </summary>
        public void ClearAll()
        {
            foreach (var obstacle in _obstacles.Keys.Where(obstacle => obstacle))
            {
                RemoveObstacle(obstacle);
            }

            _obstacles.Clear();
        }
    }
}

