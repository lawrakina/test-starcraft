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

        public List<IDrone> GetDronesByFaction(FactionType faction)
        {
            return _drones.Values.Where(d => d.Faction == faction).ToList();
        }

        public List<IDrone> GetAllDrones()
        {
            return new List<IDrone>(_drones.Values);
        }

        public List<IDrone> FindNearbyDrones(Vector3 position, float radius, int excludeDroneId)
        {
            List<IDrone> nearbyDrones = new List<IDrone>();

            foreach (var drone in _drones.Values)
            {
                if (drone.Id == excludeDroneId)
                {
                    continue;
                }

                float distance = Vector3.Distance(position, drone.Position);
                if (distance <= radius)
                {
                    nearbyDrones.Add(drone);
                }
            }

            return nearbyDrones;
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

