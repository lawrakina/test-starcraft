using Core.Enums;
using Core.Interfaces;

namespace Core.Events
{
    public class ResourceCollectedEvent
    {
        public IDrone Drone { get; }
        
        public IResource Resource { get; }
        
        public FactionType Faction { get; }

        public ResourceCollectedEvent(IDrone drone, IResource resource, FactionType faction)
        {
            Drone = drone;
            Resource = resource;
            Faction = faction;
        }
    }
}

