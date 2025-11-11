using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Core.Interfaces;

namespace DroneResourceCollection.UI
{
    /// <summary>
    /// Компонент выбора дрона для слежения (опциональная функция)
    /// </summary>
    public class DroneSelector : MonoBehaviour
    {
        [SerializeField] private Dropdown _droneDropdown;
        [SerializeField] private Camera _camera;
        
        private SimulationManager _simulationManager;
        private IDrone _selectedDrone;
        private List<IDrone> _allDrones = new List<IDrone>();

        public void Initialize(SimulationManager simulationManager)
        {
            _simulationManager = simulationManager;
            
            if (_droneDropdown != null)
            {
                _droneDropdown.onValueChanged.AddListener(OnDroneSelected);
            }
            
            RefreshDroneList();
        }

        private void Update()
        {
            // Обновляем позицию камеры для слежения за выбранным дроном
            if (_selectedDrone != null && _camera != null)
            {
                Vector3 targetPosition = _selectedDrone.Position;
                targetPosition.y = _camera.transform.position.y; // Сохраняем высоту камеры
                _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPosition, Time.deltaTime * 2f);
            }
        }

        private void RefreshDroneList()
        {
            if (_simulationManager == null || _simulationManager.DroneService == null)
            {
                return;
            }

            _allDrones = _simulationManager.DroneService.GetAllDrones();
            
            if (_droneDropdown != null)
            {
                _droneDropdown.ClearOptions();
                List<string> options = new List<string> { "None" };
                
                foreach (var drone in _allDrones)
                {
                    options.Add($"Drone {drone.Id} ({drone.Faction})");
                }
                
                _droneDropdown.AddOptions(options);
            }
        }

        private void OnDroneSelected(int index)
        {
            if (index == 0)
            {
                _selectedDrone = null;
            }
            else if (index - 1 < _allDrones.Count)
            {
                _selectedDrone = _allDrones[index - 1];
            }
        }
    }
}


