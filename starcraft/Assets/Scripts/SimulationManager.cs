using System.Collections.Generic;
using Core.Events;
using Core.Interfaces;
using Core.Services;
using Entities.Base;
using Systems.Spawning;
using UnityEngine;
using UnityEngine.Serialization;

public class SimulationManager : MonoBehaviour, IInitializable
{
    [FormerlySerializedAs("_bases")]
    [Header("References")]
    [SerializeField] private List<Base> bases = new();
    [SerializeField] private DroneSpawner droneSpawner;
    
    private List<ResourceSpawner> _resourceSpawners = new();

    [Header("Settings")]
    [SerializeField] private int initialDronesPerFaction = 3;
    [SerializeField] private float initialDroneSpeed = 5f;
    [SerializeField] private float initialResourceSpawnRate = 5f;

    private INavigationService _navigationService;
    private IResourceService _resourceService;
    private IDroneService _droneService;
    private SimulationService _simulationService;
    private bool _isInitialized = false;

    public INavigationService NavigationService => _navigationService;
    public IResourceService ResourceService => _resourceService;
    public IDroneService DroneService => _droneService;
    public SimulationService SimulationService => _simulationService;
    
    public int InitializationPhase => 0; // Первая фаза - системные менеджеры
    public System.Type[] Dependencies => null; // Нет зависимостей

    private void Awake()
    {
        InitializationManager.Instance.RegisterInitializable(this);
    }
    
    public void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        
        InitializeServices();
        InitializeSystems();
        
        // Спавн дронов будет вызван после инициализации всех объектов через InitializationManager
        // или вручную после завершения инициализации
        
        _isInitialized = true;
    }
    
    /// <summary>
    /// Вызывается после завершения инициализации всех объектов
    /// </summary>
    public void StartSimulation()
    {
        if (droneSpawner)
        {
            droneSpawner.SpawnAllDrones();
        }
    }

    private void InitializeServices()
    {
        _navigationService = new NavigationService();
        _resourceService = new ResourceService();
        _droneService = new DroneService();
        _simulationService = new SimulationService(_navigationService, _resourceService, _droneService);
    }

    private void InitializeSystems()
    {
        FindAndInitializeResourceSpawners();

        // DroneSpawner будет инициализирован через InitializationManager
        // Но мы можем установить параметры, если они нужны до инициализации
        if (droneSpawner)
        {
            droneSpawner.DronesPerFaction = initialDronesPerFaction;
        }
    }

    private void FindAndInitializeResourceSpawners()
    {
#if UNITY_2023_1_OR_NEWER
        var foundSpawners = FindObjectsByType<ResourceSpawner>(FindObjectsSortMode.None);
#else
        var foundSpawners = FindObjectsOfType<ResourceSpawner>();
#endif

        _resourceSpawners.Clear();
        
        foreach (var spawner in foundSpawners)
        {
            if (spawner != null)
            {
                spawner.Initialize(_resourceService, _navigationService);
                spawner.SpawnInterval = initialResourceSpawnRate;
                _resourceSpawners.Add(spawner);
            }
        }

        if (_resourceSpawners.Count == 0)
        {
            Debug.LogWarning("No ResourceSpawner components found in the scene!");
        }
    }

    public void SetDronesPerFaction(int count)
    {
        initialDronesPerFaction = count;
        if (droneSpawner)
        {
            droneSpawner.DronesPerFaction = count;
            droneSpawner.RespawnDrones();
        }
    }

    public void SetDroneSpeed(float speed)
    {
        initialDroneSpeed = speed;
        var allDrones = _droneService.GetAllDrones();
        foreach (var drone in allDrones)
        {
            drone.Speed = speed;
        }
    }

    public void SetResourceSpawnRate(float interval)
    {
        initialResourceSpawnRate = interval;
        foreach (var spawner in _resourceSpawners)
        {
            if (spawner != null)
            {
                spawner.SpawnInterval = interval;
            }
        }
    }

    private void OnDestroy()
    {
        EventBus.Instance.Clear();
    }
}