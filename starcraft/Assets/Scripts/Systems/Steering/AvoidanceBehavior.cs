using System.Collections.Generic;
using Core.Enums;
using Core.Interfaces;
using Systems.Collision;
using UnityEngine;

namespace DroneResourceCollection.Systems.Steering
{
    /// <summary>
    /// Поведение обхода (Avoidance)
    /// Дрон с низким приоритетом обходит дрона с высоким приоритетом при столкновении
    /// Учитывает состояние дронов (стоящие, добывающие), фракцию и использует совместное разрешение конфликтов
    /// </summary>
    public class AvoidanceBehavior : SteeringBehavior
    {
        private readonly IDrone _drone;
        private readonly IDroneService _droneService;
        private readonly CollisionResolver _collisionResolver;
        private readonly float _collisionDetectionRadius;
        private readonly float _avoidanceStrength;
        private readonly float _stuckDetectionTime = 1f; // Время для определения застревания
        private readonly float _stuckDistance = 1.0f; // Расстояние для определения застревания
        
        private float _stuckTimer;
        private IDrone _blockingDrone;
        private Vector3 _lastPosition;
        private float _lastPositionTime;

        public AvoidanceBehavior(
            IDrone drone, 
            IDroneService droneService, 
            CollisionResolver collisionResolver = null,
            float collisionDetectionRadius = 2f, 
            float avoidanceStrength = 3f)
        {
            _drone = drone;
            _droneService = droneService;
            _collisionResolver = collisionResolver ?? new CollisionResolver();
            _collisionDetectionRadius = collisionDetectionRadius;
            _avoidanceStrength = avoidanceStrength;
            _lastPosition = drone.Position;
            _lastPositionTime = Time.fixedTime;
        }

        public override Vector3 Calculate(Vector3 position, Vector3 velocity, float maxSpeed)
        {
            // Проверяем, не застряли ли мы
            float timeSinceLastMove = Time.fixedTime - _lastPositionTime;
            float distanceMoved = Vector3.Distance(position, _lastPosition);
            
            if (distanceMoved < _stuckDistance && timeSinceLastMove > 0.1f)
            {
                _stuckTimer += Time.fixedDeltaTime;
            }
            else
            {
                _stuckTimer = 0f;
                _lastPosition = position;
                _lastPositionTime = Time.fixedTime;
            }

            // Находим ближайших дронов
            List<IDrone> nearbyDrones = _droneService.FindNearbyDrones(position, _collisionDetectionRadius, _drone.Id);
            
            if (nearbyDrones.Count == 0)
            {
                _blockingDrone = null;
                return Vector3.zero;
            }

            // Ищем дрона, который блокирует наш путь
            IDrone blockingDrone = null;
            float minDistance = float.MaxValue;
            bool shouldYield = false;

            foreach (IDrone nearbyDrone in nearbyDrones)
            {
                float distance = Vector3.Distance(position, nearbyDrone.Position);
                
                // Получаем скорость ближайшего дрона
                Vector3 nearbyVelocity = nearbyDrone.Velocity;
                bool nearbyIsStanding = nearbyDrone.IsStanding;
                bool nearbyIsCollecting = nearbyDrone.CurrentState == DroneState.Collecting;
                bool isOpponent = nearbyDrone.Faction != _drone.Faction;
                
                // Правило 1: Стоящие дроны всегда имеют приоритет - их всегда обходим
                if (nearbyIsStanding)
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        blockingDrone = nearbyDrone;
                        shouldYield = true;
                    }
                    continue; // Стоящий дрон всегда блокирует, проверяем следующий
                }
                
                // Правило 2: Добывающие дроны имеют высокий приоритет - их всегда обходим
                if (nearbyIsCollecting)
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        blockingDrone = nearbyDrone;
                        shouldYield = true;
                    }
                    continue; // Добывающий дрон всегда блокирует, проверяем следующий
                }
                
                // Правило 3: Противники не могут толкать друг друга
                // Если противник стоит, его обязательно обходим (уже обработано выше)
                // Если противник движется, используем steering behaviors
                
                // Проверяем, не движемся ли мы навстречу друг другу
                if (nearbyDrone is MonoBehaviour nearbyMono)
                {
                    Rigidbody nearbyRb = nearbyMono.GetComponent<Rigidbody>();
                    if (nearbyRb != null)
                    {
                        Vector3 relativeVelocity = velocity - nearbyRb.linearVelocity;
                        Vector3 toOther = (nearbyDrone.Position - position).normalized;
                        
                        // Если движемся навстречу друг другу или очень близко
                        bool isApproaching = Vector3.Dot(relativeVelocity.normalized, toOther) < -0.5f && distance < _collisionDetectionRadius;
                        bool isVeryClose = distance < _collisionDetectionRadius * 0.5f;
                        
                        if (isApproaching || isVeryClose)
                        {
                            // Используем PriorityCalculator для определения приоритета
                            bool shouldYieldToNearby = PriorityCalculator.ShouldYield(
                                _drone, velocity,
                                nearbyDrone, nearbyVelocity);
                            
                            if (shouldYieldToNearby)
                            {
                                // Регистрируем столкновение для совместного разрешения
                                if (_collisionResolver != null)
                                {
                                    _collisionResolver.ResolveCollision(_drone, velocity, nearbyDrone, nearbyVelocity);
                                }
                                
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    blockingDrone = nearbyDrone;
                                    shouldYield = true;
                                }
                            }
                        }
                    }
                }
                
                // Также проверяем застревание
                if (_stuckTimer > _stuckDetectionTime && distance < _stuckDistance)
                {
                    // При застревании используем PriorityCalculator
                    bool shouldYieldToNearby = PriorityCalculator.ShouldYield(
                        _drone, velocity,
                        nearbyDrone, nearbyVelocity);
                    
                    if (shouldYieldToNearby)
                    {
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            blockingDrone = nearbyDrone;
                            shouldYield = true;
                        }
                    }
                }
            }

            if (blockingDrone != null && shouldYield)
            {
                _blockingDrone = blockingDrone;
                
                // Вычисляем направление обхода
                Vector3 toBlocking = (blockingDrone.Position - position).normalized;
                Vector3 perpendicular = Vector3.Cross(toBlocking, Vector3.up).normalized;
                
                // Выбираем направление обхода (вправо или влево) случайно, но стабильно
                float direction = (blockingDrone.Id % 2 == 0) ? 1f : -1f;
                Vector3 avoidanceDirection = perpendicular * direction;
                
                // Если перпендикулярное направление не подходит, используем отступление
                if (avoidanceDirection.magnitude < 0.1f)
                {
                    avoidanceDirection = -toBlocking;
                }
                
                // Применяем силу обхода
                return avoidanceDirection.normalized * _avoidanceStrength * Weight;
            }

            _blockingDrone = null;
            return Vector3.zero;
        }

        public IDrone GetBlockingDrone()
        {
            return _blockingDrone;
        }
    }
}

