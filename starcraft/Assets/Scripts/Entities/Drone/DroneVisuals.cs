using System.Collections;
using Core.Enums;
using Core.Events;
using Entities.Base;
using UnityEngine;

namespace DroneResourceCollection.Entities.Drone
{
    [RequireComponent(typeof(global::Entities.Drone.Drone))]
    public class DroneVisuals : MonoBehaviour, IUpdatable
    {
        [SerializeField] private Color _redFactionColor = Color.red;
        [SerializeField] private Color _blueFactionColor = Color.blue;
        [SerializeField] private GameObject _stateIndicatorPrefab;
        [SerializeField] private ParticleSystem _unloadEffect;
        [SerializeField] private int updatePriority = 200;
        
        private Renderer _renderer;
        private Material _material;
        private Transform _transform;
        private global::Entities.Drone.Drone _drone;
        private GameObject _stateIndicator;
        private ParticleSystem _currentUnloadEffect;
        private Renderer[] _childRenderers;
        private DroneState _currentState;
        private Color _cachedFactionColor;
        private bool _colorNeedsUpdate = true;
        
        public int UpdatePriority => updatePriority;

        private void Awake()
        {
            _drone = GetComponent<global::Entities.Drone.Drone>();
            _transform = transform;
            
            _renderer = GetComponent<Renderer>();
            if (_renderer == null)
            {
                _renderer = gameObject.AddComponent<MeshRenderer>();
            }

            _childRenderers = GetComponentsInChildren<Renderer>(true);
            
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                urpShader = Shader.Find("Standard");
            }
            
            _material = new Material(urpShader);
            _renderer.material = _material;
            
            foreach (var childRenderer in _childRenderers)
            {
                if (childRenderer != _renderer)
                {
                    Material childMaterial = new Material(urpShader);
                    childRenderer.material = childMaterial;
                }
            }
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<DroneStateChangedEvent>(OnDroneStateChanged);
            UpdateManager.Instance.RegisterUpdatable(this);
        }
        
        private void OnDisable()
        {
            UpdateManager.Instance.UnregisterUpdatable(this);
            EventBus.Instance.Unsubscribe<DroneStateChangedEvent>(OnDroneStateChanged);
        }

        private void Start()
        {
            UpdateColor();
            CreateStateIndicator();
            _currentState = _drone != null ? _drone.CurrentState : DroneState.Idle;
        }

        private void OnDroneStateChanged(DroneStateChangedEvent evt)
        {
            if (evt.Drone == null || !ReferenceEquals(evt.Drone, _drone))
            {
                return;
            }

            _currentState = evt.NewState;
            _colorNeedsUpdate = true;
            UpdateStateIndicator();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_drone != null && _drone.HomeBase != null && _material != null)
            {
                // Проверяем, изменился ли цвет фракции
                Color expectedColor = GetFactionColor();
                if (_colorNeedsUpdate || Vector4.Distance((Vector4)expectedColor, (Vector4)_cachedFactionColor) > 0.01f)
                {
                    _cachedFactionColor = expectedColor;
                    UpdateColor();
                    _colorNeedsUpdate = false;
                }
            }
        }

        private void UpdateColor()
        {
            if (_drone != null)
            {
                Color factionColor = GetFactionColor();
                
                if (_material != null)
                {
                    SetMaterialColor(_material, factionColor);
                }
                
                foreach (var childRenderer in _childRenderers)
                {
                    if (childRenderer != null && childRenderer.material != null)
                    {
                        SetMaterialColor(childRenderer.material, factionColor);
                    }
                }
            }
        }

        private void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
            else
            {
                material.color = color;
            }
        }

        private Color GetFactionColor()
        {
            if (_drone != null && _drone.HomeBase != null)
            {
                if (_drone.HomeBase is Base baseObj)
                {
                    BaseVisuals baseVisuals = baseObj.GetComponent<BaseVisuals>();
                    if (baseVisuals != null)
                    {
                        return baseVisuals.GetFactionColor(_drone.Faction);
                    }
                }
            }
            
            return _drone.Faction == FactionType.Red ? _redFactionColor : _blueFactionColor;
        }

        public void RefreshColor()
        {
            _colorNeedsUpdate = true;
            UpdateColor();
        }

        private void CreateStateIndicator()
        {
            if (_stateIndicatorPrefab != null)
            {
                _stateIndicator = Instantiate(_stateIndicatorPrefab, _transform);
                _stateIndicator.transform.localPosition = Vector3.up * 2f;
            }
        }

        private void UpdateStateIndicator()
        {
            if (_stateIndicator != null && _drone != null)
            {
                // Обновляем цвет индикатора в зависимости от состояния
                Renderer indicatorRenderer = _stateIndicator.GetComponent<Renderer>();
                if (indicatorRenderer != null && indicatorRenderer.material != null)
                {
                    Color stateColor = GetStateColor(_drone.CurrentState);
                    SetMaterialColor(indicatorRenderer.material, stateColor);
                }
            }
        }

        private Color GetStateColor(DroneState state)
        {
            switch (state)
            {
                case DroneState.Idle:
                    return Color.gray;
                case DroneState.Searching:
                    return Color.yellow;
                case DroneState.MovingToResource:
                    // Используем цвет команды для состояния движения к ресурсу
                    return GetFactionColor();
                case DroneState.Collecting:
                    // Используем цвет команды для состояния сбора
                    return GetFactionColor();
                case DroneState.Returning:
                    return Color.green;
                case DroneState.Unloading:
                    return Color.white;
                default:
                    return Color.white;
            }
        }

        public void PlayUnloadEffect()
        {
            if (_unloadEffect != null)
            {
                _currentUnloadEffect = Instantiate(_unloadEffect, _transform.position, Quaternion.identity);
                _currentUnloadEffect.Play();
                Destroy(_currentUnloadEffect.gameObject, _currentUnloadEffect.main.duration);
            }
            
            StartCoroutine(FlashEffect());
            StartCoroutine(ScaleEffect());
        }
        
        private IEnumerator FlashEffect()
        {
            if (_material != null)
            {
                Color originalColor = GetMaterialColor(_material);
                Color flashColor = originalColor * 2f;
                
                float duration = 0.2f;
                float elapsed = 0f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    SetMaterialColor(_material, Color.Lerp(flashColor, originalColor, t));
                    yield return null;
                }
                
                SetMaterialColor(_material, originalColor);
            }
        }

        private Color GetMaterialColor(Material material)
        {
            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }
            else if (material.HasProperty("_Color"))
            {
                return material.GetColor("_Color");
            }
            else
            {
                return material.color;
            }
        }
        
        private IEnumerator ScaleEffect()
        {
            Vector3 originalScale = _transform.localScale;
            
            float duration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scaleFactor = t < 0.5f 
                    ? Mathf.Lerp(1f, 1.3f, t * 2f) 
                    : Mathf.Lerp(1.3f, 1f, (t - 0.5f) * 2f);
                _transform.localScale = originalScale * scaleFactor;
                yield return null;
            }
            
            _transform.localScale = originalScale;
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
            }
        }
    }
}


