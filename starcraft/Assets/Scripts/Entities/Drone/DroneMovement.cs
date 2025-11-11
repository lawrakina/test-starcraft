using System.Collections.Generic;
using Core.Enums;
using Core.Interfaces;
using DroneResourceCollection.Systems.Steering;
using Systems.Collision;
using UnityEngine;

namespace Entities.Drone
{
    /// <summary>
    /// Компонент движения дрона
    /// Управляет перемещением дрона по пути с учетом Steering Behaviors
    /// Использует Rigidbody для физического движения и столкновений с террейном
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class DroneMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float arrivalDistance = 0.5f;
        [SerializeField] private float rotationSpeed = 5f;

        private Vector3 _targetPosition;
        private List<Vector3> _currentPath;
        private int _currentPathIndex;
        private Vector3 _velocity;
        private bool _hasTarget;

        private Rigidbody _rigidbody;
        private SteeringManager _steeringManager;
        private INavigationService _navigationService;
        private IDrone _drone;
        private IDroneService _droneService;
        private CollisionResolver _collisionResolver;

        // Для обхода блокирующих дронов
        private IDrone _blockingDrone;
        private float _lastPathRecalculationTime;
        private const float PathRecalculationCooldown = 0.5f; // Минимальное время между пересчетами пути
        private const float ObstacleDetectionRadius = 3f; // Радиус обнаружения препятствий
        
        // Для проверки застревания
        private Vector3 _lastPosition;
        private float _lastPositionCheckTime;
        private const float StuckCheckInterval = 0.5f; // Интервал проверки застревания
        private const float StuckDistanceThreshold = 0.5f; // Порог расстояния для определения застревания
        private const float StuckTimeThreshold = 1.0f; // Время в секундах, после которого считается застреванием

        public float Speed
        {
            get => speed;
            set => speed = Mathf.Max(0.1f, value);
        }

