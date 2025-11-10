using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Drone
{
    /// <summary>
    /// Компонент отрисовки пути дрона
    /// </summary>
    [RequireComponent(typeof(DroneMovement))]
    public class DronePathRenderer : MonoBehaviour
    {
        [FormerlySerializedAs("_showPath")] [SerializeField] private bool showPath = true;
        [FormerlySerializedAs("_pathColor")] [SerializeField] private Color pathColor = Color.cyan;
        [FormerlySerializedAs("_pathLineWidth")] [SerializeField] private float pathLineWidth = 0.1f;
        
        private DroneMovement _droneMovement;
        private LineRenderer _lineRenderer;

        public bool ShowPath
        {
            get => showPath;
            set
            {
                showPath = value;
                if (_lineRenderer != null)
                {
                    _lineRenderer.enabled = value;
                }
            }
        }

        private void Awake()
        {
            _droneMovement = GetComponent<DroneMovement>();
            
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = pathColor;
            _lineRenderer.endColor = pathColor;
            _lineRenderer.startWidth = pathLineWidth;
            _lineRenderer.endWidth = pathLineWidth;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.enabled = showPath;
        }

        private void Update()
        {
            if (!showPath || !_lineRenderer)
            {
                return;
            }

            // Получаем путь из DroneMovement через рефлексию или публичное свойство
            // Для упрощения, путь будет отрисовываться через Gizmos в DroneMovement
            // Здесь можно добавить дополнительную логику отрисовки, если нужно
        }
    }
}

