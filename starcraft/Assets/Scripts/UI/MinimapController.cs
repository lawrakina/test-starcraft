using UnityEngine;
using UnityEngine.UI;

namespace DroneResourceCollection.UI
{
    /// <summary>
    /// Контроллер миникарты (опциональная функция)
    /// </summary>
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] private RawImage _minimapImage;
        [SerializeField] private Camera _minimapCamera;
        [SerializeField] private RectTransform _minimapRect;
        
        private SimulationManager _simulationManager;
        private RenderTexture _minimapTexture;

        public void Initialize(SimulationManager simulationManager)
        {
            _simulationManager = simulationManager;
            
            // Создаем RenderTexture для миникарты
            _minimapTexture = new RenderTexture(256, 256, 16);
            
            if (_minimapCamera != null)
            {
                _minimapCamera.targetTexture = _minimapTexture;
            }
            
            if (_minimapImage != null)
            {
                _minimapImage.texture = _minimapTexture;
            }
            
            // Настраиваем камеру миникарты (вид сверху)
            if (_minimapCamera != null)
            {
                _minimapCamera.orthographic = true;
                _minimapCamera.orthographicSize = 20f;
                _minimapCamera.transform.position = new Vector3(0, 30, 0);
                _minimapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }

        private void Update()
        {
            // Обновляем позицию камеры миникарты для охвата всей сцены
            if (_minimapCamera != null && _simulationManager != null)
            {
                // Можно добавить логику для следования за центром сцены или выбранным дроном
            }
        }

        private void OnDestroy()
        {
            if (_minimapTexture != null)
            {
                _minimapTexture.Release();
                Destroy(_minimapTexture);
            }
        }
    }
}


