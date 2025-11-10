using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Core.Services
{
    public class ResourceService : IResourceService
    {
        private readonly List<IResource> _resources = new();

        public int AvailableResourceCount => _resources.Count(r => !r.IsReserved && !r.IsCollected);

        public IResource FindNearestAvailableResource(Vector3 position, FactionType? faction = null)
        {
            var availableResources = _resources
                .Where(r => !r.IsReserved && !r.IsCollected)
                .ToList();

            if (availableResources.Count == 0)
            {
                return null;
            }

            IResource nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var resource in availableResources)
            {
                float distance = Vector3.Distance(position, resource.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = resource;
                }
            }

            return nearest;
        }

        public List<IResource> GetAvailableResources()
        {
            return _resources.Where(r => !r.IsReserved && !r.IsCollected).ToList();
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

