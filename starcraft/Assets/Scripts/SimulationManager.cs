using System.Collections.Generic;
using Core.DI;
using Core.Events;
using Core.Interfaces;
using Core.Services;
using Entities.Base;
using Systems.Spawning;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Инициализирует системы симуляции
/// </summary>
public class SimulationManager : MonoBehaviour
{
    [FormerlySerializedAs("_bases")]
    [Header("References")]
    [SerializeField] private List<Base> bases = new();
    [SerializeField] private DroneSpawner droneSpawner;
    [SerializeField] private UpdateManager updateManager;
    [SerializeField] private InitializationManager initializationManager;
    
    private List<ResourceSpawner> _resourceSpawners = new();

    [Header("Settings")]
    [SerializeField] private int initialDronesPerFaction = 3;
    [SerializeField] private float initialDroneSpeed = 5f;
    [SerializeField] private float initialResourceSpawnRate = 5f;

    private ServiceContainer _serviceContainer;
    private INavigationService _navigationService;
    private IResourceService _resourceService;
    private IDroneService _droneService;
    private SimulationService _simulationService;
    private EventBus _eventBus;
    private bool _isInitialized = false;

    private static SimulationManager _currentInstance;
    
    public static SimulationManager Instance => _currentInstance;
    
    public ServiceContainer ServiceContainer => _serviceContainer;
    public INavigationService NavigationService => _navigationService;
    public IResourceService ResourceService => _resourceService;
    public IDroneService DroneService => _droneService;
    public SimulationService SimulationService => _simulationService;
    public EventBus EventBus => _eventBus;
    public UpdateManager UpdateManager => updateManager;
    public InitializationManager InitializationManager => initializationManager;

    private void Awake()
    {
        // Точка доступа к экземпляру
        if (_currentInstance == null)
        {
            _currentInstance = this;
        }
        else if (_currentInstance != this)
        {
            Debug.LogWarning("[SimulationManager] Multiple instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        Initialize();
    }
    
    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        
        _serviceContainer = new ServiceContainer();
        _eventBus = new EventBus();
        _serviceContainer.Register<EventBus>(_eventBus);
        
        EnsureManagersExist();
        InitializeServices();
        RegisterServices();
        InitializeSystems();
        RegisterInitializables();
        if (initializationManager != null)
        {
            initializationManager.InitializeAll();
        }
        
        FindAndInitializeResourceSpawners();
        StartSimulation();
        
        _isInitialized = true;
        Debug.Log("[SimulationManager] Initialization complete");
    }
    
    /// <summary>
    /// Создает менеджеры при необходимости
    /// </summary>
    private void EnsureManagersExist()
    {
        if (updateManager == null)
        {
            GameObject updateManagerGO = new GameObject("UpdateManager");
            updateManagerGO.transform.SetParent(transform);
            updateManager = updateManagerGO.AddComponent<UpdateManager>();
        }
        
        if (initializationManager == null)
        {
            GameObject initManagerGO = new GameObject("InitializationManager");
            initManagerGO.transform.SetParent(transform);
            initializationManager = initManagerGO.AddComponent<InitializationManager>();
        }
        
        _serviceContainer.Register<UpdateManager>(updateManager);
        _serviceContainer.Register<InitializationManager>(initializationManager);
    }
    
    /// <summary>
    /// Регистрирует сервисы в DI
    /// </summary>
    private void RegisterServices()
    {
        _serviceContainer.Register<INavigationService>(_navigationService);
        _serviceContainer.Register<IResourceService>(_resourceService);
        _serviceContainer.Register<IDroneService>(_droneService);
        _serviceContainer.Register<SimulationService>(_simulationService);
        _serviceContainer.Register<SimulationManager>(this);
    }
    
    /// <summary>
    /// Регистрирует IInitializable объекты
    /// </summary>
    private void RegisterInitializables()
    {
        if (initializationManager == null) return;
        
#if UNITY_2023_1_OR_NEWER
        MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
#else
        MonoBehaviour[] allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
#endif
        
        foreach (var mb in allMonoBehaviours)
        {
            if (mb is IInitializable initializable && mb != this)
            {
                initializationManager.RegisterInitializable(initializable);
            }
        }
    }
    
    /// <summary>
    /// Запускает симуляцию
    /// </summary>
    private void StartSimulation()
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
        if (_currentInstance == this)
        {
            _currentInstance = null;
        }
        
        if (_eventBus != null)
        {
            _eventBus.Clear();
        }
        
        if (_serviceContainer != null)
        {
            _serviceContainer.Clear();
        }
    }
}