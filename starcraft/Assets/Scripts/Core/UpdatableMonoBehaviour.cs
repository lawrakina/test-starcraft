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
        UpdateManager.Instance.RegisterUpdatable(this);
    }
    
    protected virtual void OnDisable()
    {
        UpdateManager.Instance.UnregisterUpdatable(this);
    }
    
    protected virtual void OnDestroy()
    {
        UpdateManager.Instance.UnregisterUpdatable(this);
    }
    
    /// <summary>
    /// Переопределите этот метод вместо Update()
    /// </summary>
    public abstract void OnUpdate(float deltaTime);
}

