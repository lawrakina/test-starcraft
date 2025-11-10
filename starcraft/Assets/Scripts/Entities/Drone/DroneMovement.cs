using System.Collections.Generic;
using Core.Interfaces;
using DroneResourceCollection.Systems.Steering;
using UnityEngine;

namespace Entities.Drone
{
    /// <summary>
    /// Компонент движения дрона
    /// Управляет перемещением дрона по пути с учетом Steering Behaviors
    /// </summary>
    public class DroneMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float arrivalDistance = 0.5f;
        
        private Vector3 _targetPosition;
        private List<Vector3> _currentPath;
        private int _currentPathIndex;
        private Vector3 _velocity;
        private bool _hasTarget;
        
        private SteeringManager _steeringManager;
        private INavigationService _navigationService;
        private IDrone _drone;

        public float Speed 
        { 
            get => speed; 
            set => speed = Mathf.Max(0.1f, value); 
        }

        public bool HasTarget => _hasTarget;
        public Vector3 Velocity => _velocity;

        public void Initialize(IDrone drone, INavigationService navigationService, SteeringManager steeringManager)
        {
            _drone = drone;
            _navigationService = navigationService;
            _steeringManager = steeringManager;
        }

        public void SetTargetPosition(Vector3 position)
        {
            _targetPosition = position;
            _hasTarget = true;
            _currentPathIndex = 0;
            
            if (_navigationService != null && _drone != null)
            {
                _currentPath = _navigationService.CalculatePath(_drone.Position, position);
                
                if (_currentPath == null || _currentPath.Count == 0)
                {
                    _currentPath = new List<Vector3> { position };
                }
            }
            else
            {
                _currentPath = new List<Vector3> { position };
            }
        }

        public void ClearTarget()
        {
            _hasTarget = false;
            _currentPath = null;
            _velocity = Vector3.zero;
        }

        private void Update()
        {
            if (!_hasTarget || _currentPath == null || _currentPath.Count == 0)
            {
                return;
            }

            var currentTarget = _currentPath[_currentPathIndex];
            var direction = (currentTarget - transform.position);
            var distance = direction.magnitude;

            if (distance < arrivalDistance)
            {
                _currentPathIndex++;
                
                if (_currentPathIndex >= _currentPath.Count)
                {
                    ClearTarget();
                    return;
                }
                
                currentTarget = _currentPath[_currentPathIndex];
                direction = (currentTarget - transform.position);
            }

            direction.Normalize();
            Vector3 desiredVelocity = direction * speed;

            Vector3 steeringForce = Vector3.zero;
            if (_steeringManager != null)
            {
                steeringForce = _steeringManager.CalculateSteering(transform.position, _velocity, speed);
            }

            _velocity = desiredVelocity + steeringForce;
            
            if (_velocity.magnitude > speed)
            {
                _velocity = _velocity.normalized * speed;
            }

            if (_velocity.magnitude > 0.01f)
            {
                transform.position += _velocity * Time.deltaTime;
                
                Quaternion targetRotation = Quaternion.LookRotation(_velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        private void OnDrawGizmos()
        {
            if (_currentPath is { Count: > 0 })
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < _currentPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(_currentPath[i], _currentPath[i + 1]);
                }
                
                if (_currentPathIndex < _currentPath.Count)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(_currentPath[_currentPathIndex], 0.3f);
                }
            }
        }
    }
}

