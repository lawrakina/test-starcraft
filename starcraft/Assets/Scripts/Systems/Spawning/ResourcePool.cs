using Core.Interfaces;
using DroneResourceCollection.Entities.Resource;
using UnityEngine;

namespace Systems.Spawning
{
    /// <summary>
    /// Пул для ресурсов
    /// Управляет переиспользованием ресурсов для оптимизации производительности
    /// </summary>
    public class ResourcePool : ObjectPool
    {
        private IResourceService _resourceService;
        private INavigationService _navigationService;
        
        /// <summary>
        /// Инициализирует пул ресурсов
        /// </summary>
        public void Initialize(
            IResourceService resourceService,
            INavigationService navigationService)
        {
            _resourceService = resourceService;
            _navigationService = navigationService;
        }
        
        protected override GameObject CreatePooledObject()
        {
            GameObject obj = base.CreatePooledObject();
            
            // Ресурс будет зарегистрирован при активации
            return obj;
        }
        
        /// <summary>
        /// Получает ресурс из пула и инициализирует его
        /// </summary>
        public Resource GetResource(Vector3 position)
        {
            GameObject obj = Get();
            if (obj == null)
            {
                return null;
            }
            
            obj.transform.position = position;
            
            Resource resource = obj.GetComponent<Resource>();
            if (resource != null && _resourceService != null)
            {
                _resourceService.RegisterResource(resource);
                
                if (_navigationService != null)
                {
                    _navigationService.AddTemporaryObstacle(obj);
                }
            }
            
            return resource;
        }
        
        /// <summary>
        /// Возвращает ресурс в пул
        /// </summary>
        public void ReturnResource(Resource resource)
        {
            if (resource != null && resource is MonoBehaviour resourceMono)
            {
                // Отменяем регистрацию ресурса
                if (_resourceService != null)
                {
                    _resourceService.UnregisterResource(resource);
                }
                
                if (_navigationService != null)
                {
                    _navigationService.RemoveTemporaryObstacle(resourceMono.gameObject);
                }
                
                Return(resourceMono.gameObject);
            }
        }
    }
}

