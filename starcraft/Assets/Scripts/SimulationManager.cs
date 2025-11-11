using System.Collections.Generic;
using Core.Events;
using Core.Interfaces;
using Core.Services;
using Entities.Base;
using Systems.Spawning;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Главный менеджер симуляции
/// Инициализирует все системы, настраивает DI, управляет жизненным циклом симуляции
/// </summary>
public class SimulationManager : MonoBehaviour
{
    [FormerlySerializedAs("_bases")]
    [Header("References")]
    [SerializeField] private List<Base> bases = new();
    [SerializeField] private DroneSpawner droneSpawner;
    
    // Список спаунеров ресурсов (заполняется автоматически при старте)
    private List<ResourceSpawner> _resourceSpawners = new();

    [Header("Settings")]
    [SerializeField] private int initialDronesPerFaction = 3;
    [SerializeField] private float initialDroneSpeed = 5f;
    [SerializeField] private float initialResourceSpawnRate = 5f;

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
        // Автоматический поиск всех спаунеров ресурсов в сцене
        FindAndInitializeResourceSpawners();

        if (droneSpawner)
        {
            droneSpawner.Initialize(bases, _droneService, _navigationService, _resourceService, _simulationService);
            droneSpawner.DronesPerFaction = initialDronesPerFaction;
        }
    }

    /// <summary>
    /// Находит все спаунеры ресурсов в сцене и инициализирует их
    /// </summary>
    private void FindAndInitializeResourceSpawners()
    {
        // Используем FindObjectsOfType для поиска всех ResourceSpawner в сцене
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
        else
        {
            Debug.Log($"Found and initialized {_resourceSpawners.Count} ResourceSpawner(s) in the scene.");
        }
    }

    /// <summary>
    /// Установить количество дронов на фракцию
    /// </summary>
    public void SetDronesPerFaction(int count)
    {
        initialDronesPerFaction = count;
        if (droneSpawner)
        {
            droneSpawner.DronesPerFaction = count;
            droneSpawner.RespawnDrones();
        }
    }

    /// <summary>
    /// Установить скорость дронов
    /// </summary>
    public void SetDroneSpeed(float speed)
    {
        initialDroneSpeed = speed;
        var allDrones = _droneService.GetAllDrones();
        foreach (var drone in allDrones)
        {
            drone.Speed = speed;
        }
    }

    /// <summary>
    /// Установить частоту спавна ресурсов для всех спаунеров
    /// </summary>
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