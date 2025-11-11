using UnityEngine;
using UnityEngine.UI;

namespace DroneResourceCollection.UI
{
    /// <summary>
    /// Компонент слайдера для управления количеством дронов на фракцию
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class DroneCountSlider : MonoBehaviour
    {
        [SerializeField] private Text _valueText;
        [SerializeField] private int _minValue = 1;
        [SerializeField] private int _maxValue = 10;
        
        private Slider _slider;
        private SimulationManager _simulationManager;

        public void Initialize(SimulationManager simulationManager)
        {
            _simulationManager = simulationManager;
            _slider = GetComponent<Slider>();
            
            _slider.minValue = _minValue;
            _slider.maxValue = _maxValue;
            _slider.wholeNumbers = true;
            _slider.value = 3;
            
            _slider.onValueChanged.AddListener(OnValueChanged);
            UpdateValueText();
        }

        private void OnValueChanged(float value)
        {
            int intValue = Mathf.RoundToInt(value);
            if (_simulationManager != null)
            {
                _simulationManager.SetDronesPerFaction(intValue);
            }
            UpdateValueText();
        }

        private void UpdateValueText()
        {
            if (_valueText != null)
            {
                _valueText.text = $"Drones per Faction: {Mathf.RoundToInt(_slider.value)}";
            }
        }
    }
}


