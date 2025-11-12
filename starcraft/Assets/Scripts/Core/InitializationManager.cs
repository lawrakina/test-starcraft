using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Менеджер инициализации объектов на сцене
/// </summary>
public class InitializationManager : MonoBehaviour
{
    private readonly List<IInitializable> _initializables = new List<IInitializable>();
    private readonly Dictionary<Type, IInitializable> _initializablesByType = new Dictionary<Type, IInitializable>();
    private bool _isInitialized = false;
    
    /// <summary>
    /// Регистрирует объект
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
            
            Type type = initializable.GetType();
            if (!_initializablesByType.ContainsKey(type))
            {
                _initializablesByType[type] = initializable;
            }
        }
    }
    
    /// <summary>
    /// Получает объект по типу
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
    /// Находит все объекты на сцене
    /// </summary>
    private void FindAllInitializables()
    {
        _initializables.Clear();
        _initializablesByType.Clear();
        
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
    /// Выполняет инициализацию всех объектов
    /// </summary>
    public void InitializeAll()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("[InitializationManager] Already initialized!");
            return;
        }
        
        var sorted = _initializables.OrderBy(x => x.InitializationPhase).ToList();
        HashSet<IInitializable> initialized = new HashSet<IInitializable>();
        
        foreach (var initializable in sorted)
        {
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
    }
    
    /// <summary>
    /// Переинициализирует все объекты
    /// </summary>
    public void Reinitialize()
    {
        _isInitialized = false;
        FindAllInitializables();
        InitializeAll();
    }
}

