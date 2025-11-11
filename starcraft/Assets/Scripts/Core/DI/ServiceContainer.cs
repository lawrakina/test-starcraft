using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.DI
{
    /// <summary>
    /// Простой контейнер для Dependency Injection
    /// Управляет регистрацией и получением сервисов
    /// </summary>
    public class ServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        
        /// <summary>
        /// Регистрирует сервис по типу
        /// </summary>
        public void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }
        
        /// <summary>
        /// Регистрирует сервис по конкретному типу
        /// </summary>
        public void Register(Type serviceType, object service)
        {
            if (!serviceType.IsInstanceOfType(service))
            {
                throw new ArgumentException($"Service {service.GetType().Name} is not assignable to {serviceType.Name}");
            }
            _services[serviceType] = service;
        }
        
        /// <summary>
        /// Получает сервис по типу
        /// </summary>
        public T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return service as T;
            }
            return null;
        }
        
        /// <summary>
        /// Получает сервис по типу
        /// </summary>
        public object Get(Type serviceType)
        {
            if (_services.TryGetValue(serviceType, out var service))
            {
                return service;
            }
            return null;
        }
        
        /// <summary>
        /// Проверяет, зарегистрирован ли сервис
        /// </summary>
        public bool IsRegistered<T>()
        {
            return _services.ContainsKey(typeof(T));
        }
        
        /// <summary>
        /// Очищает все зарегистрированные сервисы
        /// </summary>
        public void Clear()
        {
            _services.Clear();
        }
    }
}

