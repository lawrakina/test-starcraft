using Core.Enums;
using UnityEngine;

namespace Entities.Base
{
    /// <summary>
    /// Компонент визуализации базы
    /// </summary>
    [RequireComponent(typeof(Base))]
    public class BaseVisuals : MonoBehaviour
    {
        [SerializeField] private Color redFactionColor = Color.red;
        [SerializeField] private Color blueFactionColor = Color.blue;
        
        private Renderer _renderer;
        private Material _material;
        private Base _base;

        private void Awake()
        {
            _base = GetComponent<Base>();
            
            _renderer = GetComponent<Renderer>();
            if (_renderer == null)
            {
                _renderer = gameObject.AddComponent<MeshRenderer>();
            }

            // Используем URP шейдер
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                urpShader = Shader.Find("Standard");
            }
            _material = new Material(urpShader);
            UpdateColor();
            _renderer.material = _material;
        }

        private void UpdateColor()
        {
            if (_base != null && _material != null)
            {
                Color factionColor = _base.Faction == FactionType.Red ? redFactionColor : blueFactionColor;
                
                // Для URP используем _BaseColor, для стандартного шейдера - color
                if (_material.HasProperty("_BaseColor"))
                {
                    _material.SetColor("_BaseColor", factionColor);
                }
                else
                {
                    _material.color = factionColor;
                }
            }
        }

        /// <summary>
        /// Получает цвет для указанной фракции
        /// </summary>
        public Color GetFactionColor(FactionType faction)
        {
            return faction == FactionType.Red ? redFactionColor : blueFactionColor;
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

