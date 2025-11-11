using Core.Enums;
using Core.Events;
using Systems.StateMachine.States;
using UnityEngine;

namespace Entities.Drone
{
    public class DroneCollectionProgressIndicator : MonoBehaviour, IUpdatable
    {
        private TextMesh _textMesh;
        private GameObject _textObject;
        private Transform _textObjectTransform;
        private global::Entities.Drone.Drone _drone;
        private DroneStateMachine _stateMachine;
        private DroneState _currentState;
        private float _lastProgressUpdateTime;
        private const float ProgressUpdateInterval = 0.1f;
        private Camera _mainCamera;
        
        [SerializeField] private float _offsetY = 2.5f;
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private int _fontSize = 20;
        [SerializeField] private int updatePriority = 400;
        
        private const int ProgressBars = 10;
        
        public int UpdatePriority => updatePriority;
        
        private void Awake()
        {
            _drone = GetComponentInParent<global::Entities.Drone.Drone>();
            _stateMachine = GetComponentInParent<DroneStateMachine>();
            _mainCamera = Camera.main;
            CreateTextObject();
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<DroneStateChangedEvent>(OnDroneStateChanged);
            UpdateManager.Instance.RegisterUpdatable(this);
        }

        private void OnDisable()
        {
            UpdateManager.Instance.UnregisterUpdatable(this);
            EventBus.Instance.Unsubscribe<DroneStateChangedEvent>(OnDroneStateChanged);
        }
        
        private void CreateTextObject()
        {
            _textObject = new GameObject("CollectionProgressText");
            _textObjectTransform = _textObject.transform;
            _textObjectTransform.SetParent(transform);
            _textObjectTransform.localPosition = new Vector3(0, _offsetY, 0);
            _textObjectTransform.localRotation = Quaternion.identity;
            _textObjectTransform.localScale = Vector3.one;
            
            _textMesh = _textObject.AddComponent<TextMesh>();
            _textMesh.color = _textColor;
            _textMesh.fontSize = _fontSize;
            _textMesh.anchor = TextAnchor.MiddleCenter;
            _textMesh.alignment = TextAlignment.Center;
            _textMesh.text = "";
            _textObject.SetActive(false);
        }

        private void OnDroneStateChanged(DroneStateChangedEvent evt)
        {
            if (evt.Drone == null || !ReferenceEquals(evt.Drone, _drone))
            {
                return;
            }

            _currentState = evt.NewState;
            
            if (_currentState == DroneState.Collecting)
            {
                _textObject?.SetActive(true);
            }
            else
            {
                _textObject?.SetActive(false);
            }
        }
        
        public void OnUpdate(float deltaTime)
        {
            if (_currentState != DroneState.Collecting || _textObject == null || !_textObject.activeSelf)
            {
                return;
            }

            if (Time.time - _lastProgressUpdateTime < ProgressUpdateInterval)
            {
                return;
            }

            _lastProgressUpdateTime = Time.time;
            UpdateProgress();
            
            // Обновляем поворот к камере
            if (_textObjectTransform != null && _mainCamera != null)
            {
                Transform cameraTransform = _mainCamera.transform;
                _textObjectTransform.LookAt(_textObjectTransform.position + cameraTransform.rotation * Vector3.forward,
                    cameraTransform.rotation * Vector3.up);
            }
        }
        
        private void UpdateProgress()
        {
            if (_stateMachine == null || _stateMachine.StateMachine == null)
            {
                _textMesh.text = "";
                return;
            }
            
            float progress = GetCollectionProgress();
            string progressText = FormatProgress(progress);
            _textMesh.text = progressText;
        }
        
        private float GetCollectionProgress()
        {
            if (_stateMachine?.StateMachine?.CurrentState == null)
            {
                return 0f;
            }
            
            var currentState = _stateMachine.StateMachine.CurrentState;
            
            if (currentState is DroneCollectingState collectingState)
            {
                return collectingState.GetProgress();
            }
            
            return 0f;
        }
        
        private string FormatProgress(float progress)
        {
            int completedSegments = Mathf.FloorToInt(progress * ProgressBars);
            string result = "";
            
            for (int i = 0; i < completedSegments; i++)
            {
                result += ".";
            }
            
            for (int i = completedSegments; i < ProgressBars; i++)
            {
                result += "|";
            }
            
            return result;
        }
    }
}

