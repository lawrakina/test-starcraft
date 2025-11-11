using Core.Interfaces;
using DroneResourceCollection.Entities.Resource;
using UnityEngine;

namespace Systems.Spawning
{
    /// <summary>
    /// Спавнер ресурсов
    /// Создает ресурсы в случайных позициях через заданный интервал
    /// </summary>
    public class ResourceSpawner : MonoBehaviour, IUpdatable, IInitializable
    {
        [SerializeField] private GameObject resourcePrefab;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private int maxResources = 20;
        [SerializeField] private Bounds spawnBounds = new(Vector3.zero, new Vector3(20, 1, 20));
        [SerializeField] private LayerMask terrainLayer = -1; // Все слои по умолчанию
        [SerializeField] private int updatePriority = 500;
        [SerializeField] private ResourcePool resourcePool; // Опциональный пул ресурсов
        
        private float _spawnTimer;
        private SpawnPointProvider _spawnPointProvider;
        private IResourceService _resourceService;
        private INavigationService _navigationService;
        private int _currentResourceCount;
        private Collider[] _colliderBuffer = new Collider[10];
        private bool _isInitialized = false;
        private bool _usePooling = false;
        
        public int UpdatePriority => updatePriority;
        public int InitializationPhase => 1; // Вторая фаза - игровые системы
        public System.Type[] Dependencies => new[] { typeof(SimulationManager) };

        public float SpawnInterval
        {
            get => spawnInterval;
            set => spawnInterval = Mathf.Max(0.1f, value);
        }
        
        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }
            
            // Получаем SimulationManager
            var simulationManager = SimulationManager.Instance;
            if (simulationManager == null)
            {
                Debug.LogError("[ResourceSpawner] SimulationManager not found!");
                return;
            }
            
            _resourceService = simulationManager.ResourceService;
            _navigationService = simulationManager.NavigationService;
            _spawnPointProvider = new SpawnPointProvider(spawnBounds, -1);
            
            // Инициализируем пул, если он назначен
            if (resourcePool != null)
            {
                resourcePool.Initialize(_resourceService, _navigationService);
                _usePooling = true;
            }
            
            _isInitialized = true;
            
            // Регистрируемся в UpdateManager после инициализации
            RegisterWithUpdateManager();
            
            Debug.Log($"[ResourceSpawner] Initialized successfully. ResourceService: {_resourceService != null}, NavigationService: {_navigationService != null}");
        }
        
        public void Initialize(IResourceService resourceService, INavigationService navigationService)
        {
            _resourceService = resourceService;
            _navigationService = navigationService;
            _spawnPointProvider = new SpawnPointProvider(spawnBounds, -1);
            _isInitialized = true;
        }
        
        private void OnEnable()
        {
            // Регистрируемся в UpdateManager, если он доступен
            RegisterWithUpdateManager();
        }
        
        private void OnDisable()
        {
            var updateManager = Core.DI.DependencyHelper.GetUpdateManager();
            if (updateManager != null)
            {
                updateManager.UnregisterUpdatable(this);
            }
        }
        
        private void RegisterWithUpdateManager()
        {
            var updateManager = Core.DI.DependencyHelper.GetUpdateManager();
            if (updateManager != null)
            {
                updateManager.RegisterUpdatable(this);
            }
        }

        public void OnUpdate(float deltaTime)
        {
            // Проверяем инициализацию
            if (!_isInitialized || _resourceService == null)
            {
                // Пытаемся зарегистрироваться в UpdateManager, если еще не зарегистрированы
                RegisterWithUpdateManager();
                return;
            }
            
            if (_resourceService.AvailableResourceCount >= maxResources)
            {
                return;
            }

            _spawnTimer += deltaTime;

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
            DroneResourceCollection.Entities.Resource.Resource resource = null;
            
            if (_usePooling && resourcePool != null)
            {
                // Используем пул
                resource = resourcePool.GetResource(spawnPosition);
            }
            else
            {
                // Используем обычное создание
                var resourceObject = Instantiate(resourcePrefab, spawnPosition, Quaternion.identity);
                resource = resourceObject.GetComponent<Resource>();
                if (resource)
                {
                    _resourceService.RegisterResource(resource);
                    _navigationService?.AddTemporaryObstacle(resourceObject);
                }
            }
            
            if (resource != null)
            {
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
            int count = Physics.OverlapSphereNonAlloc(position, 0.5f, _colliderBuffer, terrainLayer);
            
            // Если найдены коллайдеры террейна, значит есть столкновение
            if (count > 0)
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


