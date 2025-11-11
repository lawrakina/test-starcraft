using System.Collections.Generic;
using Entities.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Drone
{
    [RequireComponent(typeof(DroneMovement))]
    [RequireComponent(typeof(Drone))]
    public class DronePathRenderer : MonoBehaviour, IUpdatable
    {
        [FormerlySerializedAs("_showPath")] [SerializeField] private bool showPath = true;
        [FormerlySerializedAs("_pathColor")] [SerializeField] private Color fallbackPathColor = Color.cyan;
        [FormerlySerializedAs("_pathLineWidth")] [SerializeField] private float pathLineWidth = 0.1f;
        [SerializeField] private int updatePriority = 300;
        
        private DroneMovement _droneMovement;
        private Drone _drone;
        private LineRenderer _lineRenderer;
        private Color _cachedFactionColor;
        private bool _colorNeedsUpdate = true;
        
        public int UpdatePriority => updatePriority;

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
        
        private void OnEnable()
        {
            var updateManager = Core.DI.DependencyHelper.GetUpdateManager();
            if (updateManager != null)
            {
                updateManager.RegisterUpdatable(this);
            }
        }
        
        private void OnDisable()
        {
            var updateManager = Core.DI.DependencyHelper.GetUpdateManager();
            if (updateManager != null)
            {
                updateManager.UnregisterUpdatable(this);
            }
        }

        private List<Vector3> _lastPath;
        private float _lastPathUpdateTime;
        private const float PathUpdateInterval = 0.1f;

        public void OnUpdate(float deltaTime)
        {
            if (!showPath || !_lineRenderer || !_droneMovement)
            {
                if (_lineRenderer != null)
                {
                    _lineRenderer.positionCount = 0;
                }
                return;
            }

            if (_drone != null && _drone.HomeBase != null && _lineRenderer != null)
            {
                Color expectedColor = GetFactionColor();
                if (_colorNeedsUpdate || Vector4.Distance(expectedColor, _cachedFactionColor) > 0.01f)
                {
                    _cachedFactionColor = expectedColor;
                    UpdatePathColor();
                    _colorNeedsUpdate = false;
                }
            }

            if (Time.time - _lastPathUpdateTime < PathUpdateInterval)
            {
                return;
            }

            _lastPathUpdateTime = Time.time;
            var path = _droneMovement.CurrentPath;
            
            if (path == null || path.Count <= 1)
            {
                if (_lineRenderer.positionCount > 0)
                {
                    _lineRenderer.positionCount = 0;
                }
                return;
            }

            bool pathChanged = _lastPath == null || _lastPath.Count != path.Count;
            if (!pathChanged)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    if (Vector3.Distance(_lastPath[i], path[i]) > 0.1f)
                    {
                        pathChanged = true;
                        break;
                    }
                }
            }

            if (pathChanged)
            {
                if (_lastPath == null)
                {
                    _lastPath = new List<Vector3>();
                }
                _lastPath.Clear();
                _lastPath.AddRange(path);
                _lineRenderer.positionCount = path.Count;
                
                for (int i = 0; i < path.Count; i++)
                {
                    _lineRenderer.SetPosition(i, path[i]);
                }
            }
        }

        private Color GetFactionColor()
        {
            if (_drone != null && _drone.HomeBase != null)
            {
                if (_drone.HomeBase is Base.Base baseObj)
                {
                    BaseVisuals baseVisuals = baseObj.GetComponent<BaseVisuals>();
                    if (baseVisuals != null)
                    {
                        return baseVisuals.GetFactionColor(_drone.Faction);
                    }
                }
            }
            
            return fallbackPathColor;
        }

        private void UpdatePathColor()
        {
            if (_lineRenderer != null)
            {
                Color factionColor = GetFactionColor();
                _lineRenderer.startColor = factionColor;
                _lineRenderer.endColor = factionColor;
            }
        }

        public void RefreshPathColor()
        {
            _colorNeedsUpdate = true;
            UpdatePathColor();
        }
    }
}

