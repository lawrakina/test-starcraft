using UnityEngine;

namespace DroneResourceCollection.UI
{
    /// <summary>
    /// Контроллер камеры в стиле RTS
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
        [SerializeField] private float cameraAngle = 60f;
        
        [Header("Control Settings")]
        [SerializeField] private bool enableRTSControl = true;
        
        private Camera _camera;
        private Vector3 _initialRotation;
        
        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            
            if (_camera != null)
            {
                _initialRotation = _camera.transform.eulerAngles;
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
        /// Обрабатывает движение камеры
        /// </summary>
        private void HandleMovement()
        {
            Vector3 moveDirection = Vector3.zero;
            
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                moveDirection += Vector3.forward;
            }
            
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                moveDirection += Vector3.back;
            }
            
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                moveDirection += Vector3.left;
            }
            
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                moveDirection += Vector3.right;
            }
            
            if (moveDirection.magnitude > 0f)
            {
                moveDirection.Normalize();
                
                Vector3 worldMove = new Vector3(moveDirection.x, 0f, moveDirection.z);
                worldMove *= moveSpeed * Time.deltaTime;
                
                _camera.transform.position += worldMove;
            }
        }
        
        /// <summary>
        /// Обрабатывает зум колесиком мыши
        /// </summary>
        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 position = _camera.transform.position;
                float newHeight = position.y - (scroll * zoomSpeed);
                
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
            
            float normalizedX = currentEuler.x;
            if (normalizedX > 180f)
            {
                normalizedX -= 360f;
            }
            
            if (Mathf.Abs(normalizedX - cameraAngle) > 0.1f)
            {
                _camera.transform.rotation = Quaternion.Euler(cameraAngle, currentEuler.y, currentEuler.z);
            }
        }
        
        /// <summary>
        /// Включает/выключает RTS управление
        /// </summary>
        public void SetRTSControlEnabled(bool enabled)
        {
            enableRTSControl = enabled;
        }
        
        /// <summary>
        /// Проверяет, активно ли RTS управление
        /// </summary>
        public bool IsRTSControlEnabled()
        {
            return enableRTSControl;
        }
    }
}

