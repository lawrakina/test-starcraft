using UnityEngine;

namespace DroneResourceCollection.Entities.Resource
{
    /// <summary>
    /// Компонент визуализации ресурса
    /// </summary>
    [RequireComponent(typeof(Resource))]
    public class ResourceVisuals : MonoBehaviour
    {
        [SerializeField] private Color _resourceColor = Color.yellow;
        [SerializeField] private float _rotationSpeed = 30f;
        
        private Renderer _renderer;
        private Material _material;

        private void Awake()
        {
            // Получаем или создаем Renderer
            _renderer = GetComponent<Renderer>();
            if (_renderer == null)
            {
                _renderer = gameObject.AddComponent<MeshRenderer>();
            }

            // Создаем материал для ресурса
            _material = new Material(Shader.Find("Standard"));
            _material.color = _resourceColor;
            _renderer.material = _material;
        }

        private void Update()
        {
            // Вращаем ресурс для визуального эффекта
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        }

        private void OnDestroy()
        {
            // Очищаем материал при уничтожении
            if (_material != null)
            {
                Destroy(_material);
            }
        }
    }
}

