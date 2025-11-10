using UnityEngine;
using UnityEngine.AI;

public class Drone : MonoBehaviour
{
    public enum DroneState
    {
        Idle,
        SearchResource,
        MoveToResource,
        CollectResource,
        ReturnToBase
    }

    [SerializeField] private float searchRadius = 20f;
    [SerializeField] private float collectRange = 1f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private int carryCapacity = 5;

    private DroneState currentState = DroneState.Idle;
    private Base homeBase;
    private Resource targetResource;
    private int carriedResources = 0;
    private NavMeshAgent agent;
    private float stateTimer = 0f;

    public DroneState CurrentState => currentState;
    public int CarriedResources => carriedResources;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }
    }

    public void Initialize(Base baseRef)
    {
        homeBase = baseRef;
        ChangeState(DroneState.SearchResource);
    }

    private void Update()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case DroneState.Idle:
                UpdateIdle();
                break;
            case DroneState.SearchResource:
                UpdateSearchResource();
                break;
            case DroneState.MoveToResource:
                UpdateMoveToResource();
                break;
            case DroneState.CollectResource:
                UpdateCollectResource();
                break;
            case DroneState.ReturnToBase:
                UpdateReturnToBase();
                break;
        }
    }

    private void ChangeState(DroneState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    private void UpdateIdle()
    {
        if (stateTimer > 1f)
        {
            ChangeState(DroneState.SearchResource);
        }
    }

    private void UpdateSearchResource()
    {
        Resource nearestResource = FindNearestResource();
        
        if (nearestResource != null)
        {
            targetResource = nearestResource;
            ChangeState(DroneState.MoveToResource);
        }
        else if (stateTimer > 2f)
        {
            ChangeState(DroneState.Idle);
        }
    }

    private void UpdateMoveToResource()
    {
        if (targetResource == null || !targetResource.IsAvailable)
        {
            targetResource = null;
            ChangeState(DroneState.SearchResource);
            return;
        }

        MoveToPosition(targetResource.Position);

        if (Vector3.Distance(transform.position, targetResource.Position) < collectRange)
        {
            ChangeState(DroneState.CollectResource);
        }
    }

    private void UpdateCollectResource()
    {
        if (targetResource == null || !targetResource.IsAvailable)
        {
            targetResource = null;
            if (carriedResources > 0)
            {
                ChangeState(DroneState.ReturnToBase);
            }
            else
            {
                ChangeState(DroneState.SearchResource);
            }
            return;
        }

        if (stateTimer > 0.5f && carriedResources < carryCapacity)
        {
            if (targetResource.Collect(1))
            {
                carriedResources++;
                stateTimer = 0f;
            }
        }

        if (carriedResources >= carryCapacity || !targetResource.IsAvailable)
        {
            ChangeState(DroneState.ReturnToBase);
        }
    }

    private void UpdateReturnToBase()
    {
        if (homeBase == null)
            return;

        MoveToPosition(homeBase.Position);

        if (Vector3.Distance(transform.position, homeBase.Position) < 2f)
        {
            DepositResources();
            ChangeState(DroneState.SearchResource);
        }
    }

    private void DepositResources()
    {
        if (homeBase != null && carriedResources > 0)
        {
            homeBase.DepositResource(carriedResources);
            carriedResources = 0;
        }
    }

    private Resource FindNearestResource()
    {
        Resource[] allResources = FindObjectsOfType<Resource>();
        Resource nearest = null;
        float minDistance = float.MaxValue;

        foreach (Resource resource in allResources)
        {
            if (!resource.IsAvailable)
                continue;

            float distance = Vector3.Distance(transform.position, resource.Position);
            if (distance < searchRadius && distance < minDistance)
            {
                minDistance = distance;
                nearest = resource;
            }
        }

        return nearest;
    }

    private void MoveToPosition(Vector3 position)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(position);
        }
        else
        {
            // Fallback simple movement
            Vector3 direction = (position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        if (homeBase != null)
        {
            homeBase.RemoveDrone(this);
        }
    }
}
