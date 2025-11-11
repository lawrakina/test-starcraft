using System.Collections.Generic;
using Core.Interfaces;
using UnityEngine;

namespace Systems.Collision
{
    /// <summary>
    /// Разрешитель столкновений между дронами
    /// Обеспечивает совместное принятие решений о том, кто уступает дорогу
    /// </summary>
    public class CollisionResolver
    {
        // Хранит активные конфликты (пары дронов)
        private readonly HashSet<(int, int)> _activeCollisions = new HashSet<(int, int)>();
        
        // Время жизни конфликта (после разрешения конфликт удаляется через это время)
        private readonly Dictionary<(int, int), float> _collisionTimestamps = new Dictionary<(int, int), float>();
        private readonly List<(int, int)> _keysToRemove = new List<(int, int)>(); // Переиспользуемый список
        private const float CollisionTimeout = 2.0f; // Конфликт считается разрешенным через 2 секунды
        
        /// <summary>
        /// Регистрирует столкновение между двумя дронами
        /// </summary>
        /// <param name="drone1">Первый дрон</param>
        /// <param name="drone2">Второй дрон</param>
        public void RegisterCollision(IDrone drone1, IDrone drone2)
        {
            if (drone1 == null || drone2 == null)
            {
                return;
            }
            
            // Создаем упорядоченную пару (меньший ID всегда первый)
            var collisionKey = GetCollisionKey(drone1.Id, drone2.Id);
            _activeCollisions.Add(collisionKey);
            _collisionTimestamps[collisionKey] = Time.fixedTime;
        }
        
        /// <summary>
        /// Разрешает столкновение между двумя дронами
        /// Возвращает true если первый дрон должен уступить
        /// </summary>
        /// <param name="drone1">Первый дрон</param>
        /// <param name="velocity1">Скорость первого дрона</param>
        /// <param name="drone2">Второй дрон</param>
        /// <param name="velocity2">Скорость второго дрона</param>
        /// <returns>true если drone1 должен уступить</returns>
        public bool ResolveCollision(
            IDrone drone1, Vector3 velocity1,
            IDrone drone2, Vector3 velocity2)
        {
            if (drone1 == null || drone2 == null)
            {
                return false;
            }
            
            // Регистрируем столкновение
            RegisterCollision(drone1, drone2);
            
            // Используем PriorityCalculator для детерминированного решения
            bool shouldYield = PriorityCalculator.ShouldYield(drone1, velocity1, drone2, velocity2);
            
            return shouldYield;
        }
        
        /// <summary>
        /// Проверяет, есть ли активное столкновение между двумя дронами
        /// </summary>
        /// <param name="drone1Id">ID первого дрона</param>
        /// <param name="drone2Id">ID второго дрона</param>
        /// <returns>true если есть активное столкновение</returns>
        public bool HasActiveCollision(int drone1Id, int drone2Id)
        {
            var collisionKey = GetCollisionKey(drone1Id, drone2Id);
            
            if (!_activeCollisions.Contains(collisionKey))
            {
                return false;
            }
            
            // Проверяем, не истек ли таймаут
            if (_collisionTimestamps.TryGetValue(collisionKey, out float timestamp))
            {
                if (Time.fixedTime - timestamp > CollisionTimeout)
                {
                    // Конфликт разрешен, удаляем его
                    _activeCollisions.Remove(collisionKey);
                    _collisionTimestamps.Remove(collisionKey);
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Очищает разрешенные столкновения (устаревшие конфликты)
        /// </summary>
        public void ClearResolvedCollisions()
        {
            _keysToRemove.Clear();
            
            foreach (var collisionKey in _activeCollisions)
            {
                if (_collisionTimestamps.TryGetValue(collisionKey, out float timestamp))
                {
                    if (Time.fixedTime - timestamp > CollisionTimeout)
                    {
                        _keysToRemove.Add(collisionKey);
                    }
                }
            }
            
            foreach (var key in _keysToRemove)
            {
                _activeCollisions.Remove(key);
                _collisionTimestamps.Remove(key);
            }
        }
        
        /// <summary>
        /// Очищает все столкновения (например, при перезапуске)
        /// </summary>
        public void ClearAllCollisions()
        {
            _activeCollisions.Clear();
            _collisionTimestamps.Clear();
        }
        
        /// <summary>
        /// Создает упорядоченный ключ для пары дронов (меньший ID всегда первый)
        /// </summary>
        private (int, int) GetCollisionKey(int id1, int id2)
        {
            return id1 < id2 ? (id1, id2) : (id2, id1);
        }
    }
}

