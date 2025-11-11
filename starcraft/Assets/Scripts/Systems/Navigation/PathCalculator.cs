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
        /// Путь разделяется на промежутки с учетом ландшафта для более плавного движения
        /// </summary>
        /// <param name="from">Начальная позиция</param>
        /// <param name="to">Целевая позиция</param>
        /// <param name="segmentLength">Длина сегмента пути для разделения (по умолчанию 2.0f)</param>
        /// <returns>Список точек пути или null, если путь не найден</returns>
        public static List<Vector3> CalculatePath(Vector3 from, Vector3 to, float segmentLength = 2.0f)
        {
            NavMeshPath path = new NavMeshPath();
            
            // Пытаемся найти путь через NavMesh
            if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            {
                // Если путь найден, разделяем его на промежутки с учетом ландшафта
                return SubdividePath(path.corners, segmentLength);
            }
            
            return null;
        }

        /// <summary>
        /// Разделяет путь на промежутки с учетом ландшафта
        /// Каждая точка пути проецируется на NavMesh для учета высоты и рельефа
        /// </summary>
        private static List<Vector3> SubdividePath(Vector3[] corners, float segmentLength)
        {
            if (corners == null || corners.Length < 2)
            {
                return new List<Vector3>(corners);
            }

            List<Vector3> subdividedPath = new List<Vector3>();
            
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Vector3 start = corners[i];
                Vector3 end = corners[i + 1];
                
                // Добавляем начальную точку (проецируем на NavMesh)
                Vector3 projectedStart = ProjectToNavMesh(start);
                if (subdividedPath.Count == 0 || Vector3.Distance(subdividedPath[subdividedPath.Count - 1], projectedStart) > 0.1f)
                {
                    subdividedPath.Add(projectedStart);
                }
                
                // Вычисляем расстояние между точками
                float distance = Vector3.Distance(start, end);
                
                // Если расстояние больше segmentLength, разделяем на сегменты
                if (distance > segmentLength)
                {
                    int segments = Mathf.CeilToInt(distance / segmentLength);
                    for (int j = 1; j < segments; j++)
                    {
                        float t = (float)j / segments;
                        Vector3 intermediatePoint = Vector3.Lerp(start, end, t);
                        
                        // Проецируем промежуточную точку на NavMesh для учета ландшафта
                        Vector3 projectedPoint = ProjectToNavMesh(intermediatePoint);
                        subdividedPath.Add(projectedPoint);
                    }
                }
                
                // Добавляем конечную точку (проецируем на NavMesh)
                Vector3 projectedEnd = ProjectToNavMesh(end);
                subdividedPath.Add(projectedEnd);
            }
            
            return subdividedPath;
        }

        /// <summary>
        /// Проецирует точку на NavMesh для учета высоты и рельефа ландшафта
        /// </summary>
        private static Vector3 ProjectToNavMesh(Vector3 position)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            
            // Если не удалось найти точку на NavMesh, возвращаем исходную позицию
            return position;
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

