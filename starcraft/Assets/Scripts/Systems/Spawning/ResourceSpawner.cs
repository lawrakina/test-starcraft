using Core.Interfaces;
using DroneResourceCollection.Entities.Resource;
using UnityEngine;

namespace Systems.Spawning
{
    /// <summary>
    /// Спавнер ресурсов
    /// Создает ресурсы в случайных позициях через заданный интервал
    /// </summary>
    public class ResourceSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject resourcePrefab;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private int maxResources = 20;
        [SerializeField] private Bounds spawnBounds = new(Vector3.zero, new Vector3(20, 1, 20));
        [SerializeField] private LayerMask terrainLayer = -1; // Все слои по умолчанию
        
        private float _spawnTimer;
        private SpawnPointProvider _spawnPointProvider;
        private IResourceService _resourceService;
        private INavigationService _navigationService;
        private int _currentResourceCount;

        public float SpawnInterval
        {
            get => spawnInterval;
            set => spawnInterval = Mathf.Max(0.1f, value);
        }

        public void Initialize(IResourceService resourceService, INavigationService navigationService)
        {
            _resourceService = resourceService;
            _navigationService = navigationService;
            _spawnPointProvider = new SpawnPointProvider(spawnBounds, -1);
        }

        private void Update()
        {
            if (_resourceService == null || _resourceService.AvailableResourceCount >= maxResources)
            {
                return;
            }

            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= spawnInterval)
            {
                SpawnResource();
                _spawnTimer = 0f;
            }
        }

        private void SpawnResource()
        {
            if (!resourcePrefab)
            {
                Debug.LogWarning("Resource prefab is not assigned!");
                return;
            }

            // Получаем начальную позицию для спавна
            var spawnPosition = _spawnPointProvider.GetRandomSpawnPoint();
            
            // Проверяем столкновение с террейном
            if (IsCollidingWithTerrain(spawnPosition))
            {
                // Делаем 2 попытки с повышением высоты на 1 единицу
                bool spawned = false;
                for (int attempt = 1; attempt <= 2; attempt++)
                {
                    // Получаем новую произвольную позицию с увеличенной высотой
                    var newPosition = GetRandomSpawnPointWithHeightOffset(attempt);
                    
                    // Проверяем столкновение с новой позицией
                    if (!IsCollidingWithTerrain(newPosition))
                    {
                        spawnPosition = newPosition;
                        spawned = true;
                        break;
                    }
                }
                
                // Если после всех попыток не удалось найти свободное место, не спавним
                if (!spawned)
                {
                    Debug.LogWarning($"Failed to spawn resource after collision check. Skipping spawn.");
                    return;
                }
            }

            // Создаем ресурс в найденной позиции
            var resourceObject = Instantiate(resourcePrefab, spawnPosition, Quaternion.identity);
            
            var resource = resourceObject.GetComponent<Resource>();
            if (resource)
            {
                _resourceService.RegisterResource(resource);

                _navigationService?.AddTemporaryObstacle(resourceObject);

                _currentResourceCount++;
            }
        }

        /// <summary>
        /// Проверяет столкновение с террейном в указанной позиции
        /// Использует OverlapSphere для проверки наличия коллайдеров террейна в радиусе
        /// </summary>
        private bool IsCollidingWithTerrain(Vector3 position)
        {
            // Используем OverlapSphere для проверки столкновения с террейном
            // Радиус 0.5f должен быть достаточным для проверки столкновения
            Collider[] colliders = Physics.OverlapSphere(position, 0.5f, terrainLayer);
            
            // Если найдены коллайдеры террейна, значит есть столкновение
            if (colliders.Length > 0)
            {
                return true;
            }
            
            // Дополнительная проверка через Raycast вниз (на случай, если позиция находится внутри террейна)
            RaycastHit hit;
            if (Physics.Raycast(position, Vector3.down, out hit, 0.5f, terrainLayer))
            {
                // Если расстояние очень маленькое, значит ресурс внутри террейна
                return hit.distance < 0.1f;
            }
            
            // Проверка Raycast вверх (на случай, если позиция находится под террейном)
            if (Physics.Raycast(position, Vector3.up, out hit, 0.5f, terrainLayer))
            {
                return hit.distance < 0.1f;
            }
            
            return false;
        }

        /// <summary>
        /// Получает случайную позицию спавна с увеличенной высотой
        /// </summary>
        private Vector3 GetRandomSpawnPointWithHeightOffset(int heightOffset)
        {
            // Получаем новую случайную позицию
            var basePosition = _spawnPointProvider.GetRandomSpawnPoint();
            
            // Увеличиваем высоту на указанное количество единиц
            return new Vector3(basePosition.x, basePosition.y + heightOffset, basePosition.z);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(spawnBounds.center, spawnBounds.size);
        }
    }
}


