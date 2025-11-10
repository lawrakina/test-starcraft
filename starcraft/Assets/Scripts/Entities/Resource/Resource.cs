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
        private bool _isReserved;
        private bool _isCollected;
        private int _reservedByDroneId = -1;

        public int Id => _id;
        public Vector3 Position => transform.position;
        public bool IsReserved 
        { 
            get => _isReserved; 
            set => _isReserved = value; 
        }
        public bool IsCollected => _isCollected;

        private void Awake()
        {
            // Генерируем уникальный ID при создании
            if (_id == 0)
            {
                _id = _nextId++;
            }
        }

        public bool Reserve(int droneId)
        {
            if (_isReserved || _isCollected)
            {
                return false;
            }

            _isReserved = true;
            _reservedByDroneId = droneId;
            return true;
        }

        public void Release()
        {
            _isReserved = false;
            _reservedByDroneId = -1;
        }

        public void Collect()
        {
            if (_isCollected)
            {
                return;
            }

            _isCollected = true;
            _isReserved = false;
            
            // Уничтожаем GameObject ресурса
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            // При уничтожении освобождаем резервацию
            Release();
        }
    }
}

