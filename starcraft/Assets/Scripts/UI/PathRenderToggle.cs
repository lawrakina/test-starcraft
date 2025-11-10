using Entities.Drone;
using UnityEngine;
using UnityEngine.UI;

namespace DroneResourceCollection.UI
{
    /// <summary>
    /// Компонент чекбокса для включения/отключения отрисовки пути дронов
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class PathRenderToggle : MonoBehaviour
    {
        private Toggle _toggle;
        private SimulationManager _simulationManager;

        public void Initialize(SimulationManager simulationManager)
        {
            _simulationManager = simulationManager;
            _toggle = GetComponent<Toggle>();
            _toggle.isOn = false;
            
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool value)
        {
            if (_simulationManager != null && _simulationManager.DroneService != null)
            {
                var allDrones = _simulationManager.DroneService.GetAllDrones();
                foreach (var drone in allDrones)
                {
                    if (drone is Drone droneMono)
                    {
                        droneMono.SetShowPath(value);
                    }
                }
            }
        }
    }
}

