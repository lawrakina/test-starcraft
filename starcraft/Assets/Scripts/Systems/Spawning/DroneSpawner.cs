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
    public class DroneSpawner : MonoBehaviour, IInitializable
    {
        [SerializeField] private GameObject dronePrefab;
        [SerializeField] private int dronesPerFaction = 3;
        [SerializeField] private DronePool dronePool; // Опциональный пул дронов
        
        private List<Base> _bases = new();
        private IDroneService _droneService;
        private INavigationService _navigationService;
        private IResourceService _resourceService;
        private SimulationService _simulationService;
        private bool _isInitialized = false;
        private bool _usePooling = false;

        public int DronesPerFaction
        {
            get => dronesPerFaction;
            set => dronesPerFaction = Mathf.Clamp(value, 1, 10);
        }
        
        public int InitializationPhase => 1; // Вторая фаза - игровые системы
        public System.Type[] Dependencies => new[] { typeof(SimulationManager), typeof(Entities.Base.Base) };

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }
            
            // Получаем SimulationManager
            var simulationManager = SimulationManager.Instance;
            if (simulationManager == null)
            {
                Debug.LogError("[DroneSpawner] SimulationManager not found!");
                return;
            }
            
            // Получаем базы из SimulationManager или находим их на сцене
            if (_bases == null || _bases.Count == 0)
            {
#if UNITY_2023_1_OR_NEWER
                var foundBases = FindObjectsByType<Base>(FindObjectsSortMode.None);
#else
                var foundBases = FindObjectsOfType<Base>();
#endif
                _bases = new List<Base>(foundBases);
            }
            
            _droneService = simulationManager.DroneService;
            _navigationService = simulationManager.NavigationService;
            _resourceService = simulationManager.ResourceService;
            _simulationService = simulationManager.SimulationService;
            
            // Инициализируем пул, если он назначен
            if (dronePool != null)
            {
                dronePool.Initialize(_droneService, _navigationService, _resourceService);
                _usePooling = true;
            }
            
            _isInitialized = true;
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
            _isInitialized = true;
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
                Drone drone = null;
                
                if (_usePooling && dronePool != null)
                {
                    // Используем пул
                    drone = dronePool.GetDrone(baseObj);
                    if (drone != null && drone is MonoBehaviour droneMono)
                    {
                        droneMono.transform.position = spawnPosition;
                    }
                }
                else
                {
                    // Используем обычное создание
                var droneObject = Instantiate(dronePrefab, spawnPosition, Quaternion.identity);
                    drone = droneObject.GetComponent<Drone>();
                }
                
                if (drone)
                {
                    drone.SetHomeBase(baseObj);
                    
                    if (!_usePooling || dronePool == null)
                    {
                    drone.Initialize(_navigationService, _resourceService, _droneService);
                    }
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
                    if (_usePooling && dronePool != null && drone is Drone droneComponent)
                    {
                        // Возвращаем в пул
                        dronePool.ReturnDrone(droneComponent);
                    }
                    else
                    {
                        // Обычное уничтожение
                    Destroy(droneMono.gameObject);
                    }
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

