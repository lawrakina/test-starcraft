using UnityEngine;

namespace Data
{
    /// <summary>
    /// ScriptableObject для настройки State Machine дрона через Inspector
    /// Позволяет настраивать условия перехода между состояниями
    /// </summary>
    [CreateAssetMenu(fileName = nameof(DroneStateConfig), menuName = "Drone Resource Collection/Drone State Config")]
    public class DroneStateConfig : ScriptableObject
    {
        [Header("State Transition Conditions")]
        
        [Tooltip("Минимальное количество доступных ресурсов для перехода из Idle в Searching")]
        [SerializeField] private int _minResourcesForSearch = 1;
        
        [Tooltip("Расстояние достижения ресурса для перехода в Collecting")]
        [SerializeField] private float _resourceArrivalDistance = 0.5f;
        
        [Tooltip("Время сбора ресурса в секундах")]
        [SerializeField] private float _collectDuration = 2.0f;
        
        [Tooltip("Расстояние достижения базы для перехода в Unloading")]
        [SerializeField] private float _baseArrivalDistance = 1.0f;
        
        [Tooltip("Время выгрузки ресурса на базе в секундах")]
        [SerializeField] private float _unloadDuration = 0.5f;

        public int MinResourcesForSearch => _minResourcesForSearch;
        public float ResourceArrivalDistance => _resourceArrivalDistance;
        public float CollectDuration => _collectDuration;
        public float BaseArrivalDistance => _baseArrivalDistance;
        public float UnloadDuration => _unloadDuration;
    }
}


