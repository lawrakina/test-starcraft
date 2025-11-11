using Core.Enums;
using Systems.StateMachine.States;
using UnityEngine;

namespace Entities.Drone
{
    /// <summary>
    /// Компонент для отображения прогресса сбора ресурсов над дроном
    /// Отображает прогресс в виде символов: |||||||||| где каждые 10% заменяются на точку
    /// </summary>
    public class DroneCollectionProgressIndicator : MonoBehaviour
    {
        private TextMesh _textMesh;
        private GameObject _textObject;
        private global::Entities.Drone.Drone _drone;
        private DroneStateMachine _stateMachine;
        
        [SerializeField] private float _offsetY = 2.5f; // Высота над дроном
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private int _fontSize = 20;
        
        private const int ProgressBars = 10; // Количество делений (10 делений по 10%)
        
        private void Awake()
        {
            _drone = GetComponentInParent<global::Entities.Drone.Drone>();
            _stateMachine = GetComponentInParent<DroneStateMachine>();
            
            // Создаем дочерний объект для текста
            CreateTextObject();
        }
        
        private void CreateTextObject()
        {
            // Создаем дочерний объект для TextMesh
            _textObject = new GameObject("CollectionProgressText");
            _textObject.transform.SetParent(transform);
            _textObject.transform.localPosition = new Vector3(0, _offsetY, 0);
            _textObject.transform.localRotation = Quaternion.identity;
            _textObject.transform.localScale = Vector3.one;
            
            // Добавляем TextMesh
            _textMesh = _textObject.AddComponent<TextMesh>();
            
            // Настраиваем TextMesh
            _textMesh.color = _textColor;
            _textMesh.fontSize = _fontSize;
            _textMesh.anchor = TextAnchor.MiddleCenter;
            _textMesh.alignment = TextAlignment.Center;
            _textMesh.text = "";
            
            // Скрываем по умолчанию
            _textObject.SetActive(false);
        }
        
        private void Update()
        {
            // Показываем индикатор только когда дрон собирает ресурсы
            if (_drone != null && _drone.CurrentState == DroneState.Collecting && _textObject != null)
            {
                UpdateProgress();
                _textObject.SetActive(true);
            }
            else if (_textObject != null)
            {
                _textObject.SetActive(false);
            }
        }
        
        private void LateUpdate()
        {
            // Поворачиваем текст к камере (billboard effect)
            if (_textObject != null && Camera.main != null)
            {
                _textObject.transform.LookAt(_textObject.transform.position + Camera.main.transform.rotation * Vector3.forward,
                    Camera.main.transform.rotation * Vector3.up);
            }
        }
        
        private void UpdateProgress()
        {
            if (_stateMachine == null || _stateMachine.StateMachine == null)
            {
                _textMesh.text = "";
                return;
            }
            
            // Получаем прогресс из состояния Collecting
            float progress = GetCollectionProgress();
            
            // Формируем строку прогресса
            string progressText = FormatProgress(progress);
            _textMesh.text = progressText;
        }
        
        /// <summary>
        /// Получает прогресс сбора из состояния Collecting
        /// </summary>
        private float GetCollectionProgress()
        {
            if (_stateMachine?.StateMachine?.CurrentState == null)
            {
                return 0f;
            }
            
            // Получаем текущее состояние
            var currentState = _stateMachine.StateMachine.CurrentState;
            
            // Проверяем тип состояния и получаем прогресс
            if (currentState is DroneCollectingState collectingState)
            {
                return collectingState.GetProgress();
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Форматирует прогресс в виде символов
        /// |||||||||| - начальное состояние
        /// Каждые 10% заменяются на точку .
        /// </summary>
        private string FormatProgress(float progress)
        {
            // Вычисляем сколько делений должно быть точками
            int completedSegments = Mathf.FloorToInt(progress * ProgressBars);
            
            // Создаем строку: сначала точки (завершенные сегменты), затем палки
            string result = "";
            
            // Добавляем точки для завершенных сегментов
            for (int i = 0; i < completedSegments; i++)
            {
                result += ".";
            }
            
            // Добавляем палки для оставшихся сегментов
            for (int i = completedSegments; i < ProgressBars; i++)
            {
                result += "|";
            }
            
            return result;
        }
    }
}

