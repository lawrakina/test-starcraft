using Core.Enums;
using UnityEngine;

namespace DroneResourceCollection.Entities.Drone
{
    /// <summary>
    /// Компонент визуализации дрона
    /// Управляет цветом, индикаторами состояния и визуальными эффектами
    /// </summary>
    [RequireComponent(typeof(global::Entities.Drone.Drone))]
    public class DroneVisuals : MonoBehaviour
    {
        [SerializeField] private Color _redFactionColor = Color.red;
        [SerializeField] private Color _blueFactionColor = Color.blue;
        [SerializeField] private GameObject _stateIndicatorPrefab;
        [SerializeField] private ParticleSystem _unloadEffect;
        
        private Renderer _renderer;
        private Material _material;
        private global::Entities.Drone.Drone _drone;
        private GameObject _stateIndicator;
        private ParticleSystem _currentUnloadEffect;

        private void Awake()
        {
            _drone = GetComponent<global::Entities.Drone.Drone>();
            
            // Получаем или создаем Renderer
            _renderer = GetComponent<Renderer>();
            if (_renderer == null)
            {
                _renderer = gameObject.AddComponent<MeshRenderer>();
            }

            // Создаем материал для дрона
            _material = new Material(Shader.Find("Standard"));
            _renderer.material = _material;
        }

        private void Start()
        {
            UpdateColor();
            CreateStateIndicator();
        }

        private void Update()
        {
            UpdateStateIndicator();
        }

        private void UpdateColor()
        {
            if (_drone != null && _material != null)
            {
                _material.color = _drone.Faction == FactionType.Red ? _redFactionColor : _blueFactionColor;
            }
        }

        private void CreateStateIndicator()
        {
            if (_stateIndicatorPrefab != null)
            {
                _stateIndicator = Instantiate(_stateIndicatorPrefab, transform);
                _stateIndicator.transform.localPosition = Vector3.up * 2f;
            }
        }

        private void UpdateStateIndicator()
        {
            if (_stateIndicator != null && _drone != null)
            {
                // Обновляем цвет индикатора в зависимости от состояния
                Renderer indicatorRenderer = _stateIndicator.GetComponent<Renderer>();
                if (indicatorRenderer != null)
                {
                    Color stateColor = GetStateColor(_drone.CurrentState);
                    indicatorRenderer.material.color = stateColor;
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
                    return Color.cyan;
                case DroneState.Collecting:
                    return Color.magenta;
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
                _currentUnloadEffect = Instantiate(_unloadEffect, transform.position, Quaternion.identity);
                _currentUnloadEffect.Play();
                
                // Уничтожаем эффект после завершения
                Destroy(_currentUnloadEffect.gameObject, _currentUnloadEffect.main.duration);
            }
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

