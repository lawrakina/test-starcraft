using System.Collections.Generic;
using UnityEngine;

namespace Core.Interfaces
{
    public interface INavigationService
    {
        /// <summary>
        /// Рассчитать путь от начальной позиции до целевой
        /// </summary>
        /// <param name="from">Начальная позиция</param>
        /// <param name="to">Целевая позиция</param>
        /// <param name="segmentLength">Длина сегмента пути для разделения (опционально, по умолчанию 2.0f)</param>
        /// <returns>Список точек пути или null, если путь не найден</returns>
        List<Vector3> CalculatePath(Vector3 from, Vector3 to, float segmentLength = 2.0f);
        
        bool IsReachable(Vector3 from, Vector3 to);
        
        void AddTemporaryObstacle(GameObject obstacle);
        
        void RemoveTemporaryObstacle(GameObject obstacle);
        
        bool GetNearestNavMeshPoint(Vector3 position, out Vector3 hitPosition);
    }
}

