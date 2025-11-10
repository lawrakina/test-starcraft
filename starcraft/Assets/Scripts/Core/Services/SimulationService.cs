using Core.Interfaces;

namespace Core.Services
{
    public class SimulationService
    {
        public INavigationService NavigationService { get; }
        public IResourceService ResourceService { get; }
        public IDroneService DroneService { get; }

        public SimulationService(
            INavigationService navigationService,
            IResourceService resourceService,
            IDroneService droneService)
        {
            NavigationService = navigationService;
            ResourceService = resourceService;
            DroneService = droneService;
        }
    }
}

