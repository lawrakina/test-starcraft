using UnityEngine;

/// <summary>
/// Helper script to spawn resources in specific patterns or at runtime
/// </summary>
public class ResourceSpawner : MonoBehaviour
{
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] private int resourceCount = 15;
    [SerializeField] private float spawnRadius = 40f;
    [SerializeField] private float minDistanceFromBase = 5f;
    [SerializeField] private Transform baseTransform;
    [SerializeField] private bool spawnOnStart = true;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnResources();
        }
    }

    public void SpawnResources()
    {
        if (resourcePrefab == null)
        {
            Debug.LogWarning("Resource prefab is not assigned!");
            return;
        }

        for (int i = 0; i < resourceCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            
            if (spawnPosition != Vector3.zero)
            {
                GameObject resource = Instantiate(resourcePrefab, spawnPosition, Quaternion.identity);
                resource.transform.parent = transform;
            }
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        int maxAttempts = 30;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 position = new Vector3(randomCircle.x, 0.5f, randomCircle.y);
            
            // Check distance from base
            if (baseTransform != null)
            {
                float distanceFromBase = Vector3.Distance(position, baseTransform.position);
                if (distanceFromBase < minDistanceFromBase)
                    continue;
            }

            return position;
        }
        
        Debug.LogWarning("Could not find valid spawn position after " + maxAttempts + " attempts");
        return Vector3.zero;
    }

    public void ClearResources()
    {
        Resource[] resources = GetComponentsInChildren<Resource>();
        foreach (Resource resource in resources)
        {
            Destroy(resource.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        if (baseTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(baseTransform.position, minDistanceFromBase);
        }
    }
}
