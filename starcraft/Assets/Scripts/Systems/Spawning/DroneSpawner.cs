using System.Collections.Generic;
using Core.Interfaces;
using Core.Services;
using Entities.Base;
using Entities.Drone;
using UnityEngine;

namespace Systems.Spawning
{
    /// <summary>
    /// Спавнер дронов
    /// Создает дронов для каждой фракции на их базах
    /// </summary>
    public class DroneSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject dronePrefab;
        [SerializeField] private int dronesPerFaction = 3;
        
        private List<Base> _bases = new();
        private IDroneService _droneService;
        private INavigationService _navigationService;
        private IResourceService _resourceService;
        private SimulationService _simulationService;

        public int DronesPerFaction
        {
            get => dronesPerFaction;
            set => dronesPerFaction = Mathf.Clamp(value, 1, 10);
        }

        public void Initialize(
            List<Base> bases,
            IDroneService droneService,
            INavigationService navigationService,
            IResourceService resourceService,
            SimulationService simulationService)
        {
            _bases = bases;
            _droneService = droneService;
            _navigationService = navigationService;
            _resourceService = resourceService;
            _simulationService = simulationService;
        }

        public void SpawnAllDrones()
        {
            if (!dronePrefab)
            {
                Debug.LogError("Drone prefab is not assigned!");
                return;
            }

            foreach (Base baseObj in _bases)
            {
                SpawnDronesForBase(baseObj);
            }
        }

        private void SpawnDronesForBase(Base baseObj)
        {
            for (var i = 0; i < dronesPerFaction; i++)
            {
                var spawnPosition = baseObj.GetSpawnPoint(i);
                var droneObject = Instantiate(dronePrefab, spawnPosition, Quaternion.identity);
                
                var drone = droneObject.GetComponent<Drone>();
                if (drone)
                {
                    drone.SetHomeBase(baseObj);
                    
                    drone.Initialize(_navigationService, _resourceService, _droneService);
                }
            }
        }

        public void ClearAllDrones()
        {
            var allDrones = _droneService.GetAllDrones();
            foreach (var drone in allDrones)
            {
                if (drone is MonoBehaviour droneMono)
                {
                    Destroy(droneMono.gameObject);
                }
            }
        }

        public void RespawnDrones()
        {
            ClearAllDrones();
            SpawnAllDrones();
        }
    }
}

