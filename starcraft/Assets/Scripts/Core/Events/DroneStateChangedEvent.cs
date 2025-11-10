using Core.Enums;
using Core.Interfaces;

namespace Core.Events
{
    public class DroneStateChangedEvent
    {
        public IDrone Drone { get; }
        
        public DroneState PreviousState { get; }
        
        public DroneState NewState { get; }

        public DroneStateChangedEvent(IDrone drone, DroneState previousState, DroneState newState)
        {
            Drone = drone;
            PreviousState = previousState;
            NewState = newState;
        }
    }
}

