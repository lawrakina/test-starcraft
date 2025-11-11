using System.Collections;
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
        private Renderer[] _childRenderers; // Рендереры дочерних объектов (например, Visual)

        private void Awake()
        {
            _drone = GetComponent<global::Entities.Drone.Drone>();
            
            // Получаем или создаем Renderer на основном объекте
            _renderer = GetComponent<Renderer>();
            if (_renderer == null)
            {
                _renderer = gameObject.AddComponent<MeshRenderer>();
            }

            // Получаем все рендереры дочерних объектов (для объекта Visual)
            _childRenderers = GetComponentsInChildren<Renderer>(true);
            
            // Создаем материал для дрона
            _material = new Material(Shader.Find("Standard"));
            _renderer.material = _material;
            
            // Применяем материал ко всем дочерним рендерерам
            foreach (var childRenderer in _childRenderers)
            {
                if (childRenderer != _renderer)
                {
                    // Создаем отдельный материал для каждого дочернего объекта
                    Material childMaterial = new Material(Shader.Find("Standard"));
                    childRenderer.material = childMaterial;
                }
            }
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
            if (_drone != null)
            {
                Color factionColor = _drone.Faction == FactionType.Red ? _redFactionColor : _blueFactionColor;
                
                // Применяем цвет к основному рендереру
                if (_material != null)
                {
                    _material.color = factionColor;
                }
                
                // Применяем цвет ко всем дочерним рендерерам (например, объект Visual)
                foreach (var childRenderer in _childRenderers)
                {
                    if (childRenderer != null && childRenderer.material != null)
                    {
                        childRenderer.material.color = factionColor;
                    }
                }
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
            // Частицы
            if (_unloadEffect != null)
            {
                _currentUnloadEffect = Instantiate(_unloadEffect, transform.position, Quaternion.identity);
                _currentUnloadEffect.Play();
                
                // Уничтожаем эффект после завершения
                Destroy(_currentUnloadEffect.gameObject, _currentUnloadEffect.main.duration);
            }
            
            // Вспышка (изменение яркости материала)
            StartCoroutine(FlashEffect());
            
            // Масштаб (пульсация)
            StartCoroutine(ScaleEffect());
        }
        
        private IEnumerator FlashEffect()
        {
            if (_material != null)
            {
                Color originalColor = _material.color;
                Color flashColor = originalColor * 2f; // Увеличиваем яркость
                
                float duration = 0.2f;
                float elapsed = 0f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    _material.color = Color.Lerp(flashColor, originalColor, t);
                    yield return null;
                }
                
                _material.color = originalColor;
            }
        }
        
        private IEnumerator ScaleEffect()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 pulseScale = originalScale * 1.3f; // Увеличиваем масштаб на 30%
            
            float duration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Пульсация: увеличиваем, затем возвращаем
                float scaleFactor = t < 0.5f 
                    ? Mathf.Lerp(1f, 1.3f, t * 2f) 
                    : Mathf.Lerp(1.3f, 1f, (t - 0.5f) * 2f);
                transform.localScale = originalScale * scaleFactor;
                yield return null;
            }
            
            transform.localScale = originalScale;
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

