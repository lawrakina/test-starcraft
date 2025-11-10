using System.Collections.Generic;
using Core.Events;
using Core.Interfaces;
using Core.Services;
using UnityEngine;
using Entities.Base;
using Systems.Spawning;

namespace DroneResourceCollection
{
    /// <summary>
    /// Главный менеджер симуляции
    /// Инициализирует все системы, настраивает DI, управляет жизненным циклом симуляции
    /// </summary>
    public class SimulationManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private List<Base> _bases = new List<Base>();
        [SerializeField] private ResourceSpawner _resourceSpawner;
        [SerializeField] private DroneSpawner _droneSpawner;

        [Header("Settings")]
        [SerializeField] private int _initialDronesPerFaction = 3;
        [SerializeField] private float _initialDroneSpeed = 5f;
        [SerializeField] private float _initialResourceSpawnRate = 5f;

        // Сервисы (DI контейнер)
        private INavigationService _navigationService;
        private IResourceService _resourceService;
        private IDroneService _droneService;
        private SimulationService _simulationService;

        // Публичные свойства для доступа к сервисам
        public INavigationService NavigationService => _navigationService;
        public IResourceService ResourceService => _resourceService;
        public IDroneService DroneService => _droneService;
        public SimulationService SimulationService => _simulationService;

        private void Awake()
        {
            InitializeServices();
            InitializeSystems();
        }

        private void Start()
        {
            // Спавним дронов после инициализации всех систем
            if (_droneSpawner != null)
            {
                _droneSpawner.SpawnAllDrones();
            }
        }

        private void InitializeServices()
        {
            // Создаем сервисы (простой DI контейнер)
            _navigationService = new NavigationService(0.5f);
            _resourceService = new ResourceService();
            _droneService = new DroneService();
            _simulationService = new SimulationService(_navigationService, _resourceService, _droneService);
        }

        private void InitializeSystems()
        {
            // Инициализируем спавнер ресурсов
            if (_resourceSpawner != null)
            {
                _resourceSpawner.Initialize(_resourceService, _navigationService);
                _resourceSpawner.SpawnInterval = _initialResourceSpawnRate;
            }

            // Инициализируем спавнер дронов
            if (_droneSpawner != null)
            {
                _droneSpawner.Initialize(_bases, _droneService, _navigationService, _resourceService, _simulationService);
                _droneSpawner.DronesPerFaction = _initialDronesPerFaction;
            }
        }

        /// <summary>
        /// Установить количество дронов на фракцию
        /// </summary>
        public void SetDronesPerFaction(int count)
        {
            _initialDronesPerFaction = count;
            if (_droneSpawner != null)
            {
                _droneSpawner.DronesPerFaction = count;
                _droneSpawner.RespawnDrones();
            }
        }

        /// <summary>
        /// Установить скорость дронов
        /// </summary>
        public void SetDroneSpeed(float speed)
        {
            _initialDroneSpeed = speed;
            var allDrones = _droneService.GetAllDrones();
            foreach (var drone in allDrones)
            {
                drone.Speed = speed;
            }
        }

        /// <summary>
        /// Установить частоту спавна ресурсов
        /// </summary>
        public void SetResourceSpawnRate(float interval)
        {
            _initialResourceSpawnRate = interval;
            if (_resourceSpawner != null)
            {
                _resourceSpawner.SpawnInterval = interval;
            }
        }

        private void OnDestroy()
        {
            // Очищаем Event Bus при уничтожении
            EventBus.Instance.Clear();
        }
    }
}

