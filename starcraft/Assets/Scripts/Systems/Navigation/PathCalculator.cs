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
        /// Рассчитывает путь от начальной позиции до целевой
        /// </summary>
        public static List<Vector3> CalculatePath(Vector3 from, Vector3 to, float segmentLength = 2.0f)
        {
            NavMeshPath path = new NavMeshPath();
            
            if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            {
                return SubdividePath(path.corners, segmentLength);
            }
            
            return null;
        }

        /// <summary>
        /// Разделяет путь на промежутки
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
                
                Vector3 projectedStart = ProjectToNavMesh(start);
                if (subdividedPath.Count == 0 || Vector3.Distance(subdividedPath[subdividedPath.Count - 1], projectedStart) > 0.1f)
                {
                    subdividedPath.Add(projectedStart);
                }
                
                float distance = Vector3.Distance(start, end);
                
                if (distance > segmentLength)
                {
                    int segments = Mathf.CeilToInt(distance / segmentLength);
                    for (int j = 1; j < segments; j++)
                    {
                        float t = (float)j / segments;
                        Vector3 intermediatePoint = Vector3.Lerp(start, end, t);
                        Vector3 projectedPoint = ProjectToNavMesh(intermediatePoint);
                        subdividedPath.Add(projectedPoint);
                    }
                }
                
                Vector3 projectedEnd = ProjectToNavMesh(end);
                subdividedPath.Add(projectedEnd);
            }
            
            return subdividedPath;
        }

        /// <summary>
        /// Проецирует точку на NavMesh
        /// </summary>
        private static Vector3 ProjectToNavMesh(Vector3 position)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            
            return position;
        }

        /// <summary>
        /// Проверяет, достижима ли целевая позиция
        /// </summary>
        public static bool IsReachable(Vector3 from, Vector3 to)
        {
            NavMeshPath path = new NavMeshPath();
            return NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);
        }

        /// <summary>
        /// Получает ближайшую точку на NavMesh
        /// </summary>
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

