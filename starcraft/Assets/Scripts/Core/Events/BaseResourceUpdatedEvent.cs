using Core.Enums;

namespace Core.Events
{
    public class BaseResourceUpdatedEvent
    {
        public FactionType Faction { get; }

        public int ResourceCount { get; }

        public BaseResourceUpdatedEvent(FactionType faction, int resourceCount)
        {
            Faction = faction;
            ResourceCount = resourceCount;
        }
    }
}