using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Менеджер для управления Update и FixedUpdate
/// </summary>
public class UpdateManager : MonoBehaviour
{
    private readonly List<IUpdatable> _updatables = new List<IUpdatable>();
    private readonly List<IFixedUpdatable> _fixedUpdatables = new List<IFixedUpdatable>();
    private readonly List<IUpdatable> _updatablesToProcess = new List<IUpdatable>();
    private readonly List<IFixedUpdatable> _fixedUpdatablesToProcess = new List<IFixedUpdatable>();
    private bool _isPaused = false;
    private bool _needsSort = false;
    
    /// <summary>
    /// Регистрирует объект для обновления
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
    /// Отменяет регистрацию объекта
    /// </summary>
    public void UnregisterUpdatable(IUpdatable updatable)
    {
        if (updatable != null)
        {
            _updatables.Remove(updatable);
        }
    }
    
    /// <summary>
    /// Регистрирует объект для FixedUpdate
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
        
        if (_needsSort)
        {
            _updatables.Sort((a, b) => a.UpdatePriority.CompareTo(b.UpdatePriority));
            _fixedUpdatables.Sort((a, b) => a.FixedUpdatePriority.CompareTo(b.FixedUpdatePriority));
            _needsSort = false;
        }
        
        _updatablesToProcess.Clear();
        _updatablesToProcess.AddRange(_updatables);
        
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
        
        _updatables.RemoveAll(x => x == null);
    }
    
    private void FixedUpdate()
    {
        if (_isPaused || _fixedUpdatables.Count == 0)
        {
            return;
        }
        
        _fixedUpdatablesToProcess.Clear();
        _fixedUpdatablesToProcess.AddRange(_fixedUpdatables);
        
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
        
        _fixedUpdatables.RemoveAll(x => x == null);
    }
    
    /// <summary>
    /// Очищает все регистрации
    /// </summary>
    public void Clear()
    {
        _updatables.Clear();
        _fixedUpdatables.Clear();
        _updatablesToProcess.Clear();
        _fixedUpdatablesToProcess.Clear();
    }
    
}

