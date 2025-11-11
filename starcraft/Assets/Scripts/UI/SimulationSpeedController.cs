using UnityEngine;
using UnityEngine.UI;

namespace DroneResourceCollection.UI
{
    /// <summary>
    /// Контроллер управления скоростью симуляции (опциональная функция)
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class SimulationSpeedController : MonoBehaviour
    {
        [SerializeField] private Text _valueText;
        [SerializeField] private float _minValue = 0.1f;
        [SerializeField] private float _maxValue = 3f;
        
        private Slider _slider;

        public void Initialize()
        {
            _slider = GetComponent<Slider>();
            
            _slider.minValue = _minValue;
            _slider.maxValue = _maxValue;
            _slider.value = 1f;
            
            _slider.onValueChanged.AddListener(OnValueChanged);
            UpdateValueText();
        }

        private void OnValueChanged(float value)
        {
            Time.timeScale = value;
            UpdateValueText();
        }

        private void UpdateValueText()
        {
            if (_valueText != null)
            {
                _valueText.text = $"Simulation Speed: {_slider.value:F1}x";
            }
        }

        private void OnDestroy()
        {
            // Сбрасываем скорость времени при уничтожении
            Time.timeScale = 1f;
        }
    }
}


