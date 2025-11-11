/// <summary>
/// Интерфейс для объектов, требующих обновления каждый кадр
/// </summary>
public interface IUpdatable
{
    /// <summary>
    /// Вызывается каждый кадр через UpdateManager
    /// </summary>
    void OnUpdate(float deltaTime);
    
    /// <summary>
    /// Приоритет выполнения (меньше = выше приоритет)
    /// </summary>
    int UpdatePriority { get; }
}

