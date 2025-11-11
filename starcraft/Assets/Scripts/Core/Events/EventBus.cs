using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Система событий для слабой связанности компонентов
    /// Должен быть создан и зарегистрирован через DI контейнер
    /// </summary>
    public class EventBus
    {
        private readonly Dictionary<Type, List<object>> _subscribers = new();

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            var eventType = typeof(T);
            
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<object>();
            }
            
            _subscribers[eventType].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            var eventType = typeof(T);
            
            if (_subscribers.TryGetValue(eventType, out var subscriber))
            {
                subscriber.Remove(handler);
            }
        }

        public void Publish<T>(T eventData) where T : class
        {
            var eventType = typeof(T);
            
            if (!_subscribers.TryGetValue(eventType, out var subscriber))
            {
                return;
            }
            
            var handlers = new List<object>(subscriber);
            
            foreach (var handler in handlers)
            {
                try
                {
                    ((Action<T>)handler)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error handling event {eventType.Name}: {ex.Message}");
                }
            }
        }

        public void Clear()
        {
            _subscribers.Clear();
        }
    }
}

