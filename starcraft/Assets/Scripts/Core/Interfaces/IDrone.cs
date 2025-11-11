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
        
        int Priority { get; }
        
        IBase HomeBase { get; }
        
        IResource TargetResource { get; }
        
        /// <summary>
        /// Проверяет, стоит ли дрон (не движется)
        /// </summary>
        bool IsStanding { get; }
        
        /// <summary>
        /// Текущая скорость дрона
        /// </summary>
        Vector3 Velocity { get; }
        
        void SetTargetPosition(Vector3 position);
        
        void SetTargetResource(IResource resource);
        
        void ClearTargetResource();
    }
}

