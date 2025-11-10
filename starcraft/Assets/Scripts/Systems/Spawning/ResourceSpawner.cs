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

            var spawnPosition = _spawnPointProvider.GetRandomSpawnPoint();
            var resourceObject = Instantiate(resourcePrefab, spawnPosition, Quaternion.identity);
            
            var resource = resourceObject.GetComponent<Resource>();
            if (resource)
            {
                _resourceService.RegisterResource(resource);

                _navigationService?.AddTemporaryObstacle(resourceObject);

                _currentResourceCount++;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(spawnBounds.center, spawnBounds.size);
        }
    }
}

