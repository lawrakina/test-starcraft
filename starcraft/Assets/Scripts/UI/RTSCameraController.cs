using UnityEngine;

namespace DroneResourceCollection.UI
{
    /// <summary>
    /// Контроллер камеры в стиле RTS (стратегий)
    /// Управление: стрелки/WASD для движения, колесико мыши для зума
    /// </summary>
    public class RTSCameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 10f;
        
        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minHeight = 5f;
        [SerializeField] private float maxHeight = 30f;
        
        [Header("Camera Settings")]
        [SerializeField] private float cameraAngle = 60f; // Угол наклона камеры по оси X
        
        [Header("Control Settings")]
        [SerializeField] private bool enableRTSControl = true; // Флаг для переключения режимов
        
        private Camera _camera;
        private Vector3 _initialRotation;
        
        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            
            // Сохраняем начальный поворот камеры
            if (_camera != null)
            {
                _initialRotation = _camera.transform.eulerAngles;
                // Устанавливаем фиксированный угол, если он еще не установлен
                if (Mathf.Approximately(_initialRotation.x, 0f))
                {
                    _camera.transform.rotation = Quaternion.Euler(cameraAngle, _initialRotation.y, _initialRotation.z);
                }
            }
        }
        
        private void Update()
        {
            if (!enableRTSControl || _camera == null)
            {
                return;
            }
            
            HandleMovement();
            HandleZoom();
            MaintainCameraAngle();
        }
        
        /// <summary>
        /// Обработка движения камеры по стрелкам и WASD
        /// </summary>
        private void HandleMovement()
        {
            Vector3 moveDirection = Vector3.zero;
            
            // Движение вперед (UpArrow или W)
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                moveDirection += Vector3.forward;
            }
            
            // Движение назад (DownArrow или S)
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                moveDirection += Vector3.back;
            }
            
            // Движение влево (LeftArrow или A)
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                moveDirection += Vector3.left;
            }
            
            // Движение вправо (RightArrow или D)
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                moveDirection += Vector3.right;
            }
            
            // Нормализуем направление для диагонального движения
            if (moveDirection.magnitude > 0f)
            {
                moveDirection.Normalize();
                
                // Применяем движение в локальном пространстве камеры, но только по X и Z
                // Чтобы движение было горизонтальным независимо от угла камеры
                Vector3 worldMove = new Vector3(moveDirection.x, 0f, moveDirection.z);
                worldMove *= moveSpeed * Time.deltaTime;
                
                _camera.transform.position += worldMove;
            }
        }
        
        /// <summary>
        /// Обработка зума колесиком мыши
        /// </summary>
        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // Изменяем высоту камеры для зума
                Vector3 position = _camera.transform.position;
                float newHeight = position.y - (scroll * zoomSpeed);
                
                // Ограничиваем высоту
                newHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);
                
                position.y = newHeight;
                _camera.transform.position = position;
            }
        }
        
        /// <summary>
        /// Поддерживает фиксированный угол наклона камеры
        /// </summary>
        private void MaintainCameraAngle()
        {
            Vector3 currentEuler = _camera.transform.eulerAngles;
            
            // Нормализуем угол X к диапазону 0-360 для корректного сравнения
            float normalizedX = currentEuler.x;
            if (normalizedX > 180f)
            {
                normalizedX -= 360f;
            }
            
            // Устанавливаем фиксированный угол по X, сохраняя Y и Z
            // Используем небольшую погрешность для сравнения
            if (Mathf.Abs(normalizedX - cameraAngle) > 0.1f)
            {
                _camera.transform.rotation = Quaternion.Euler(cameraAngle, currentEuler.y, currentEuler.z);
            }
        }
        
        /// <summary>
        /// Включить/выключить RTS управление (для переключения режимов)
        /// </summary>
        public void SetRTSControlEnabled(bool enabled)
        {
            enableRTSControl = enabled;
        }
        
        /// <summary>
        /// Проверить, активно ли RTS управление
        /// </summary>
        public bool IsRTSControlEnabled()
        {
            return enableRTSControl;
        }
    }
}

