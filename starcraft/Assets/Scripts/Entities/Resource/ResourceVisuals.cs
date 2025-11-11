using UnityEngine;

namespace DroneResourceCollection.Entities.Resource
{
    /// <summary>
    /// Компонент визуализации ресурса
    /// </summary>
    [RequireComponent(typeof(Resource))]
    public class ResourceVisuals : MonoBehaviour, IUpdatable
    {
        [SerializeField] private Color _resourceColor = Color.yellow;
        [SerializeField] private float _rotationSpeed = 30f;
        [SerializeField] private int updatePriority = 600;
        
        private Renderer _renderer;
        private Material _material;
        private Transform _transform;
        
        public int UpdatePriority => updatePriority;

        private void Awake()
        {
            _transform = transform;
            
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
        
        private void OnEnable()
        {
            UpdateManager.Instance.RegisterUpdatable(this);
        }
        
        private void OnDisable()
        {
            UpdateManager.Instance.UnregisterUpdatable(this);
        }

        public void OnUpdate(float deltaTime)
        {
            // Вращаем ресурс для визуального эффекта
            _transform.Rotate(Vector3.up, _rotationSpeed * deltaTime);
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

