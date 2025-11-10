using UnityEngine;
using System.Collections.Generic;

public class Base : MonoBehaviour
{
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private float buildCooldown = 3f;
    [SerializeField] private int initialDrones = 3;
    [SerializeField] private float spawnRadius = 2f;
    
    private int totalResources = 0;
    private List<Drone> drones = new List<Drone>();
    private float lastBuildTime;

    public Vector3 Position => transform.position;
    public int TotalResources => totalResources;
    public int DroneCount => drones.Count;

    private void Start()
    {
        for (int i = 0; i < initialDrones; i++)
        {
            BuildDrone();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && CanBuildDrone())
        {
            BuildDrone();
        }
    }

    public void DepositResource(int amount)
    {
        totalResources += amount;
        GameManager.Instance?.UpdateUI();
    }

    private bool CanBuildDrone()
    {
        return Time.time - lastBuildTime >= buildCooldown;
    }

    private void BuildDrone()
    {
        if (dronePrefab == null)
            return;

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);
        
        GameObject droneObj = Instantiate(dronePrefab, spawnPosition, Quaternion.identity);
        Drone drone = droneObj.GetComponent<Drone>();
        
        if (drone != null)
        {
            drone.Initialize(this);
            drones.Add(drone);
        }

        lastBuildTime = Time.time;
        GameManager.Instance?.UpdateUI();
    }

    public void RemoveDrone(Drone drone)
    {
        drones.Remove(drone);
        GameManager.Instance?.UpdateUI();
    }
}
