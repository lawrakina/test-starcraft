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
    public class Base : MonoBehaviour, IBase
    {
        [SerializeField] private FactionType faction;
        [SerializeField] private Transform unloadPoint;
        [SerializeField] private Transform[] spawnPoints;

        private BaseResourceCounter _resourceCounter;

        public FactionType Faction => faction;
        public Vector3 Position => transform.position;
        public Vector3 UnloadPoint => unloadPoint != null ? unloadPoint.position : transform.position;
        public int ResourceCount => _resourceCounter != null ? _resourceCounter.Count : 0;

        private void Awake()
        {
            _resourceCounter = GetComponent<BaseResourceCounter>();

            if (unloadPoint == null)
            {
                unloadPoint = transform;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                CreateDefaultSpawnPoints();
            }
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