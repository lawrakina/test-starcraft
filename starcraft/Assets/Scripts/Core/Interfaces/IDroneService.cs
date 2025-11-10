using System.Collections.Generic;
using Core.Enums;
using UnityEngine;

namespace Core.Interfaces
{
    public interface IDroneService
    {
        List<IDrone> GetDronesByFaction(FactionType faction);
        
        List<IDrone> GetAllDrones();
        
        List<IDrone> FindNearbyDrones(Vector3 position, float radius, int excludeDroneId);
        
        void RegisterDrone(IDrone drone);
        
        void UnregisterDrone(IDrone drone);
        
        IDrone GetDroneById(int droneId);
    }
}

