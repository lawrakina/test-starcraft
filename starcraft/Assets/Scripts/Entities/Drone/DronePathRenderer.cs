using Entities.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Drone
{
    /// <summary>
    /// Компонент отрисовки пути дрона
    /// </summary>
    [RequireComponent(typeof(DroneMovement))]
    [RequireComponent(typeof(Drone))]
    public class DronePathRenderer : MonoBehaviour
    {
        [FormerlySerializedAs("_showPath")] [SerializeField] private bool showPath = true;
        [FormerlySerializedAs("_pathColor")] [SerializeField] private Color fallbackPathColor = Color.cyan;
        [FormerlySerializedAs("_pathLineWidth")] [SerializeField] private float pathLineWidth = 0.1f;
        
        private DroneMovement _droneMovement;
        private Drone _drone;
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
            _drone = GetComponent<Drone>();
            
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startWidth = pathLineWidth;
            _lineRenderer.endWidth = pathLineWidth;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.enabled = showPath;
        }

        private void Start()
        {
            UpdatePathColor();
        }

        private void Update()
        {
            if (!showPath || !_lineRenderer || !_droneMovement)
            {
                if (_lineRenderer != null)
                {
                    _lineRenderer.positionCount = 0;
                }
                return;
            }

            // Обновляем цвет пути, если база была установлена после Start()
            if (_drone != null && _drone.HomeBase != null && _lineRenderer != null)
            {
                Color expectedColor = GetFactionColor();
                Color currentColor = _lineRenderer.startColor;
                
                // Если цвета не совпадают (с небольшой погрешностью), обновляем
                if (Vector4.Distance(expectedColor, currentColor) > 0.01f)
                {
                    UpdatePathColor();
                }
            }

            // Получаем путь из DroneMovement
            var path = _droneMovement.CurrentPath;
            
            if (path != null && path.Count > 1)
            {
                // Устанавливаем количество точек для LineRenderer
                _lineRenderer.positionCount = path.Count;
                
                // Устанавливаем позиции точек пути
                for (int i = 0; i < path.Count; i++)
                {
                    _lineRenderer.SetPosition(i, path[i]);
                }
            }
            else
            {
                _lineRenderer.positionCount = 0;
            }
        }

        /// <summary>
        /// Получает цвет фракции из базы или использует fallback
        /// </summary>
        private Color GetFactionColor()
        {
            // Пытаемся получить цвет из базы
            if (_drone != null && _drone.HomeBase != null)
            {
                // Кастим IBase к Base для доступа к BaseVisuals
                if (_drone.HomeBase is Base.Base baseObj)
                {
                    BaseVisuals baseVisuals = baseObj.GetComponent<BaseVisuals>();
                    if (baseVisuals != null)
                    {
                        return baseVisuals.GetFactionColor(_drone.Faction);
                    }
                }
            }
            
            // Fallback на локальный цвет, если база еще не установлена
            return fallbackPathColor;
        }

        /// <summary>
        /// Обновляет цвет пути на основе цвета команды
        /// </summary>
        private void UpdatePathColor()
        {
            if (_lineRenderer != null)
            {
                Color factionColor = GetFactionColor();
                _lineRenderer.startColor = factionColor;
                _lineRenderer.endColor = factionColor;
            }
        }

        /// <summary>
        /// Публичный метод для обновления цвета пути (вызывается из Drone.SetHomeBase)
        /// </summary>
        public void RefreshPathColor()
        {
            UpdatePathColor();
        }
    }
}