        public bool HasTarget => _hasTarget;
        public Vector3 Velocity => _velocity;
        public List<Vector3> CurrentPath => _currentPath;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
            }

            // Настраиваем Rigidbody для движения без гравитации и с ограничением вращения
            _rigidbody.useGravity = false;
            _rigidbody.freezeRotation = true;
            _rigidbody.linearDamping = 2f; // Добавляем сопротивление для плавного движения
        }

        public void Initialize(
            IDrone drone, 
            INavigationService navigationService, 
            SteeringManager steeringManager,
            IDroneService droneService = null,
            CollisionResolver collisionResolver = null)
        {
            _drone = drone;
            _navigationService = navigationService;
            _steeringManager = steeringManager;
            _droneService = droneService;
            _collisionResolver = collisionResolver ?? new CollisionResolver();
        }

        public void SetTargetPosition(Vector3 position)
        {
            _targetPosition = position;
            _hasTarget = true;
            _currentPathIndex = 0;
            
            // Сбрасываем проверку застревания при установке новой цели
            _lastPosition = _drone != null ? _drone.Position : transform.position;
            _lastPositionCheckTime = Time.fixedTime;

            if (_navigationService != null && _drone != null)
            {
                _currentPath = _navigationService.CalculatePath(_drone.Position, position);

                if (_currentPath == null || _currentPath.Count == 0)
                {
                    _currentPath = new List<Vector3> { position };
                }
            }
            else
            {
                _currentPath = new List<Vector3> { position };
            }
        }

        public void ClearTarget()
        {
            _hasTarget = false;
            _currentPath = null;
            _velocity = Vector3.zero;

            // Останавливаем Rigidbody
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
            }
        }

        private void FixedUpdate()
        {
            if (!_hasTarget || _currentPath == null || _currentPath.Count == 0 || _rigidbody == null)
            {
                return;
            }

            // Проверяем застревание - если путь активен но позиция не меняется
            CheckForStuck();

            // Проверяем наличие блокирующего дрона через AvoidanceBehavior
            CheckForBlockingDrone();
            
            // Проверяем наличие стоящих/добывающих дронов как препятствий
            CheckForStandingOrCollectingDrones();

            // Если есть блокирующий дрон и прошло достаточно времени, пересчитываем путь для обхода
            if (_blockingDrone != null && Time.fixedTime - _lastPathRecalculationTime > PathRecalculationCooldown)
            {
                RecalculatePathToAvoid(_blockingDrone);
                _lastPathRecalculationTime = Time.fixedTime;
            }

            var currentTarget = _currentPath[_currentPathIndex];
            var direction = (currentTarget - transform.position);
            var distance = direction.magnitude;

            if (distance < arrivalDistance)
            {
                _currentPathIndex++;

                if (_currentPathIndex >= _currentPath.Count)
                {
                    ClearTarget();
                    return;
                }

                currentTarget = _currentPath[_currentPathIndex];
                direction = (currentTarget - transform.position);
            }

            direction.Normalize();
            Vector3 desiredVelocity = direction * speed;

            Vector3 steeringForce = Vector3.zero;
            if (_steeringManager != null)
            {
                steeringForce = _steeringManager.CalculateSteering(transform.position, _rigidbody.linearVelocity, speed);
            }

            // Комбинируем желаемую скорость и steering force
            // Steering force имеет приоритет при избежании столкновений
            _velocity = desiredVelocity + steeringForce;

            // Ограничиваем максимальную скорость, но позволяем steering force влиять на направление
            if (_velocity.magnitude > speed)
            {
                _velocity = _velocity.normalized * speed;
            }

            // Если steering force очень большой (экстренное избежание), уменьшаем желаемую скорость
            if (steeringForce.magnitude > speed * 0.5f)
            {
                _velocity = steeringForce.normalized * speed;
            }

            // Применяем скорость через Rigidbody для физического движения и столкновений
            if (_velocity.magnitude > 0.01f)
            {
                _rigidbody.linearVelocity = _velocity;

                // Поворот дрона в направлении движения
                Quaternion targetRotation = Quaternion.LookRotation(_velocity.normalized);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            }
            else
            {
                _rigidbody.linearVelocity = Vector3.zero;
            }
        }

        private void OnDrawGizmos()
        {
            if (_currentPath is { Count: > 0 })
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < _currentPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(_currentPath[i], _currentPath[i + 1]);
                }

                if (_currentPathIndex < _currentPath.Count)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(_currentPath[_currentPathIndex], 0.3f);
                }
            }
        }

        /// <summary>
        /// Проверяет застревание дрона - если путь активен но позиция не меняется
        /// </summary>
        private void CheckForStuck()
        {
            if (_drone == null)
            {
                return;
            }

            Vector3 currentPosition = _drone.Position;
            float timeSinceLastCheck = Time.fixedTime - _lastPositionCheckTime;

            // Проверяем каждые StuckCheckInterval секунд
            if (timeSinceLastCheck >= StuckCheckInterval)
            {
                float distanceMoved = Vector3.Distance(currentPosition, _lastPosition);

                // Если дрон не двигается (с точностью StuckDistanceThreshold) и прошло достаточно времени
                if (distanceMoved < StuckDistanceThreshold && timeSinceLastCheck >= StuckTimeThreshold)
                {
                    // Дрон застрял - перестраиваем путь
                    RecalculatePath();
                }

                // Обновляем последнюю позицию и время
                _lastPosition = currentPosition;
                _lastPositionCheckTime = Time.fixedTime;
            }
        }

        /// <summary>
        /// Пересчитывает путь к текущей цели
        /// </summary>
        private void RecalculatePath()
        {
            if (_navigationService == null || _drone == null || !_hasTarget)
            {
                return;
            }

            // Пересчитываем путь к цели
            var newPath = _navigationService.CalculatePath(_drone.Position, _targetPosition);

            if (newPath != null && newPath.Count > 0)
            {
                _currentPath = newPath;
                _currentPathIndex = 0;
                _lastPathRecalculationTime = Time.fixedTime;
                
                // Сбрасываем проверку застревания
                _lastPosition = _drone.Position;
                _lastPositionCheckTime = Time.fixedTime;
            }
        }

        /// <summary>
        /// Проверяет наличие блокирующего дрона через AvoidanceBehavior
        /// </summary>
        private void CheckForBlockingDrone()
        {
            if (_steeringManager != null)
            {
                // Получаем AvoidanceBehavior из SteeringManager
                var avoidanceBehavior = GetAvoidanceBehavior();
                if (avoidanceBehavior != null)
                {
                    _blockingDrone = avoidanceBehavior.GetBlockingDrone();
                }
            }
        }

        /// <summary>
        /// Получает AvoidanceBehavior из SteeringManager
        /// </summary>
        private AvoidanceBehavior GetAvoidanceBehavior()
        {
            if (_steeringManager != null)
            {
                return _steeringManager.GetBehavior<AvoidanceBehavior>();
            }

            return null;
        }

        /// <summary>
        /// Проверяет наличие стоящих или добывающих дронов как препятствий
        /// </summary>
        private void CheckForStandingOrCollectingDrones()
        {
            if (_droneService == null || _drone == null)
            {
                return;
            }
            
            // Находим ближайших дронов
            List<IDrone> nearbyDrones = _droneService.FindNearbyDrones(
                _drone.Position, 
                ObstacleDetectionRadius, 
                _drone.Id);
            
            if (nearbyDrones.Count == 0)
            {
                return;
            }
            
            // Ищем стоящих или добывающих дронов на пути
            IDrone obstacleDrone = null;
            float minDistance = float.MaxValue;
            bool needsPathRecalculation = false;
            
            foreach (IDrone nearbyDrone in nearbyDrones)
            {
                float distance = Vector3.Distance(_drone.Position, nearbyDrone.Position);
                
                // Проверяем, находится ли дрон на пути к цели
                if (IsOnPath(nearbyDrone.Position))
                {
                    bool isStanding = nearbyDrone.IsStanding;
                    bool isCollecting = nearbyDrone.CurrentState == DroneState.Collecting;
                    bool isOpponent = nearbyDrone.Faction != _drone.Faction;
                    
                    // Правило 1: Стоящие дроны всегда блокируют путь
                    if (isStanding)
                    {
                        // Особенно важно обойти стоящего противника
                        if (isOpponent || distance < minDistance)
                        {
                            minDistance = distance;
                            obstacleDrone = nearbyDrone;
                            needsPathRecalculation = true;
                        }
                    }
                    // Правило 2: Добывающие дроны блокируют путь
                    else if (isCollecting)
                    {
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            obstacleDrone = nearbyDrone;
                            needsPathRecalculation = true;
                        }
                    }
                }
            }
            
            // Если нашли препятствие и прошло достаточно времени, пересчитываем путь
            if (needsPathRecalculation && obstacleDrone != null && 
                Time.fixedTime - _lastPathRecalculationTime > PathRecalculationCooldown)
            {
                _blockingDrone = obstacleDrone;
                RecalculatePathToAvoid(obstacleDrone);
                _lastPathRecalculationTime = Time.fixedTime;
            }
        }
        
        /// <summary>
        /// Проверяет, находится ли точка на пути дрона
        /// </summary>
        private bool IsOnPath(Vector3 point)
        {
            if (_currentPath == null || _currentPath.Count == 0)
            {
                return false;
            }
            
            // Проверяем расстояние от точки до ближайшего сегмента пути
            float minDistanceToPath = float.MaxValue;
            
            for (int i = 0; i < _currentPath.Count - 1; i++)
            {
                Vector3 segmentStart = i == 0 ? _drone.Position : _currentPath[i];
                Vector3 segmentEnd = _currentPath[i + 1];
                
                // Вычисляем расстояние от точки до сегмента
                float distanceToSegment = DistanceToSegment(point, segmentStart, segmentEnd);
                minDistanceToPath = Mathf.Min(minDistanceToPath, distanceToSegment);
            }
            
            // Если точка находится близко к пути (в пределах радиуса обнаружения)
            return minDistanceToPath < ObstacleDetectionRadius * 0.5f;
        }
        
        /// <summary>
        /// Вычисляет расстояние от точки до сегмента линии
        /// </summary>
        private float DistanceToSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
        {
            Vector3 segment = segmentEnd - segmentStart;
            Vector3 toPoint = point - segmentStart;
            
            float segmentLength = segment.magnitude;
            if (segmentLength < 0.001f)
            {
                return Vector3.Distance(point, segmentStart);
            }
            
            float t = Mathf.Clamp01(Vector3.Dot(toPoint, segment) / (segmentLength * segmentLength));
            Vector3 closestPoint = segmentStart + segment * t;
            
            return Vector3.Distance(point, closestPoint);
        }
        
        /// <summary>
        /// Пересчитывает путь для обхода блокирующего дрона
        /// </summary>
        private void RecalculatePathToAvoid(IDrone blockingDrone)
        {
            if (_navigationService == null || _drone == null || blockingDrone == null)
            {
                return;
            }

            // Вычисляем точку обхода - в сторону от блокирующего дрона
            Vector3 toBlocking = (blockingDrone.Position - _drone.Position).normalized;
            Vector3 perpendicular = Vector3.Cross(toBlocking, Vector3.up).normalized;

            // Выбираем направление обхода (вправо или влево) на основе ID для стабильности
            float direction = (blockingDrone.Id % 2 == 0) ? 1f : -1f;
            
            // Увеличиваем отступ для стоящих/добывающих дронов
            bool isStandingOrCollecting = blockingDrone.IsStanding || 
                                         blockingDrone.CurrentState == DroneState.Collecting;
            float avoidanceDistance = isStandingOrCollecting ? 4f : 3f; // Больший отступ для важных препятствий
            
            Vector3 avoidanceOffset = perpendicular * direction * avoidanceDistance;

            // Вычисляем промежуточную точку обхода
            Vector3 avoidancePoint = blockingDrone.Position + avoidanceOffset;

            // Проецируем точку обхода на NavMesh
            if (_navigationService.GetNearestNavMeshPoint(avoidancePoint, out Vector3 navMeshPoint))
            {
                avoidancePoint = navMeshPoint;
            }

            // Пересчитываем путь через точку обхода
            var pathToAvoidance = _navigationService.CalculatePath(_drone.Position, avoidancePoint);
            var pathFromAvoidance = _navigationService.CalculatePath(avoidancePoint, _targetPosition);

            if (pathToAvoidance != null && pathFromAvoidance != null)
            {
                // Объединяем пути
                _currentPath = new List<Vector3>(pathToAvoidance);
                _currentPath.AddRange(pathFromAvoidance);
                _currentPathIndex = 0;
            }
            else if (pathToAvoidance != null)
            {
                // Если путь от точки обхода не найден, используем только путь к точке обхода
                _currentPath = pathToAvoidance;
                _currentPathIndex = 0;
            }
        }
    }
}