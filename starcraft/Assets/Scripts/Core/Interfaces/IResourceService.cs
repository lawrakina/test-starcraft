using System.Collections.Generic;
using Core.Enums;
using UnityEngine;

namespace Core.Interfaces
{
    public interface IResourceService
    {
        IResource FindNearestAvailableResource(Vector3 position, FactionType? faction = null);
        
        List<IResource> GetAvailableResources();
        
        void RegisterResource(IResource resource);
        
        void UnregisterResource(IResource resource);
        
        int AvailableResourceCount { get; }
    }
}

