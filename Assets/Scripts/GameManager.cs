using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Base baseObject;
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] private int resourceCount = 15;
    [SerializeField] private float mapSize = 40f;
    [SerializeField] private UIManager uiManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SpawnResources();
        UpdateUI();
    }

    private void SpawnResources()
    {
        if (resourcePrefab == null)
            return;

        for (int i = 0; i < resourceCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * mapSize;
            Vector3 spawnPosition = new Vector3(randomCircle.x, 0.5f, randomCircle.y);
            
            // Avoid spawning too close to base
            if (baseObject != null && Vector3.Distance(spawnPosition, baseObject.Position) < 5f)
            {
                i--;
                continue;
            }

            Instantiate(resourcePrefab, spawnPosition, Quaternion.identity);
        }
    }

    public void UpdateUI()
    {
        if (uiManager != null && baseObject != null)
        {
            uiManager.UpdateResourceCount(baseObject.TotalResources);
            uiManager.UpdateDroneCount(baseObject.DroneCount);
        }
    }

    public Base GetBase()
    {
        return baseObject;
    }
}
