using Core.Enums;
using Core.Events;
using Core.Interfaces;
using DroneResourceCollection.Entities.Drone;
using Systems.Collision;
using UnityEngine;

namespace Entities.Drone
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(DroneMovement))]
    [RequireComponent(typeof(DroneSteering))]
    [RequireComponent(typeof(DroneStateMachine))]
    [RequireComponent(typeof(DroneVisuals))]
    [RequireComponent(typeof(DronePathRenderer))]
    [RequireComponent(typeof(DroneCollectionProgressIndicator))]
    public class Drone : MonoBehaviour, IDrone, IInitializable
    {
        private static int _nextId = 1;
        
        [SerializeField] private int id;
        [SerializeField] private FactionType faction;
        [SerializeField] private IBase _homeBase;
        
        private int _priority;
        private Transform _transform;
        private bool _isInitialized = false;
        
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
        public Vector3 Position => _transform != null ? _transform.position : transform.position;
        public int Priority => _priority;
        
        public int InitializationPhase => 2; // Третья фаза - визуальные системы
        public System.Type[] Dependencies => new[] { typeof(Base.Base) };
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
                    return true;
                }
                Vector3 velocity = _movement.Velocity;
                return velocity.magnitude < 0.1f;
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
            _transform = transform;
            
            if (id == 0)
            {
                id = _nextId++;
            }
            
            _priority = Random.Range(0, 10000);
            
            _movement = GetComponent<DroneMovement>();
            _steering = GetComponent<DroneSteering>();
            _stateMachine = GetComponent<DroneStateMachine>();
            _visuals = GetComponent<DroneVisuals>();
            _pathRenderer = GetComponent<DronePathRenderer>();
            
        }
        
        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }
            
            var simulationManager = SimulationManager.Instance;
            if (simulationManager == null)
            {
                Debug.LogWarning($"[Drone] SimulationManager not found for drone {id}, using manual initialization");
                return;
            }
            
            Initialize(
                simulationManager.NavigationService,
                simulationManager.ResourceService,
                simulationManager.DroneService
            );
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
            
            _collisionResolver = collisionResolver ?? new CollisionResolver();
            
            _steering.Initialize(this, _droneService, _collisionResolver);
            _movement.Initialize(this, _navigationService, _steering.SteeringManager, _droneService, _collisionResolver);
            _stateMachine.Initialize(this, _resourceService);
            
            _droneService.RegisterDrone(this);
            
            var eventBus = Core.DI.DependencyHelper.GetEventBus();
            eventBus?.Publish(new DroneSpawnedEvent(this));
            
            _isInitialized = true;
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
            _targetResource = resource;
            
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
                
                if (_targetResource is DroneResourceCollection.Entities.Resource.Resource resource)
                {
                    if (resource)
                    {
                        resource.Release(faction);
                    }
                    else
                    {
                        Debug.LogWarning($"[Drone] Drone {Id} tried to release destroyed Resource {resourceId}");
                    }
                }
                else
                {
                    _targetResource.Release();
                }
                _targetResource = null;
            }
            
            if (_movement)
            {
                _movement.ClearTarget();
            }
        }
        
        public bool IsTargetResourceValid()
        {
            if (_targetResource == null)
            {
                return false;
            }
            
            if (_targetResource is MonoBehaviour resourceMono)
            {
                return resourceMono != null;
            }
            
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
            
            if (_visuals != null)
            {
                _visuals.RefreshColor();
            }
            
            if (_pathRenderer != null)
            {
                _pathRenderer.RefreshPathColor();
            }
        }

        private void OnDestroy()
        {
            ClearTargetResource();

            _droneService?.UnregisterDrone(this);
        }
    }
}

