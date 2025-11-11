using System.Collections.Generic;
using Core.Enums;
using Core.Interfaces;
using DroneResourceCollection.Entities.Drone;
using Entities.Drone;
using UnityEngine;

namespace Entities.Base
{
    [RequireComponent(typeof(Collider))]
    public class BaseUnloadTrigger : MonoBehaviour
    {
        [SerializeField] private Base baseComponent;
        
        private HashSet<int> _processedDrones = new HashSet<int>();
        
        public void SetBaseComponent(Base baseComp)
        {
            baseComponent = baseComp;
        }
        
        private void Awake()
        {
            if (!baseComponent)
            {
                baseComponent = GetComponentInParent<Base>();
            }
            
            if (!baseComponent)
            {
                baseComponent = GetComponent<Base>();
            }
            
            if (!baseComponent)
            {
                Debug.LogWarning($"[BaseUnloadTrigger] Base component not found in Awake on {gameObject.name}");
            }
            
            Collider collider = GetComponent<Collider>();
            if (collider)
            {
                collider.isTrigger = true;
            }
            else
            {
                collider = gameObject.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                ((SphereCollider)collider).radius = 2f;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!baseComponent)
            {
                baseComponent = GetComponentInParent<Base>();
                if (!baseComponent)
                {
                    baseComponent = GetComponent<Base>();
                }
            }
            
            if (!baseComponent)
            {
                Debug.LogWarning("[BaseUnloadTrigger] Base component not found!");
                return;
            }
            
            Entities.Drone.Drone drone = other.GetComponent<Entities.Drone.Drone>();
            if (!drone)
            {
                drone = other.GetComponentInParent<Entities.Drone.Drone>();
            }
            if (!drone)
            {
                drone = other.gameObject.GetComponent<Entities.Drone.Drone>();
            }
            if (!drone)
            {
                Transform parent = other.transform.parent;
                while (parent != null && !drone)
                {
                    drone = parent.GetComponent<Entities.Drone.Drone>();
                    parent = parent.parent;
                }
            }
            if (!drone)
            {
                return;
            }
            
            bool hasTargetResource = drone.TargetResource != null;
            bool hasAttachedResource = false;
            DroneResourceCollection.Entities.Resource.Resource attachedResource = null;
            
            if (drone is MonoBehaviour droneMono)
            {
                attachedResource = droneMono.GetComponentInChildren<DroneResourceCollection.Entities.Resource.Resource>();
                hasAttachedResource = attachedResource != null;
            }
            
            if (drone.Faction != baseComponent.Faction)
            {
                return;
            }
            
            if (drone.CurrentState != DroneState.Returning)
            {
                return;
            }
            
            bool hasResource = hasTargetResource || hasAttachedResource;
            
            if (!hasResource)
            {
                return;
            }
            
            if (_processedDrones.Contains(drone.Id))
            {
                return;
            }
            
            UnloadResources(drone);
        }
        
        private void OnTriggerExit(Collider other)
        {
            Entities.Drone.Drone drone = other.GetComponent<Entities.Drone.Drone>();
            if (!drone)
            {
                drone = other.GetComponentInParent<Entities.Drone.Drone>();
            }
            
            if (drone)
            {
                _processedDrones.Remove(drone.Id);
            }
        }
        
        private void UnloadResources(Entities.Drone.Drone drone)
        {
            if (!baseComponent || !drone)
            {
                Debug.LogError($"[BaseUnloadTrigger] Cannot unload: baseComponent={baseComponent != null}, drone={drone != null}");
                return;
            }
            
            _processedDrones.Add(drone.Id);
            
            baseComponent.AddResource();
            
            DroneResourceCollection.Entities.Resource.Resource resourceToDestroy = null;
            if (drone.TargetResource != null)
            {
                if (drone.TargetResource is DroneResourceCollection.Entities.Resource.Resource resourceComponent)
                {
                    if (resourceComponent)
                    {
                        resourceToDestroy = resourceComponent;
                    }
                    else
                    {
                        Debug.LogWarning($"[BaseUnloadTrigger] TargetResource component is destroyed for drone {drone.Id}");
                    }
                }
            }
            
            if (!resourceToDestroy && drone is MonoBehaviour droneMono)
            {
                resourceToDestroy = droneMono.GetComponentInChildren<DroneResourceCollection.Entities.Resource.Resource>();
                if (!resourceToDestroy)
                {
                    Debug.LogWarning($"[BaseUnloadTrigger] No attached resource found on drone {drone.Id}");
                }
            }
            
            drone.ClearTargetResource();
            
            if (resourceToDestroy)
            {
                resourceToDestroy.DestroyAfterUnload();
            }
            else
            {
                Debug.LogWarning($"[BaseUnloadTrigger] No resource found to destroy for drone {drone.Id}");
            }
            
            DroneVisuals visuals = drone.GetComponent<DroneVisuals>();
            if (visuals)
            {
                visuals.PlayUnloadEffect();
            }
            else
            {
                Debug.LogWarning($"[BaseUnloadTrigger] DroneVisuals not found for drone {drone.Id}");
            }
            
            DroneStateMachine stateMachine = drone.GetComponent<DroneStateMachine>();
            if (stateMachine)
            {
                stateMachine.ForceSearchingState();
            }
            else
            {
                Debug.LogError($"[BaseUnloadTrigger] DroneStateMachine not found for drone {drone.Id}!");
            }
        }
    }
}

