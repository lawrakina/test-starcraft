using UnityEngine;

namespace Core.Interfaces
{
    public interface IResource
    {
        int Id { get; }
        
        Vector3 Position { get; }
        
        bool IsReserved { get; set; }
        
        bool IsCollected { get; }
        
        bool Reserve(int droneId);
        
        void Release();
        
        void Collect();
    }
}

