using System.Collections.Generic;
using UnityEngine;

namespace Core.Interfaces
{
    public interface INavigationService
    {
        List<Vector3> CalculatePath(Vector3 from, Vector3 to);
        
        bool IsReachable(Vector3 from, Vector3 to);
        
        void AddTemporaryObstacle(GameObject obstacle);
        
        void RemoveTemporaryObstacle(GameObject obstacle);
        
        bool GetNearestNavMeshPoint(Vector3 position, out Vector3 hitPosition);
    }
}

