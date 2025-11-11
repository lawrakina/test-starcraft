using System.Collections.Generic;
using Core.Interfaces;
using UnityEngine;

namespace DroneResourceCollection.Entities.Resource
{
    /// <summary>
    /// Компонент ресурса для сбора дронами
    /// Реализует интерфейс IResource
    /// </summary>
    [RequireComponent(typeof(ResourceVisuals))]
    public class Resource : MonoBehaviour, IResource
    {
        private static int _nextId = 1;
        
        [SerializeField] private int _id;
        private bool _isCollected;
        private Transform _transform;
        
        // Резервации по фракциям - дроны разных фракций не видят резервации друг друга
        private readonly Dictionary<Core.Enums.FactionType, int> _reservationsByFaction = new();

        public int Id => _id;
        public Vector3 Position => _transform != null ? _transform.position : transform.position;
        public bool IsReserved 
        { 
            get => _reservationsByFaction.Count > 0; 
            set 
            {
                // Для обратной совместимости - очищаем все резервации
                if (!value)
                {
                    _reservationsByFaction.Clear();
                }
            }
        }
        public bool IsCollected => _isCollected;

        private void Awake()
        {
            _transform = transform;
            
            // Генерируем уникальный ID при создании
            if (_id == 0)
            {
                _id = _nextId++;
            }
        }

        public bool Reserve(int droneId, Core.Enums.FactionType faction)
        {
            if (_isCollected)
            {
                return false;
            }

            // Проверяем, не зарезервирован ли уже этой фракцией
            if (_reservationsByFaction.ContainsKey(faction))
            {
                return false;
            }

            _reservationsByFaction[faction] = droneId;
            return true;
        }

        public void Release(Core.Enums.FactionType faction)
        {
            _reservationsByFaction.Remove(faction);
        }

        public bool IsReservedByFaction(Core.Enums.FactionType faction)
        {
            return _reservationsByFaction.ContainsKey(faction);
        }
        
        /// <summary>
        /// Получает ID дрона, который зарезервировал ресурс для указанной фракции
        /// Возвращает -1, если ресурс не зарезервирован этой фракцией
        /// </summary>
        public int GetReservedDroneId(Core.Enums.FactionType faction)
        {
            if (_reservationsByFaction.TryGetValue(faction, out int droneId))
            {
                return droneId;
            }
            return -1;
        }
        
        /// <summary>
        /// Проверяет, доступен ли ресурс для указанного дрона
        /// Ресурс доступен, если он не собран и либо не зарезервирован, либо зарезервирован именно этим дроном
        /// </summary>
        public bool IsAvailableForDrone(int droneId, Core.Enums.FactionType faction)
        {
            // Если ресурс собран, он недоступен
            if (_isCollected)
            {
                return false;
            }
            
            // Если ресурс не зарезервирован, он доступен
            if (!IsReservedByFaction(faction))
            {
                return true;
            }
            
            // Если ресурс зарезервирован, проверяем, что это именно этот дрон
            return GetReservedDroneId(faction) == droneId;
        }

        public void Collect()
        {
            if (_isCollected)
            {
                return;
            }

            _isCollected = true;
            // Очищаем все резервации при сборе ресурса
            _reservationsByFaction.Clear();
            
            // Ресурс не уничтожается сразу - он будет прикреплен к дрону
            // и уничтожен только после выгрузки на базе
        }
        
        /// <summary>
        /// Прикрепляет ресурс к дрону (вызывается после сбора)
        /// </summary>
        public void AttachToDrone(Transform droneTransform)
        {
            if (droneTransform != null)
            {
                // Отключаем визуализацию и коллайдеры ресурса
                var visuals = GetComponent<ResourceVisuals>();
                if (visuals != null)
                {
                    visuals.enabled = false;
                }
                
                var collider = GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
                
                // Прикрепляем к дрону
                _transform.SetParent(droneTransform);
                _transform.localPosition = Vector3.up * 1.5f; // Над дроном
                _transform.localRotation = Quaternion.identity;
                _transform.localScale = Vector3.one * 0.5f; // Уменьшаем размер
            }
        }
        
        /// <summary>
        /// Уничтожает ресурс после выгрузки на базе
        /// </summary>
        public void DestroyAfterUnload()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            // При уничтожении освобождаем все резервации
            _reservationsByFaction.Clear();
        }
        
        // Старый метод для обратной совместимости с интерфейсом
        public bool Reserve(int droneId)
        {
            // Не можем резервировать без фракции - возвращаем false
            return false;
        }

        public void Release()
        {
            // Очищаем все резервации
            _reservationsByFaction.Clear();
        }
    }
}

