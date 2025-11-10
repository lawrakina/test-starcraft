using Core.Enums;
using UnityEngine;

namespace Core.Interfaces
{
    public interface IDrone
    {
        int Id { get; }
        
        FactionType Faction { get; }
        
        DroneState CurrentState { get; }
        
        Vector3 Position { get; }
        
        float Speed { get; set; }
        
        IBase HomeBase { get; }
        
        IResource TargetResource { get; }
        
        void SetTargetPosition(Vector3 position);
        
        void SetTargetResource(IResource resource);
        
        void ClearTargetResource();
    }
}

