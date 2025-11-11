using UnityEngine;

/// <summary>
/// Базовый класс для MonoBehaviour, который автоматически регистрируется в UpdateManager
/// </summary>
public abstract class UpdatableMonoBehaviour : MonoBehaviour, IUpdatable
{
    [SerializeField] private int updatePriority = 0;
    
    public int UpdatePriority => updatePriority;
    
    protected virtual void OnEnable()
    {
        var updateManager = Core.DI.DependencyHelper.GetUpdateManager();
        if (updateManager != null)
        {
            updateManager.RegisterUpdatable(this);
        }
    }
    
    protected virtual void OnDisable()
    {
        var updateManager = Core.DI.DependencyHelper.GetUpdateManager();
        if (updateManager != null)
        {
            updateManager.UnregisterUpdatable(this);
        }
    }
    
    protected virtual void OnDestroy()
    {
        var updateManager = Core.DI.DependencyHelper.GetUpdateManager();
        if (updateManager != null)
        {
            updateManager.UnregisterUpdatable(this);
        }
    }
    
    /// <summary>
    /// Переопределите этот метод вместо Update()
    /// </summary>
    public abstract void OnUpdate(float deltaTime);
}

