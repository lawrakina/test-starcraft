using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Entities.Base
{
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
            _resourceCounter = GetComponent<BaseResourceCounter>();
            if (!_resourceCounter)
            {
                Debug.LogError($"[Base] BaseResourceCounter not found on {gameObject.name}!");
            }

            if (unloadPoint == null)
            {
                unloadPoint = transform;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                CreateDefaultSpawnPoints();
            }
            
            SetupUnloadTrigger();
            ScanAndLinkUnloadTrigger();
        }
        
        private void SetupUnloadTrigger()
        {
            BaseUnloadTrigger trigger = GetComponent<BaseUnloadTrigger>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<BaseUnloadTrigger>();
            }
            
            Collider collider = GetComponent<Collider>();
            if (collider == null)
            {
                SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
                sphereCollider.radius = 2f;
            }
            else if (!collider.isTrigger)
            {
                GameObject triggerObject = new GameObject("UnloadTrigger");
                triggerObject.transform.SetParent(transform);
                triggerObject.transform.localPosition = Vector3.zero;
                
                SphereCollider triggerCollider = triggerObject.AddComponent<SphereCollider>();
                triggerCollider.isTrigger = true;
                triggerCollider.radius = 2f;
                
                BaseUnloadTrigger triggerComponent = triggerObject.AddComponent<BaseUnloadTrigger>();
            }
            else
            {
                if (collider is SphereCollider sphereCollider)
                {
                    sphereCollider.radius = 2f;
                }
            }
        }
        
        private void ScanAndLinkUnloadTrigger()
        {
            _unloadTrigger = GetComponent<BaseUnloadTrigger>();
            
            if (!_unloadTrigger)
            {
                _unloadTrigger = GetComponentInChildren<BaseUnloadTrigger>();
            }
            
            if (!_unloadTrigger)
            {
                _unloadTrigger = FindUnloadTriggerInChildren(transform);
            }
            
            if (_unloadTrigger)
            {
                _unloadTrigger.SetBaseComponent(this);
            }
            else
            {
                Debug.LogWarning($"[Base] UnloadTrigger not found for {gameObject.name}, creating new one");
                SetupUnloadTrigger();
                _unloadTrigger = GetComponent<BaseUnloadTrigger>();
                if (!_unloadTrigger)
                {
                    _unloadTrigger = GetComponentInChildren<BaseUnloadTrigger>();
                }
                if (_unloadTrigger)
                {
                    _unloadTrigger.SetBaseComponent(this);
                }
                else
                {
                    Debug.LogError($"[Base] Failed to create or find UnloadTrigger for {gameObject.name}!");
                }
            }
        }
        
        private BaseUnloadTrigger FindUnloadTriggerInChildren(Transform parent)
        {
            foreach (Transform child in parent)
            {
                BaseUnloadTrigger trigger = child.GetComponent<BaseUnloadTrigger>();
                if (trigger)
                {
                    return trigger;
                }
                
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
                float angle = (i - 2) * 15f;
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