using UnityEngine;

namespace Systems.Spawning
{
    /// <summary>
    /// Провайдер точек спавна для ресурсов
    /// </summary>
    public class SpawnPointProvider
    {
        private readonly Bounds _spawnBounds;
        private readonly LayerMask _navMeshLayer;

        private const int Attempts = 10;

        public SpawnPointProvider(Bounds spawnBounds, LayerMask navMeshLayer)
        {
            _spawnBounds = spawnBounds;
            _navMeshLayer = navMeshLayer;
        }

        /// <summary>
        /// Получить случайную точку спавна на NavMesh
        /// </summary>
        public Vector3 GetRandomSpawnPoint()
        {
            for (var i = 0; i < Attempts; i++)
            {
                var randomPoint = new Vector3(
                    Random.Range(_spawnBounds.min.x, _spawnBounds.max.x),
                    Random.Range(_spawnBounds.min.y, _spawnBounds.max.y),
                    Random.Range(_spawnBounds.min.z, _spawnBounds.max.z)
                );

                if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out var hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }

            // Если не удалось найти точку на NavMesh, возвращаем центр области
            return _spawnBounds.center;
        }
    }
}


