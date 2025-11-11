using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Централизованный менеджер для управления Update и FixedUpdate
/// Контролирует жизненный цикл объектов и вызывает их методы обновления
/// Должен быть размещен на сцене вручную
/// </summary>
public class UpdateManager : MonoBehaviour
{
    // Списки для хранения обновляемых объектов
    private readonly List<IUpdatable> _updatables = new List<IUpdatable>();
    private readonly List<IFixedUpdatable> _fixedUpdatables = new List<IFixedUpdatable>();
    
    // Временные списки для безопасной итерации (на случай изменения во время обновления)
    private readonly List<IUpdatable> _updatablesToProcess = new List<IUpdatable>();
    private readonly List<IFixedUpdatable> _fixedUpdatablesToProcess = new List<IFixedUpdatable>();
    
    // Флаги для контроля обновлений
    private bool _isPaused = false;
    private bool _needsSort = false;
    
    /// <summary>
    /// Регистрирует объект для обновления каждый кадр
    /// </summary>
    public void RegisterUpdatable(IUpdatable updatable)
    {
        if (updatable == null)
        {
            Debug.LogWarning("[UpdateManager] Attempted to register null IUpdatable");
            return;
        }
        
        if (!_updatables.Contains(updatable))
        {
            _updatables.Add(updatable);
            _needsSort = true;
        }
    }
    
    /// <summary>
    /// Отменяет регистрацию объекта для обновления
    /// </summary>
    public void UnregisterUpdatable(IUpdatable updatable)
    {
        if (updatable != null)
        {
            _updatables.Remove(updatable);
        }
    }
    
    /// <summary>
    /// Регистрирует объект для обновления в FixedUpdate
    /// </summary>
    public void RegisterFixedUpdatable(IFixedUpdatable fixedUpdatable)
    {
        if (fixedUpdatable == null)
        {
            Debug.LogWarning("[UpdateManager] Attempted to register null IFixedUpdatable");
            return;
        }
        
        if (!_fixedUpdatables.Contains(fixedUpdatable))
        {
            _fixedUpdatables.Add(fixedUpdatable);
            _needsSort = true;
        }
    }
    
    /// <summary>
    /// Отменяет регистрацию объекта для FixedUpdate
    /// </summary>
    public void UnregisterFixedUpdatable(IFixedUpdatable fixedUpdatable)
    {
        if (fixedUpdatable != null)
        {
            _fixedUpdatables.Remove(fixedUpdatable);
        }
    }
    
    /// <summary>
    /// Приостанавливает все обновления
    /// </summary>
    public void Pause()
    {
        _isPaused = true;
    }
    
    /// <summary>
    /// Возобновляет все обновления
    /// </summary>
    public void Resume()
    {
        _isPaused = false;
    }
    
    private void Update()
    {
        if (_isPaused || _updatables.Count == 0)
        {
            return;
        }
        
        // Сортируем по приоритету при необходимости
        if (_needsSort)
        {
            _updatables.Sort((a, b) => a.UpdatePriority.CompareTo(b.UpdatePriority));
            _fixedUpdatables.Sort((a, b) => a.FixedUpdatePriority.CompareTo(b.FixedUpdatePriority));
            _needsSort = false;
        }
        
        // Копируем список для безопасной итерации
        _updatablesToProcess.Clear();
        _updatablesToProcess.AddRange(_updatables);
        
        // Вызываем OnUpdate для всех зарегистрированных объектов
        float deltaTime = Time.deltaTime;
        foreach (var updatable in _updatablesToProcess)
        {
            if (updatable != null)
            {
                try
                {
                    updatable.OnUpdate(deltaTime);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[UpdateManager] Error in OnUpdate: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
        
        // Удаляем null ссылки (на случай уничтожения объектов)
        _updatables.RemoveAll(x => x == null);
    }
    
    private void FixedUpdate()
    {
        if (_isPaused || _fixedUpdatables.Count == 0)
        {
            return;
        }
        
        // Копируем список для безопасной итерации
        _fixedUpdatablesToProcess.Clear();
        _fixedUpdatablesToProcess.AddRange(_fixedUpdatables);
        
        // Вызываем OnFixedUpdate для всех зарегистрированных объектов
        float fixedDeltaTime = Time.fixedDeltaTime;
        foreach (var fixedUpdatable in _fixedUpdatablesToProcess)
        {
            if (fixedUpdatable != null)
            {
                try
                {
                    fixedUpdatable.OnFixedUpdate(fixedDeltaTime);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[UpdateManager] Error in OnFixedUpdate: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
        
        // Удаляем null ссылки
        _fixedUpdatables.RemoveAll(x => x == null);
    }
    
    /// <summary>
    /// Очищает все регистрации (полезно при смене сцены)
    /// </summary>
    public void Clear()
    {
        _updatables.Clear();
        _fixedUpdatables.Clear();
        _updatablesToProcess.Clear();
        _fixedUpdatablesToProcess.Clear();
    }
    
}

