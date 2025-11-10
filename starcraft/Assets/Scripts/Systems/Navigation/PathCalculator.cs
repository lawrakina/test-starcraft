using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DroneResourceCollection.Systems.Navigation
{
    /// <summary>
    /// Калькулятор путей через NavMesh
    /// </summary>
    public class PathCalculator
    {
        /// <summary>
        /// Рассчитать путь от начальной позиции до целевой
        /// </summary>
        /// <param name="from">Начальная позиция</param>
        /// <param name="to">Целевая позиция</param>
        /// <returns>Список точек пути или null, если путь не найден</returns>
        public static List<Vector3> CalculatePath(Vector3 from, Vector3 to)
        {
            NavMeshPath path = new NavMeshPath();
            
            // Пытаемся найти путь
            if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            {
                return new List<Vector3>(path.corners);
            }
            
            return null;
        }

        /// <summary>
        /// Проверить, достижима ли целевая позиция
        /// </summary>
        /// <param name="from">Начальная позиция</param>
        /// <param name="to">Целевая позиция</param>
        /// <returns>True, если путь существует</returns>
        public static bool IsReachable(Vector3 from, Vector3 to)
        {
            NavMeshPath path = new NavMeshPath();
            return NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);
        }

        /// <summary>
        /// Получить ближайшую точку на NavMesh
        /// </summary>
        /// <param name="position">Исходная позиция</param>
        /// <param name="hitPosition">Найденная позиция на NavMesh</param>
        /// <returns>True, если точка найдена</returns>
        public static bool GetNearestNavMeshPoint(Vector3 position, out Vector3 hitPosition)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, 10f, NavMesh.AllAreas))
            {
                hitPosition = hit.position;
                return true;
            }
            
            hitPosition = position;
            return false;
        }
    }
}

