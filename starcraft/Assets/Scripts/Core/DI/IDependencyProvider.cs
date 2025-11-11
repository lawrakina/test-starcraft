namespace Core.DI
{
    /// <summary>
    /// Интерфейс для объектов, которые могут предоставлять зависимости
    /// </summary>
    public interface IDependencyProvider
    {
        ServiceContainer GetServiceContainer();
    }
}

