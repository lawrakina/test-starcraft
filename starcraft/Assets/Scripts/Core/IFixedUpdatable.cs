/// <summary>
/// Интерфейс для объектов, требующих обновления в FixedUpdate
/// </summary>
public interface IFixedUpdatable
{
    /// <summary>
    /// Вызывается каждый фиксированный кадр через UpdateManager
    /// </summary>
    void OnFixedUpdate(float fixedDeltaTime);
    
    /// <summary>
    /// Приоритет выполнения (меньше = выше приоритет)
    /// </summary>
    int FixedUpdatePriority { get; }
}

