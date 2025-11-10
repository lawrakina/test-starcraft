using Core.Events;
using UnityEngine;

namespace DroneResourceCollection.UI
{
    /// <summary>
    /// Главный контроллер UI симуляции
    /// Координирует работу всех UI компонентов
    /// </summary>
    public class SimulationUIController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private DroneCountSlider _droneCountSlider;
        [SerializeField] private DroneSpeedSlider _droneSpeedSlider;
        [SerializeField] private ResourceSpawnRateInput _resourceSpawnRateInput;
        [SerializeField] private PathRenderToggle _pathRenderToggle;
        [SerializeField] private ResourceCounterDisplay _resourceCounterDisplay;
        [SerializeField] private DroneSelector _droneSelector;
        [SerializeField] private MinimapController _minimapController;
        [SerializeField] private SimulationSpeedController _simulationSpeedController;

        [Header("References")]
        [SerializeField] private SimulationManager _simulationManager;

        private void Start()
        {
            // Подписываемся на события
            EventBus.Instance.Subscribe<BaseResourceUpdatedEvent>(OnResourceUpdated);
            
            // Инициализируем UI компоненты
            InitializeUIComponents();
        }

        private void InitializeUIComponents()
        {
            if (_droneCountSlider != null)
            {
                _droneCountSlider.Initialize(_simulationManager);
            }

            if (_droneSpeedSlider != null)
            {
                _droneSpeedSlider.Initialize(_simulationManager);
            }

            if (_resourceSpawnRateInput != null)
            {
                _resourceSpawnRateInput.Initialize(_simulationManager);
            }

            if (_pathRenderToggle != null)
            {
                _pathRenderToggle.Initialize(_simulationManager);
            }

            if (_resourceCounterDisplay != null)
            {
                _resourceCounterDisplay.Initialize();
            }

            if (_droneSelector != null)
            {
                _droneSelector.Initialize(_simulationManager);
            }

            if (_minimapController != null)
            {
                _minimapController.Initialize(_simulationManager);
            }

            if (_simulationSpeedController != null)
            {
                _simulationSpeedController.Initialize();
            }
        }

        private void OnResourceUpdated(BaseResourceUpdatedEvent eventData)
        {
            if (_resourceCounterDisplay != null)
            {
                _resourceCounterDisplay.UpdateResourceCount(eventData.Faction, eventData.ResourceCount);
            }
        }

        private void OnDestroy()
        {
            EventBus.Instance.Unsubscribe<BaseResourceUpdatedEvent>(OnResourceUpdated);
        }
    }
}

