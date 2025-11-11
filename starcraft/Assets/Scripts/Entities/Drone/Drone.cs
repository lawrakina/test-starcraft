using Core.Enums;
using Core.Events;
using Core.Interfaces;
using DroneResourceCollection.Entities.Drone;
using Systems.Collision;
using UnityEngine;

namespace Entities.Drone
{
    /// <summary>
    /// Главный компонент дрона
    /// Реализует интерфейс IDrone и координирует работу всех подсистем
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(DroneMovement))]
    [RequireComponent(typeof(DroneSteering))]
    [RequireComponent(typeof(DroneStateMachine))]
    [RequireComponent(typeof(DroneVisuals))]
    [RequireComponent(typeof(DronePathRenderer))]
    [RequireComponent(typeof(DroneCollectionProgressIndicator))]
    public class Drone : MonoBehaviour, IDrone
    {
        private static int _nextId = 1;
        
        [SerializeField] private int id;
        [SerializeField] private FactionType faction;
        [SerializeField] private IBase _homeBase;
        
        // Случайный приоритет для разрешения конфликтов при столкновениях
        private int _priority;
        
        private DroneMovement _movement;
        private DroneSteering _steering;
        private DroneStateMachine _stateMachine;
        private DroneVisuals _visuals;
        private DronePathRenderer _pathRenderer;
        
        private IResource _targetResource;
        private INavigationService _navigationService;
        private IResourceService _resourceService;
        private IDroneService _droneService;
        private CollisionResolver _collisionResolver;

        public int Id => id;
        public FactionType Faction => faction;
        public DroneState CurrentState => _stateMachine != null ? _stateMachine.CurrentState : DroneState.Idle;
        public Vector3 Position => transform.position;
        public int Priority => _priority;
        public float Speed 
        { 
            get => _movement ? _movement.Speed : 5f; 
            set 
            { 
                if (_movement)
                {
                    _movement.Speed = value;
                }
            }
        }
        public IBase HomeBase => _homeBase;
        public IResource TargetResource => _targetResource;
        
        public bool IsStanding
        {
            get
            {
                if (_movement == null)
                {
                    return true; // Если компонент движения отсутствует, считаем что стоит
                }
                Vector3 velocity = _movement.Velocity;
                return velocity.magnitude < 0.1f; // Порог для определения стоящего дрона
            }
        }
        
        public Vector3 Velocity
        {
            get
            {
                if (_movement == null)
                {
                    return Vector3.zero;
                }
                return _movement.Velocity;
            }
        }

        private void Awake()
        {
            // Генерируем уникальный ID
            if (id == 0)
            {
                id = _nextId++;
            }
            
            // Генерируем случайный приоритет для разрешения конфликтов
            _priority = Random.Range(0, 10000);
            
            // Получаем компоненты
            _movement = GetComponent<DroneMovement>();
            _steering = GetComponent<DroneSteering>();
            _stateMachine = GetComponent<DroneStateMachine>();
            _visuals = GetComponent<DroneVisuals>();
            _pathRenderer = GetComponent<DronePathRenderer>();
        }

        public void Initialize(
            INavigationService navigationService,
            IResourceService resourceService,
            IDroneService droneService,
            CollisionResolver collisionResolver = null)
        {
            _navigationService = navigationService;
            _resourceService = resourceService;
            _droneService = droneService;
            
            // Создаем общий CollisionResolver для этого дрона (или используем переданный)
            _collisionResolver = collisionResolver ?? new CollisionResolver();
            
            // Инициализируем компоненты с передачей CollisionResolver и DroneService
            _steering.Initialize(this, _droneService, _collisionResolver);
            _movement.Initialize(this, _navigationService, _steering.SteeringManager, _droneService, _collisionResolver);
            _stateMachine.Initialize(this, _resourceService);
            
            _droneService.RegisterDrone(this);
            
            EventBus.Instance.Publish(new DroneSpawnedEvent(this));
        }

        public void SetTargetPosition(Vector3 position)
        {
            if (_movement)
            {
                _movement.SetTargetPosition(position);
            }
        }

        public void SetTargetResource(IResource resource)
        {
            IResource oldResource = _targetResource;
            _targetResource = resource;
            
            Debug.Log($"[Drone] Drone {Id} SetTargetResource: Old={(oldResource != null ? $"Resource {oldResource.Id}" : "null")}, " +
                     $"New={(resource != null ? $"Resource {resource.Id}" : "null")}, State={CurrentState}");
            
            if (resource != null && _movement)
            {
                _movement.SetTargetPosition(resource.Position);
            }
        }

        public void ClearTargetResource()
        {
            if (_targetResource != null)
            {
                int resourceId = _targetResource.Id;
                Debug.Log($"[Drone] Drone {Id} ClearTargetResource: Clearing Resource {resourceId}, State={CurrentState}");
                
                // Освобождаем резервацию для фракции дрона
                if (_targetResource is DroneResourceCollection.Entities.Resource.Resource resource)
                {
                    // Проверяем, что ресурс не уничтожен перед освобождением
                    if (resource)
                    {
                        resource.Release(faction);
                        Debug.Log($"[Drone] Drone {Id} released reservation for Resource {resourceId}");
                    }
                    else
                    {
                        Debug.LogWarning($"[Drone] Drone {Id} tried to release destroyed Resource {resourceId}");
                    }
                }
                else
                {
                    // Для других реализаций используем старый метод
                    _targetResource.Release();
                }
                _targetResource = null;
            }
            else
            {
                Debug.Log($"[Drone] Drone {Id} ClearTargetResource: No target resource to clear, State={CurrentState}");
            }
            
            if (_movement)
            {
                _movement.ClearTarget();
            }
        }
        
        /// <summary>
        /// Проверяет, является ли целевой ресурс валидным (не null и не уничтожен)
        /// </summary>
        public bool IsTargetResourceValid()
        {
            if (_targetResource == null)
            {
                return false;
            }
            
            // Если ресурс - MonoBehaviour, проверяем, что он не уничтожен
            if (_targetResource is MonoBehaviour resourceMono)
            {
                return resourceMono != null;
            }
            
            // Для других реализаций считаем валидным, если не null
            return true;
        }

        public void SetShowPath(bool show)
        {
            if (_pathRenderer != null)
            {
                _pathRenderer.ShowPath = show;
            }
        }

        
        public void SetHomeBase(global::Entities.Base.Base homeBase)
        {
            this._homeBase = homeBase;
            if (homeBase != null)
            {
                faction = homeBase.Faction;
            }
        }

        private void OnDestroy()
        {
            ClearTargetResource();

            _droneService?.UnregisterDrone(this);
        }
    }
}

