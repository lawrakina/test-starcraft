using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Interfaces;
using DroneResourceCollection.Entities.Resource;
using UnityEngine;

namespace Core.Services
{
    public class ResourceService : IResourceService
    {
        private readonly List<IResource> _resources = new();
        private float _lastCleanupTime;
        private const float CleanupInterval = 1.0f;

        private bool IsResourceValid(IResource resource)
        {
            if (resource == null)
            {
                return false;
            }
            
            if (resource is MonoBehaviour resourceMono)
            {
                return resourceMono != null;
            }
            
            return true;
        }
        
        private void CleanupDestroyedResources()
        {
            float currentTime = Time.time;
            if (currentTime - _lastCleanupTime < CleanupInterval)
            {
                return;
            }
            
            _lastCleanupTime = currentTime;
            _resources.RemoveAll(r => !IsResourceValid(r));
        }

        public int AvailableResourceCount
        {
            get
            {
                CleanupDestroyedResources();
                return _resources.Count(r => IsResourceValid(r) && !r.IsReserved && !r.IsCollected);
            }
        }

        public IResource FindNearestAvailableResource(Vector3 position, FactionType? faction = null)
        {
            CleanupDestroyedResources();
            
            var availableResources = _resources
                .Where(r => 
                {
                    if (!IsResourceValid(r))
                    {
                        return false;
                    }
                    
                    if (r.IsCollected)
                        return false;
                    
                    if (faction.HasValue)
                    {
                        if (r is Resource resource)
                        {
                            if (!IsResourceValid(resource))
                            {
                                return false;
                            }
                            return !resource.IsReservedByFaction(faction.Value);
                        }
                        return !r.IsReserved;
                    }
                    
                    return !r.IsReserved;
                })
                .ToList();

            if (availableResources.Count == 0)
            {
                return null;
            }

            IResource nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var resource in availableResources)
            {
                if (!IsResourceValid(resource))
                {
                    continue;
                }
                
                try
                {
                    float distance = Vector3.Distance(position, resource.Position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = resource;
                    }
                }
                catch (MissingReferenceException)
                {
                    Debug.LogWarning($"[ResourceService] Resource was destroyed during search, skipping");
                    continue;
                }
            }

            return nearest;
        }

        public List<IResource> GetAvailableResources()
        {
            CleanupDestroyedResources();
            return _resources.Where(r => IsResourceValid(r) && !r.IsReserved && !r.IsCollected).ToList();
        }

        public void RegisterResource(IResource resource)
        {
            if (resource != null && !_resources.Contains(resource))
            {
                _resources.Add(resource);
            }
        }

        public void UnregisterResource(IResource resource)
        {
            _resources.Remove(resource);
        }
    }
}

