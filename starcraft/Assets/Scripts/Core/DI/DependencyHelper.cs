using UnityEngine;

namespace Core.DI
{
    /// <summary>
    /// Вспомогательный класс для получения зависимостей
    /// </summary>
    public static class DependencyHelper
    {
        /// <summary>
        /// Получает UpdateManager через SimulationManager
        /// </summary>
        public static UpdateManager GetUpdateManager()
        {
            var simulationManager = SimulationManager.Instance;
            return simulationManager?.UpdateManager;
        }
        
        /// <summary>
        /// Получает EventBus через SimulationManager
        /// </summary>
        public static Core.Events.EventBus GetEventBus()
        {
            var simulationManager = SimulationManager.Instance;
            return simulationManager?.EventBus;
        }
        
        /// <summary>
        /// Получает InitializationManager через SimulationManager
        /// </summary>
        public static InitializationManager GetInitializationManager()
        {
            var simulationManager = SimulationManager.Instance;
            return simulationManager?.InitializationManager;
        }
    }
}

