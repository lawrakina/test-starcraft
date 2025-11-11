using System.Collections;
using Core.Enums;
using Entities.Base;
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
            
            // Используем URP шейдер
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                urpShader = Shader.Find("Standard");
            }
            
            // Создаем материал для дрона
            _material = new Material(urpShader);
            _renderer.material = _material;
            
            // Применяем материал ко всем дочерним рендерерам
            foreach (var childRenderer in _childRenderers)
            {
                if (childRenderer != _renderer)
                {
                    // Создаем отдельный материал для каждого дочернего объекта
                    Material childMaterial = new Material(urpShader);
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
            
            // Обновляем цвет, если база была установлена после Start()
            if (_drone != null && _drone.HomeBase != null && _material != null)
            {
                // Проверяем, что цвет соответствует цвету базы (на случай, если база установилась позже)
                Color expectedColor = GetFactionColor();
                Color currentColor = GetMaterialColor(_material);
                
                // Если цвета не совпадают (с небольшой погрешностью), обновляем
                if (Vector4.Distance((Vector4)expectedColor, (Vector4)currentColor) > 0.01f)
                {
                    UpdateColor();
                }
            }
        }

        private void UpdateColor()
        {
            if (_drone != null)
            {
                Color factionColor = GetFactionColor();
                
                // Применяем цвет к основному рендереру
                if (_material != null)
                {
                    SetMaterialColor(_material, factionColor);
                }
                
                // Применяем цвет ко всем дочерним рендерерам (например, объект Visual)
                foreach (var childRenderer in _childRenderers)
                {
                    if (childRenderer != null && childRenderer.material != null)
                    {
                        SetMaterialColor(childRenderer.material, factionColor);
                    }
                }
            }
        }

        /// <summary>
        /// Устанавливает цвет материала с поддержкой URP (_BaseColor) и стандартного шейдера (color)
        /// </summary>
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

        /// <summary>
        /// Получает цвет фракции из базы или использует fallback
        /// </summary>
        private Color GetFactionColor()
        {
            // Пытаемся получить цвет из базы
            if (_drone != null && _drone.HomeBase != null)
            {
                // Кастим IBase к Base для доступа к BaseVisuals
                if (_drone.HomeBase is Base baseObj)
                {
                    BaseVisuals baseVisuals = baseObj.GetComponent<BaseVisuals>();
                    if (baseVisuals != null)
                    {
                        return baseVisuals.GetFactionColor(_drone.Faction);
                    }
                }
            }
            
            // Fallback на локальные цвета, если база еще не установлена
            return _drone.Faction == FactionType.Red ? _redFactionColor : _blueFactionColor;
        }

        /// <summary>
        /// Публичный метод для обновления цвета (вызывается из Drone.SetHomeBase)
        /// </summary>
        public void RefreshColor()
        {
            UpdateColor();
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
                Color originalColor = GetMaterialColor(_material);
                Color flashColor = originalColor * 2f; // Увеличиваем яркость
                
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

        /// <summary>
        /// Получает цвет материала с поддержкой URP (_BaseColor) и стандартного шейдера (color)
        /// </summary>
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

