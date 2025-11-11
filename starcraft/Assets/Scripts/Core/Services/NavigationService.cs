using System.Collections.Generic;
using Core.Interfaces;
using DroneResourceCollection.Systems.Navigation;
using Systems.Navigation;
using UnityEngine;

namespace Core.Services
{
    public class NavigationService : INavigationService
    {
        private readonly TemporaryObstacleManager _obstacleManager;

        public NavigationService(float obstacleRadius = 0.5f)
        {
            _obstacleManager = new TemporaryObstacleManager(obstacleRadius);
        }

        public List<Vector3> CalculatePath(Vector3 from, Vector3 to, float segmentLength = 2.0f)
        {
            if (!GetNearestNavMeshPoint(from, out var fromNavMesh) || 
                !GetNearestNavMeshPoint(to, out var toNavMesh))
            {
                return null;
            }
            
            return PathCalculator.CalculatePath(fromNavMesh, toNavMesh, segmentLength);
        }

        public bool IsReachable(Vector3 from, Vector3 to)
        {
            if (!GetNearestNavMeshPoint(from, out var fromNavMesh) || 
                !GetNearestNavMeshPoint(to, out var toNavMesh))
            {
                return false;
            }
            
            return PathCalculator.IsReachable(fromNavMesh, toNavMesh);
        }

        public void AddTemporaryObstacle(GameObject obstacle)
        {
            _obstacleManager.AddObstacle(obstacle);
        }

        public void RemoveTemporaryObstacle(GameObject obstacle)
        {
            _obstacleManager.RemoveObstacle(obstacle);
        }

        public bool GetNearestNavMeshPoint(Vector3 position, out Vector3 hitPosition)
        {
            return PathCalculator.GetNearestNavMeshPoint(position, out hitPosition);
        }
    }
}

