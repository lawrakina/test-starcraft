using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Entities.Base
{
    /// <summary>
    /// Компонент базы фракции
    /// Реализует интерфейс IBase
    /// </summary>
    [RequireComponent(typeof(BaseResourceCounter))]
    [RequireComponent(typeof(BaseVisuals))]
    [RequireComponent(typeof(BaseUnloadTrigger))]
    public class Base : MonoBehaviour, IBase
    {
        [SerializeField] private FactionType faction;
        [SerializeField] private Transform unloadPoint;
        [SerializeField] private Transform[] spawnPoints;

        private BaseResourceCounter _resourceCounter;
        private BaseUnloadTrigger _unloadTrigger;

        public FactionType Faction => faction;
        public Vector3 Position => transform.position;
        public Vector3 UnloadPoint => unloadPoint != null ? unloadPoint.position : transform.position;
        public int ResourceCount => _resourceCounter != null ? _resourceCounter.Count : 0;

        private void Awake()
        {
            Debug.Log($"[Base] Awake() called on {gameObject.name}, Faction: {faction}");
            
            _resourceCounter = GetComponent<BaseResourceCounter>();
            if (!_resourceCounter)
            {
                Debug.LogError($"[Base] BaseResourceCounter not found on {gameObject.name}!");
            }

            if (unloadPoint == null)
            {
                unloadPoint = transform;
                Debug.Log($"[Base] UnloadPoint not set, using transform position on {gameObject.name}");
            }
            else
            {
                Debug.Log($"[Base] UnloadPoint set to {unloadPoint.position} on {gameObject.name}");
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.Log($"[Base] Creating default spawn points for {gameObject.name}");
                CreateDefaultSpawnPoints();
            }
            else
            {
                Debug.Log($"[Base] Using {spawnPoints.Length} spawn points for {gameObject.name}");
            }
            
            // Убеждаемся, что триггер для сдачи ресурсов настроен
            Debug.Log($"[Base] Setting up unload trigger for {gameObject.name}");
            SetupUnloadTrigger();
            
            // Сканируем объект базы и находим триггер, связываем его с базой
            Debug.Log($"[Base] Scanning and linking unload trigger for {gameObject.name}");
            ScanAndLinkUnloadTrigger();
            
            Debug.Log($"[Base] Base {gameObject.name} initialization complete. UnloadTrigger: {_unloadTrigger != null}");
        }
        
        /// <summary>
        /// Настраивает триггер для сдачи ресурсов
        /// </summary>
        private void SetupUnloadTrigger()
        {
            Debug.Log($"[Base] SetupUnloadTrigger() called on {gameObject.name}");
            
            BaseUnloadTrigger trigger = GetComponent<BaseUnloadTrigger>();
            if (trigger == null)
            {
                Debug.Log($"[Base] BaseUnloadTrigger not found, creating new one on {gameObject.name}");
                trigger = gameObject.AddComponent<BaseUnloadTrigger>();
            }
            else
            {
                Debug.Log($"[Base] BaseUnloadTrigger found on {gameObject.name}");
            }
            
            // Настраиваем коллайдер как триггер
            Collider collider = GetComponent<Collider>();
            if (collider == null)
            {
                // Если коллайдера нет, создаем SphereCollider как триггер
                Debug.Log($"[Base] No collider found, creating SphereCollider trigger on {gameObject.name}");
                SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
                sphereCollider.radius = 2f; // Радиус 2 единицы для сдачи ресурсов
                Debug.Log($"[Base] Created SphereCollider trigger with radius 2f on {gameObject.name}");
            }
            else if (!collider.isTrigger)
            {
                // Если коллайдер есть, но не триггер, создаем отдельный дочерний объект с триггером
                Debug.Log($"[Base] Collider exists but is not a trigger, creating child trigger object on {gameObject.name}");
                GameObject triggerObject = new GameObject("UnloadTrigger");
                triggerObject.transform.SetParent(transform);
                triggerObject.transform.localPosition = Vector3.zero;
                
                SphereCollider triggerCollider = triggerObject.AddComponent<SphereCollider>();
                triggerCollider.isTrigger = true;
                triggerCollider.radius = 2f;
                
                // Добавляем компонент триггера на дочерний объект
                BaseUnloadTrigger triggerComponent = triggerObject.AddComponent<BaseUnloadTrigger>();
                Debug.Log($"[Base] Created child UnloadTrigger object with BaseUnloadTrigger component on {gameObject.name}");
            }
            else
            {
                // Коллайдер уже триггер - убеждаемся, что он настроен правильно
                Debug.Log($"[Base] Collider is already a trigger on {gameObject.name}");
                if (collider is SphereCollider sphereCollider)
                {
                    sphereCollider.radius = 2f;
                    Debug.Log($"[Base] Set SphereCollider radius to 2f on {gameObject.name}");
                }
            }
        }
        
        /// <summary>
        /// Сканирует объект базы и дочерние объекты, находит все триггеры и связывает их с базой
        /// </summary>
        private void ScanAndLinkUnloadTrigger()
        {
            // Ищем триггер на самом объекте базы
            _unloadTrigger = GetComponent<BaseUnloadTrigger>();
            
            // Если не нашли на самом объекте, ищем в дочерних объектах
            if (!_unloadTrigger)
            {
                _unloadTrigger = GetComponentInChildren<BaseUnloadTrigger>();
            }
            
            // Если все еще не нашли, ищем во всех дочерних объектах рекурсивно
            if (!_unloadTrigger)
            {
                _unloadTrigger = FindUnloadTriggerInChildren(transform);
            }
            
            // Если нашли триггер, связываем его с базой
            if (_unloadTrigger)
            {
                Debug.Log($"[Base] Found UnloadTrigger on {gameObject.name}, linking to base");
                _unloadTrigger.SetBaseComponent(this);
                Debug.Log($"[Base] Found and linked UnloadTrigger for {gameObject.name}. Trigger enabled: {_unloadTrigger.enabled}, GameObject active: {_unloadTrigger.gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning($"[Base] UnloadTrigger not found for {gameObject.name}, creating new one");
                // Создаем новый триггер, если не нашли
                SetupUnloadTrigger();
                _unloadTrigger = GetComponent<BaseUnloadTrigger>();
                if (!_unloadTrigger)
                {
                    _unloadTrigger = GetComponentInChildren<BaseUnloadTrigger>();
                }
                if (_unloadTrigger)
                {
                    Debug.Log($"[Base] Linking newly created UnloadTrigger to base on {gameObject.name}");
                    _unloadTrigger.SetBaseComponent(this);
                    Debug.Log($"[Base] New UnloadTrigger linked. Trigger enabled: {_unloadTrigger.enabled}, GameObject active: {_unloadTrigger.gameObject.activeInHierarchy}");
                }
                else
                {
                    Debug.LogError($"[Base] Failed to create or find UnloadTrigger for {gameObject.name}!");
                }
            }
        }
        
        /// <summary>
        /// Рекурсивно ищет BaseUnloadTrigger в дочерних объектах
        /// </summary>
        private BaseUnloadTrigger FindUnloadTriggerInChildren(Transform parent)
        {
            foreach (Transform child in parent)
            {
                BaseUnloadTrigger trigger = child.GetComponent<BaseUnloadTrigger>();
                if (trigger)
                {
                    return trigger;
                }
                
                // Рекурсивно ищем в дочерних объектах
                BaseUnloadTrigger found = FindUnloadTriggerInChildren(child);
                if (found)
                {
                    return found;
                }
            }
            
            return null;
        }

        public void AddResource()
        {
            if (_resourceCounter)
            {
                _resourceCounter.AddResource();
            }
        }

        public Vector3 GetSpawnPoint(int index)
        {
            if (spawnPoints != null && index >= 0 && index < spawnPoints.Length)
            {
                return spawnPoints[index].position;
            }

            return transform.position + Vector3.right * (index * 2f);
        }

        private void CreateDefaultSpawnPoints()
        {
            spawnPoints = new Transform[5];
            for (int i = 0; i < 5; i++)
            {
                GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
                spawnPoint.transform.SetParent(transform);
                float angle = (i - 2) * 15f; // От -30 до +30 градусов
                spawnPoint.transform.localPosition = Quaternion.Euler(0, angle, 0) * Vector3.forward * 3f;
                spawnPoints[i] = spawnPoint.transform;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(UnloadPoint, 0.5f);

            if (spawnPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (var spawnPoint in spawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
                    }
                }
            }
        }
    }
}