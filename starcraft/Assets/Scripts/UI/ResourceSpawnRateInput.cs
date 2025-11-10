using UnityEngine;
using UnityEngine.UI;

namespace DroneResourceCollection.UI
{
    /// <summary>
    /// Компонент поля ввода для управления частотой генерации ресурсов
    /// </summary>
    [RequireComponent(typeof(InputField))]
    public class ResourceSpawnRateInput : MonoBehaviour
    {
        [SerializeField] private float _minValue = 0.5f;
        [SerializeField] private float _maxValue = 30f;
        
        private InputField _inputField;
        private SimulationManager _simulationManager;

        public void Initialize(SimulationManager simulationManager)
        {
            _simulationManager = simulationManager;
            _inputField = GetComponent<InputField>();
            _inputField.contentType = InputField.ContentType.DecimalNumber;
            _inputField.text = "5.0";
            
            _inputField.onEndEdit.AddListener(OnValueChanged);
        }

        private void OnValueChanged(string value)
        {
            if (float.TryParse(value, out float floatValue))
            {
                floatValue = Mathf.Clamp(floatValue, _minValue, _maxValue);
                
                if (_simulationManager != null)
                {
                    _simulationManager.SetResourceSpawnRate(floatValue);
                }
                
                _inputField.text = floatValue.ToString("F1");
            }
            else
            {
                _inputField.text = "5.0";
            }
        }
    }
}

