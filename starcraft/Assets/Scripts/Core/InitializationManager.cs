using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Централизованный менеджер инициализации объектов на сцене
/// Обеспечивает правильный порядок инициализации и установку ссылок
/// </summary>
public class InitializationManager : MonoBehaviour
{
    private static InitializationManager _instance;
    
    private readonly List<IInitializable> _initializables = new List<IInitializable>();
    private readonly Dictionary<Type, IInitializable> _initializablesByType = new Dictionary<Type, IInitializable>();
    private bool _isInitialized = false;
    
    public static InitializationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("InitializationManager");
                _instance = go.AddComponent<InitializationManager>();
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Находим все объекты, требующие инициализации
        FindAllInitializables();
        
        // Выполняем инициализацию
        InitializeAll();
    }
    
    private void OnDisable()
    {
        // Очищаем singleton при остановке плей-режима
        if (!Application.isPlaying && _instance == this)
        {
            _initializables.Clear();
            _initializablesByType.Clear();
            _isInitialized = false;
            _instance = null;
        }
    }
    
    private void OnDestroy()
    {
        // Очищаем singleton при уничтожении объекта
        if (_instance == this)
        {
            _initializables.Clear();
            _initializablesByType.Clear();
            _isInitialized = false;
            _instance = null;
        }
    }
    
    /// <summary>
    /// Регистрирует объект для инициализации
    /// </summary>
    public void RegisterInitializable(IInitializable initializable)
    {
        if (initializable == null)
        {
            Debug.LogWarning("[InitializationManager] Attempted to register null IInitializable");
            return;
        }
        
        if (!_initializables.Contains(initializable))
        {
            _initializables.Add(initializable);
            
            // Сохраняем по типу для поиска зависимостей
            Type type = initializable.GetType();
            if (!_initializablesByType.ContainsKey(type))
            {
                _initializablesByType[type] = initializable;
            }
        }
    }
    
    /// <summary>
    /// Получает инициализированный объект по типу
    /// </summary>
    public T GetInitialized<T>() where T : class, IInitializable
    {
        Type type = typeof(T);
        if (_initializablesByType.TryGetValue(type, out var initializable))
        {
            return initializable as T;
        }
        return null;
    }
    
    /// <summary>
    /// Находит все объекты на сцене, реализующие IInitializable
    /// </summary>
    private void FindAllInitializables()
    {
        _initializables.Clear();
        _initializablesByType.Clear();
        
        // Находим все MonoBehaviour на сцене
#if UNITY_2023_1_OR_NEWER
        MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
#else
        MonoBehaviour[] allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
#endif
        
        foreach (var mb in allMonoBehaviours)
        {
            if (mb is IInitializable initializable)
            {
                RegisterInitializable(initializable);
            }
        }
        
        Debug.Log($"[InitializationManager] Found {_initializables.Count} initializable objects");
    }
    
    /// <summary>
    /// Выполняет инициализацию всех объектов в правильном порядке
    /// </summary>
    private void InitializeAll()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("[InitializationManager] Already initialized!");
            return;
        }
        
        // Сортируем по фазе инициализации
        var sorted = _initializables.OrderBy(x => x.InitializationPhase).ToList();
        
        // Инициализируем с учетом зависимостей
        HashSet<IInitializable> initialized = new HashSet<IInitializable>();
        
        foreach (var initializable in sorted)
        {
            // Проверяем зависимости
            if (initializable.Dependencies != null && initializable.Dependencies.Length > 0)
            {
                foreach (var dependencyType in initializable.Dependencies)
                {
                    if (_initializablesByType.TryGetValue(dependencyType, out var dependency))
                    {
                        if (!initialized.Contains(dependency))
                        {
                            Debug.LogWarning($"[InitializationManager] Dependency {dependencyType.Name} not initialized before {initializable.GetType().Name}");
                        }
                    }
                }
            }
            
            // Инициализируем объект
            try
            {
                initializable.Initialize();
                initialized.Add(initializable);
                Debug.Log($"[InitializationManager] Initialized {initializable.GetType().Name} (Phase: {initializable.InitializationPhase})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InitializationManager] Error initializing {initializable.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        _isInitialized = true;
        Debug.Log("[InitializationManager] All objects initialized successfully");
        
        // Запускаем симуляцию после завершения инициализации
        var simulationManager = GetInitialized<SimulationManager>();
        if (simulationManager != null)
        {
            simulationManager.StartSimulation();
        }
    }
    
    /// <summary>
    /// Принудительно переинициализирует все объекты
    /// </summary>
    public void Reinitialize()
    {
        _isInitialized = false;
        FindAllInitializables();
        InitializeAll();
    }
}

