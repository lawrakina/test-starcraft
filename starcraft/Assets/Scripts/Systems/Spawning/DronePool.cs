using Core.Interfaces;
using Entities.Drone;
using UnityEngine;

namespace Systems.Spawning
{
    /// <summary>
    /// Пул для дронов
    /// Управляет переиспользованием дронов для оптимизации производительности
    /// </summary>
    public class DronePool : ObjectPool
    {
        private IDroneService _droneService;
        private INavigationService _navigationService;
        private IResourceService _resourceService;
        
        /// <summary>
        /// Инициализирует пул дронов
        /// </summary>
        public void Initialize(
            IDroneService droneService,
            INavigationService navigationService,
            IResourceService resourceService)
        {
            _droneService = droneService;
            _navigationService = navigationService;
            _resourceService = resourceService;
        }
        
        protected override GameObject CreatePooledObject()
        {
            GameObject obj = base.CreatePooledObject();
            
            // Инициализируем дрон, если он есть
            Drone drone = obj.GetComponent<Drone>();
            if (drone != null && _droneService != null)
            {
                // Дрон будет инициализирован через InitializationManager при активации
                // Здесь мы только создаем объект
            }
            
            return obj;
        }
        
        /// <summary>
        /// Получает дрон из пула и инициализирует его
        /// </summary>
        public Drone GetDrone(Entities.Base.Base homeBase)
        {
            GameObject obj = Get();
            if (obj == null)
            {
                return null;
            }
            
            Drone drone = obj.GetComponent<Drone>();
            if (drone != null && homeBase != null)
            {
                drone.SetHomeBase(homeBase);
                
                // Инициализируем дрон, если он еще не инициализирован
                if (_navigationService != null && _resourceService != null && _droneService != null)
                {
                    drone.Initialize(_navigationService, _resourceService, _droneService);
                }
            }
            
            return drone;
        }
        
        /// <summary>
        /// Возвращает дрон в пул
        /// </summary>
        public void ReturnDrone(Drone drone)
        {
            if (drone != null && drone is MonoBehaviour droneMono)
            {
                // Очищаем состояние дрона перед возвратом в пул
                drone.ClearTargetResource();
                
                Return(droneMono.gameObject);
            }
        }
    }
}

