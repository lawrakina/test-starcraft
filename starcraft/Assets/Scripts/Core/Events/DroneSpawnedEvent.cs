using Core.Interfaces;

namespace Core.Events
{
    public class DroneSpawnedEvent
    {
        public IDrone Drone { get; }

        public DroneSpawnedEvent(IDrone drone)
        {
            Drone = drone;
        }
    }
}

