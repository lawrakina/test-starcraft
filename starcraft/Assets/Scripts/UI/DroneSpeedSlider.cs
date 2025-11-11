using UnityEngine;
using UnityEngine.UI;

namespace DroneResourceCollection.UI
{
    /// <summary>
    /// Компонент слайдера для управления скоростью дронов
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class DroneSpeedSlider : MonoBehaviour
    {
        [SerializeField] private Text _valueText;
        [SerializeField] private float _minValue = 1f;
        [SerializeField] private float _maxValue = 10f;
        
        private Slider _slider;
        private SimulationManager _simulationManager;

        public void Initialize(SimulationManager simulationManager)
        {
            _simulationManager = simulationManager;
            _slider = GetComponent<Slider>();
            
            _slider.minValue = _minValue;
            _slider.maxValue = _maxValue;
            _slider.value = 5f;
            
            _slider.onValueChanged.AddListener(OnValueChanged);
            UpdateValueText();
        }

        private void OnValueChanged(float value)
        {
            if (_simulationManager != null)
            {
                _simulationManager.SetDroneSpeed(value);
            }
            UpdateValueText();
        }

        private void UpdateValueText()
        {
            if (_valueText != null)
            {
                _valueText.text = $"Drone Speed: {_slider.value:F1}";
            }
        }
    }
}


