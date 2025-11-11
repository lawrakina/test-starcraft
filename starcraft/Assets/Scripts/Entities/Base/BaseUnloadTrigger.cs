using System.Collections.Generic;
using Core.Enums;
using Core.Interfaces;
using DroneResourceCollection.Entities.Drone;
using Entities.Drone;
using UnityEngine;

namespace Entities.Base
{
    /// <summary>
    /// Компонент триггера для сдачи ресурсов на базе
    /// Обрабатывает вход дронов в состоянии Returning и вызывает сдачу ресурсов
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BaseUnloadTrigger : MonoBehaviour
    {
        [SerializeField] private Base baseComponent;
        
        // Защита от множественных сдач ресурсов одним дроном
        private HashSet<int> _processedDrones = new HashSet<int>();
        
        // Для периодического логирования статуса триггера
        private float _lastStatusLogTime = 0f;
        private const float StatusLogInterval = 5f; // Логируем каждые 5 секунд
        
        /// <summary>
        /// Устанавливает ссылку на компонент базы
        /// Вызывается из Base при сканировании
        /// </summary>
        public void SetBaseComponent(Base baseComp)
        {
            Debug.Log($"[BaseUnloadTrigger] SetBaseComponent() called on {gameObject.name}. Base: {baseComp != null}");
            if (baseComp)
            {
                Debug.Log($"[BaseUnloadTrigger] Base component set: {baseComp.gameObject.name}, Faction: {baseComp.Faction}");
            }
            baseComponent = baseComp;
        }
        
        private void Awake()
        {
            Debug.Log($"[BaseUnloadTrigger] Awake() called on {gameObject.name}");
            
            // Получаем компонент базы, если не установлен
            if (!baseComponent)
            {
                baseComponent = GetComponentInParent<Base>();
            }
            
            // Если все еще не нашли, пытаемся найти на том же объекте
            if (!baseComponent)
            {
                baseComponent = GetComponent<Base>();
            }
            
            if (baseComponent)
            {
                Debug.Log($"[BaseUnloadTrigger] Base component found in Awake: {baseComponent.gameObject.name}, Faction: {baseComponent.Faction}");
            }
            else
            {
                Debug.LogWarning($"[BaseUnloadTrigger] Base component not found in Awake on {gameObject.name}");
            }
            
            // Настраиваем коллайдер как триггер
            Collider collider = GetComponent<Collider>();
            if (collider)
            {
                collider.isTrigger = true;
                Debug.Log($"[BaseUnloadTrigger] Collider found and set as trigger on {gameObject.name}. Type: {collider.GetType().Name}, IsTrigger: {collider.isTrigger}");
                if (collider is SphereCollider sphereCollider)
                {
                    Debug.Log($"[BaseUnloadTrigger] SphereCollider radius: {sphereCollider.radius}");
                }
            }
            else
            {
                // Создаем коллайдер, если его нет
                collider = gameObject.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                ((SphereCollider)collider).radius = 2f; // Радиус 2 единицы
                Debug.Log($"[BaseUnloadTrigger] Created new SphereCollider trigger with radius 2f on {gameObject.name}");
            }
        }
        
        private void OnEnable()
        {
            Debug.Log($"[BaseUnloadTrigger] OnEnable() called on {gameObject.name}. Base component: {baseComponent != null}, Processed drones count: {_processedDrones.Count}");
        }
        
        private void OnDisable()
        {
            Debug.LogWarning($"[BaseUnloadTrigger] OnDisable() called on {gameObject.name}! Trigger is now disabled!");
        }
        
        private void Update()
        {
            // Периодическое логирование статуса триггера
            if (Time.time - _lastStatusLogTime >= StatusLogInterval)
            {
                _lastStatusLogTime = Time.time;
                bool hasBase = baseComponent != null;
                bool isEnabled = enabled && gameObject.activeInHierarchy;
                Collider collider = GetComponent<Collider>();
                bool hasCollider = collider != null;
                bool isTrigger = collider != null && collider.isTrigger;
                
                Debug.Log($"[BaseUnloadTrigger] Status check on {gameObject.name}: " +
                         $"HasBase={hasBase}, IsEnabled={isEnabled}, HasCollider={hasCollider}, " +
                         $"IsTrigger={isTrigger}, ProcessedDrones={_processedDrones.Count}");
                
                if (!hasBase)
                {
                    Debug.LogWarning($"[BaseUnloadTrigger] WARNING: Base component is null on {gameObject.name}!");
                }
                if (!isEnabled)
                {
                    Debug.LogWarning($"[BaseUnloadTrigger] WARNING: Trigger is disabled on {gameObject.name}!");
                }
                if (!hasCollider || !isTrigger)
                {
                    Debug.LogWarning($"[BaseUnloadTrigger] WARNING: Collider issue on {gameObject.name}! HasCollider={hasCollider}, IsTrigger={isTrigger}");
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[BaseUnloadTrigger] OnTriggerEnter called with: {other.gameObject.name}");
            
            // Если база не установлена, пытаемся найти ее снова
            if (!baseComponent)
            {
                baseComponent = GetComponentInParent<Base>();
                if (!baseComponent)
                {
                    baseComponent = GetComponent<Base>();
                }
            }
            
            // Если база все еще не найдена, выходим
            if (!baseComponent)
            {
                Debug.LogWarning("[BaseUnloadTrigger] Base component not found!");
                return;
            }
            
            Debug.Log($"[BaseUnloadTrigger] Base found: {baseComponent.gameObject.name}, Faction: {baseComponent.Faction}");
            
            // Проверяем, что это дрон - используем правильный тип из Entities.Drone
            // Пробуем несколько способов получения компонента
            Entities.Drone.Drone drone = other.GetComponent<Entities.Drone.Drone>();
            if (!drone)
            {
                // Пробуем получить через родительский объект (если коллайдер на дочернем объекте)
                drone = other.GetComponentInParent<Entities.Drone.Drone>();
            }
            if (!drone)
            {
                // Пробуем получить через GameObject
                drone = other.gameObject.GetComponent<Entities.Drone.Drone>();
            }
            if (!drone)
            {
                Debug.Log($"[BaseUnloadTrigger] No Drone component found on {other.gameObject.name}. Trying to find in parent...");
                // Последняя попытка - ищем в родительских объектах
                Transform parent = other.transform.parent;
                while (parent != null && !drone)
                {
                    drone = parent.GetComponent<Entities.Drone.Drone>();
                    parent = parent.parent;
                }
            }
            if (!drone)
            {
                Debug.LogWarning($"[BaseUnloadTrigger] No Drone component found on {other.gameObject.name} or its parents");
                return;
            }
            
            // Проверяем наличие ресурса (TargetResource или прикрепленный)
            bool hasTargetResource = drone.TargetResource != null;
            bool hasAttachedResource = false;
            DroneResourceCollection.Entities.Resource.Resource attachedResource = null;
            
            if (drone is MonoBehaviour droneMono)
            {
                // Ищем дочерние объекты с компонентом Resource
                attachedResource = droneMono.GetComponentInChildren<DroneResourceCollection.Entities.Resource.Resource>();
                hasAttachedResource = attachedResource != null;
            }
            
            Debug.Log($"[BaseUnloadTrigger] Drone found: ID={drone.Id}, Faction={drone.Faction}, State={drone.CurrentState}, HasTargetResource={hasTargetResource}, HasAttachedResource={hasAttachedResource}");
            
            // Проверяем, что дрон принадлежит этой базе
            if (drone.Faction != baseComponent.Faction)
            {
                Debug.Log($"[BaseUnloadTrigger] Drone {drone.Id} faction mismatch: drone={drone.Faction}, base={baseComponent.Faction}");
                return;
            }
            
            // Проверяем, что дрон в состоянии Returning (несет ресурсы)
            if (drone.CurrentState != DroneState.Returning)
            {
                Debug.Log($"[BaseUnloadTrigger] Drone {drone.Id} is not in Returning state (current: {drone.CurrentState})");
                return;
            }
            
            // Проверяем, что у дрона есть ресурс для сдачи
            // Ресурс может быть либо в TargetResource, либо прикреплен как дочерний объект
            bool hasResource = hasTargetResource || hasAttachedResource;
            
            if (!hasResource)
            {
                Debug.Log($"[BaseUnloadTrigger] Drone {drone.Id} has no target resource and no attached resource");
                return;
            }
            
            // Защита от множественных сдач - проверяем, не обрабатывали ли мы уже этого дрона
            if (_processedDrones.Contains(drone.Id))
            {
                Debug.Log($"[BaseUnloadTrigger] Drone {drone.Id} already processed");
                return;
            }
            
            // Вызываем сдачу ресурсов
            Debug.Log($"[BaseUnloadTrigger] All checks passed! Unloading resources from drone {drone.Id}");
            UnloadResources(drone);
        }
        
        private void OnTriggerExit(Collider other)
        {
            // Когда дрон выходит из триггера, удаляем его из списка обработанных
            Entities.Drone.Drone drone = other.GetComponent<Entities.Drone.Drone>();
            if (!drone)
            {
                drone = other.GetComponentInParent<Entities.Drone.Drone>();
            }
            
            if (drone)
            {
                bool wasProcessed = _processedDrones.Contains(drone.Id);
                _processedDrones.Remove(drone.Id);
                Debug.Log($"[BaseUnloadTrigger] Drone {drone.Id} exited trigger. Was processed: {wasProcessed}, Removed from processed list. Remaining: {_processedDrones.Count}");
            }
        }
        
        /// <summary>
        /// Выполняет сдачу ресурсов дроном на базе
        /// </summary>
        private void UnloadResources(Entities.Drone.Drone drone)
        {
            Debug.Log($"[BaseUnloadTrigger] ===== UNLOAD RESOURCES START for drone {drone.Id} =====");
            
            if (!baseComponent || !drone)
            {
                Debug.LogError($"[BaseUnloadTrigger] Cannot unload: baseComponent={baseComponent != null}, drone={drone != null}");
                return;
            }
            
            Debug.Log($"[BaseUnloadTrigger] Drone {drone.Id} state before unload: {drone.CurrentState}");
            
            // Добавляем дрона в список обработанных, чтобы избежать повторной сдачи
            _processedDrones.Add(drone.Id);
            Debug.Log($"[BaseUnloadTrigger] Drone {drone.Id} added to processed list. Total processed: {_processedDrones.Count}");
            
            // Добавляем ресурс на базу
            int resourcesBefore = baseComponent.ResourceCount;
            baseComponent.AddResource();
            Debug.Log($"[BaseUnloadTrigger] Resource added to base. Before: {resourcesBefore}, After: {baseComponent.ResourceCount}");
            
            // Находим ресурс для уничтожения ДО очистки TargetResource
            // Сначала проверяем TargetResource
            DroneResourceCollection.Entities.Resource.Resource resourceToDestroy = null;
            if (drone.TargetResource != null)
            {
                Debug.Log($"[BaseUnloadTrigger] Drone {drone.Id} has TargetResource: {drone.TargetResource.Id}");
                if (drone.TargetResource is DroneResourceCollection.Entities.Resource.Resource resourceComponent)
                {
                    // Проверяем, что ресурс не уничтожен
                    if (resourceComponent)
                    {
                        resourceToDestroy = resourceComponent;
                        Debug.Log($"[BaseUnloadTrigger] Found TargetResource to destroy: {resourceToDestroy.Id}");
                    }
                    else
                    {
                        Debug.LogWarning($"[BaseUnloadTrigger] TargetResource component is destroyed for drone {drone.Id}");
                    }
                }
            }
            else
            {
                Debug.Log($"[BaseUnloadTrigger] Drone {drone.Id} has no TargetResource, searching for attached resource");
            }
            
            // Если TargetResource null или не найден, ищем прикрепленный ресурс на дроне
            if (!resourceToDestroy && drone is MonoBehaviour droneMono)
            {
                resourceToDestroy = droneMono.GetComponentInChildren<DroneResourceCollection.Entities.Resource.Resource>();
                if (resourceToDestroy)
                {
                    Debug.Log($"[BaseUnloadTrigger] Found attached resource on drone {drone.Id}: {resourceToDestroy.Id}");
                }
                else
                {
                    Debug.LogWarning($"[BaseUnloadTrigger] No attached resource found on drone {drone.Id}");
                }
            }
            
            // Очищаем целевой ресурс у дрона ПЕРЕД уничтожением объекта
            // Это важно, чтобы избежать обращения к уничтоженному объекту
            Debug.Log($"[BaseUnloadTrigger] Clearing TargetResource for drone {drone.Id}");
            drone.ClearTargetResource();
            Debug.Log($"[BaseUnloadTrigger] TargetResource cleared. Current TargetResource: {drone.TargetResource != null}");
            
            // Теперь уничтожаем ресурс
            if (resourceToDestroy)
            {
                Debug.Log($"[BaseUnloadTrigger] Destroying resource {resourceToDestroy.Id} for drone {drone.Id}");
                resourceToDestroy.DestroyAfterUnload();
                Debug.Log($"[BaseUnloadTrigger] Resource {resourceToDestroy.Id} destroyed successfully");
            }
            else
            {
                Debug.LogWarning($"[BaseUnloadTrigger] No resource found to destroy for drone {drone.Id}");
            }
            
            // Воспроизводим визуальный эффект выгрузки
            DroneVisuals visuals = drone.GetComponent<DroneVisuals>();
            if (visuals)
            {
                Debug.Log($"[BaseUnloadTrigger] Playing unload effect for drone {drone.Id}");
                visuals.PlayUnloadEffect();
            }
            else
            {
                Debug.LogWarning($"[BaseUnloadTrigger] DroneVisuals not found for drone {drone.Id}");
            }
            
            // Переключаем дрон на состояние Searching через стейтмашину
            Debug.Log($"[BaseUnloadTrigger] Attempting to switch drone {drone.Id} to Searching state");
            DroneStateMachine stateMachine = drone.GetComponent<DroneStateMachine>();
            if (stateMachine)
            {
                Debug.Log($"[BaseUnloadTrigger] StateMachine found for drone {drone.Id}. Current state: {drone.CurrentState}");
                stateMachine.ForceSearchingState();
                Debug.Log($"[BaseUnloadTrigger] ForceSearchingState() called for drone {drone.Id}. New state: {drone.CurrentState}");
            }
            else
            {
                Debug.LogError($"[BaseUnloadTrigger] DroneStateMachine not found for drone {drone.Id}!");
            }
            
            Debug.Log($"[BaseUnloadTrigger] ===== UNLOAD RESOURCES END for drone {drone.Id} =====");
        }
    }
}

