using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Базовый класс для пула объектов
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [SerializeField] protected GameObject prefab;
    [SerializeField] protected int initialSize = 10;
    [SerializeField] protected int maxSize = 50;
    [SerializeField] protected bool expandOnDemand = true;
    
    protected Queue<GameObject> _pool = new Queue<GameObject>();
    protected HashSet<GameObject> _activeObjects = new HashSet<GameObject>();
    
    protected virtual void Awake()
    {
        if (prefab == null)
        {
            Debug.LogError("[ObjectPool] Prefab is not assigned!");
            return;
        }
        
        InitializePool();
    }
    
    /// <summary>
    /// Инициализирует пул
    /// </summary>
    protected virtual void InitializePool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreatePooledObject();
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
    
    /// <summary>
    /// Создает объект для пула
    /// </summary>
    protected virtual GameObject CreatePooledObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.name = $"{prefab.name}_Pooled_{_pool.Count + _activeObjects.Count}";
        return obj;
    }
    
    /// <summary>
    /// Получает объект из пула
    /// </summary>
    public virtual GameObject Get()
    {
        GameObject obj = null;
        
        while (_pool.Count > 0 && obj == null)
        {
            obj = _pool.Dequeue();
            if (obj == null)
            {
                continue;
            }
        }
        
        if (obj == null && expandOnDemand && (_pool.Count + _activeObjects.Count) < maxSize)
        {
            obj = CreatePooledObject();
        }
        
        if (obj == null)
        {
            Debug.LogWarning($"[ObjectPool] Pool exhausted for {prefab.name}. Max size: {maxSize}");
            return null;
        }
        
        obj.SetActive(true);
        _activeObjects.Add(obj);
        
        return obj;
    }
    
    /// <summary>
    /// Возвращает объект в пул
    /// </summary>
    public virtual void Return(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }
        
        if (!_activeObjects.Contains(obj))
        {
            Debug.LogWarning($"[ObjectPool] Attempted to return object {obj.name} that is not in active set");
            return;
        }
        
        _activeObjects.Remove(obj);
        obj.SetActive(false);
        
        if (_pool.Count < maxSize)
        {
            _pool.Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
    
    /// <summary>
    /// Очищает пул
    /// </summary>
    public virtual void Clear()
    {
        while (_pool.Count > 0)
        {
            GameObject obj = _pool.Dequeue();
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        
        foreach (var obj in _activeObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        
        _activeObjects.Clear();
    }
    
    /// <summary>
    /// Количество активных объектов
    /// </summary>
    public int ActiveCount => _activeObjects.Count;
    
    /// <summary>
    /// Количество объектов в пуле
    /// </summary>
    public int PooledCount => _pool.Count;
    
    protected virtual void OnDestroy()
    {
        Clear();
    }
}

