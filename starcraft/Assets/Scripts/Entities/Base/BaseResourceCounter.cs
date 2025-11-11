using Core.Events;

namespace Entities.Base
{
    public class BaseResourceCounter : UnityEngine.MonoBehaviour
    {
        private int _count;
        private Base _base;

        public int Count => _count;

        private void Awake()
        {
            _base = GetComponent<Base>();
        }

        public void AddResource()
        {
            _count++;
            
            if (_base)
            {
                var eventBus = Core.DI.DependencyHelper.GetEventBus();
                eventBus?.Publish(new BaseResourceUpdatedEvent(_base.Faction, _count));
            }
        }

        public void ResetCount()
        {
            _count = 0;
            
            if (_base != null)
            {
                var eventBus = Core.DI.DependencyHelper.GetEventBus();
                eventBus?.Publish(new BaseResourceUpdatedEvent(_base.Faction, _count));
            }
        }
    }
}

