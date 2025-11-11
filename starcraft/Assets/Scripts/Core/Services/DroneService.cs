using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Core.Services
{
    public class DroneService : IDroneService
    {
        private readonly Dictionary<int, IDrone> _drones = new();
        private readonly List<IDrone> _tempDroneList = new List<IDrone>(); // Переиспользуемый список

        public List<IDrone> GetDronesByFaction(FactionType faction)
        {
            _tempDroneList.Clear();
            foreach (var drone in _drones.Values)
            {
                if (drone.Faction == faction)
                {
                    _tempDroneList.Add(drone);
                }
            }
            return new List<IDrone>(_tempDroneList);
        }

        public List<IDrone> GetAllDrones()
        {
            return new List<IDrone>(_drones.Values);
        }

        public List<IDrone> FindNearbyDrones(Vector3 position, float radius, int excludeDroneId)
        {
            _tempDroneList.Clear();

            foreach (var drone in _drones.Values)
            {
                if (drone.Id == excludeDroneId)
                {
                    continue;
                }

                float distance = Vector3.Distance(position, drone.Position);
                if (distance <= radius)
                {
                    _tempDroneList.Add(drone);
                }
            }

            return new List<IDrone>(_tempDroneList);
        }

        public void RegisterDrone(IDrone drone)
        {
            if (drone != null)
            {
                _drones.TryAdd(drone.Id, drone);
            }
        }

        public void UnregisterDrone(IDrone drone)
        {
            if (drone != null)
            {
                _drones.Remove(drone.Id);
            }
        }

        public IDrone GetDroneById(int droneId)
        {
            _drones.TryGetValue(droneId, out IDrone drone);
            return drone;
        }
    }
}

