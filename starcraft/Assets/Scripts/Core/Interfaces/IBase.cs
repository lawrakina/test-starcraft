using Core.Enums;
using UnityEngine;

namespace Core.Interfaces
{
    public interface IBase
    {
        FactionType Faction { get; }
        
        Vector3 Position { get; }
        
        Vector3 UnloadPoint { get; }
        
        int ResourceCount { get; }
        
        void AddResource();
        
        Vector3 GetSpawnPoint(int index);
    }
}

