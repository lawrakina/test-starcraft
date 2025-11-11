using System;

/// <summary>
/// Интерфейс для объектов, требующих инициализации при старте сцены
/// </summary>
public interface IInitializable
{
    /// <summary>
    /// Вызывается при инициализации
    /// </summary>
    void Initialize();
    
    /// <summary>
    /// Фаза инициализации (меньше = раньше)
    /// </summary>
    int InitializationPhase { get; }
    
    /// <summary>
    /// Зависимости от других инициализируемых объектов
    /// </summary>
    Type[] Dependencies { get; }
}

