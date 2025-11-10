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

            _material = new Material(Shader.Find("Standard"));
            UpdateColor();
            _renderer.material = _material;
        }

        private void UpdateColor()
        {
            if (_base != null && _material != null)
            {
                _material.color = _base.Faction == FactionType.Red ? redFactionColor : blueFactionColor;
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

