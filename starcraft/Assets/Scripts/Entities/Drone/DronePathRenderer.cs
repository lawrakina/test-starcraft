using System.Collections.Generic;
using Entities.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Drone
{
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

        private List<Vector3> _lastPath;
        private float _lastPathUpdateTime;
        private const float PathUpdateInterval = 0.1f;

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

            if (_drone != null && _drone.HomeBase != null && _lineRenderer != null)
            {
                Color expectedColor = GetFactionColor();
                Color currentColor = _lineRenderer.startColor;
                
                if (Vector4.Distance(expectedColor, currentColor) > 0.01f)
                {
                    UpdatePathColor();
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
                _lastPath = new List<Vector3>(path);
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
            UpdatePathColor();
        }
    }
}

