using System;
using Core.Enums;
using Core.Interfaces;
using UnityEngine;

namespace Systems.StateMachine.States
{
    /// <summary>
    /// Состояние поиска ресурса
    /// Дрон ищет ближайший доступный ресурс
    /// </summary>
    public class DroneSearchingState : IDroneState
    {
        private readonly IDrone _drone;
        private readonly IResourceService _resourceService;

        public DroneState StateType => DroneState.Searching;

        public DroneSearchingState(IDrone drone, IResourceService resourceService)
        {
            _drone = drone;
            _resourceService = resourceService;
        }

        public void Enter()
        {
            _drone.ClearTargetResource();
        }

        public void Update()
        {
            // Ищем ресурс с учетом фракции дрона - дроны разных фракций не видят резервации друг друга
            var nearestResource = _resourceService.FindNearestAvailableResource(_drone.Position, _drone.Faction);
            
            if (nearestResource != null)
            {
                // Проверяем, что ресурс все еще валиден перед использованием
                // Если ресурс - MonoBehaviour, проверяем, что он не уничтожен
                if (nearestResource is MonoBehaviour resourceMono)
                {
                    if (resourceMono == null)
                    {
                        // Ресурс был уничтожен между поиском и использованием
                        return;
                    }
                }
                
                // Пытаемся зарезервировать ресурс для фракции дрона
                if (nearestResource is DroneResourceCollection.Entities.Resource.Resource resource)
                {
                    // Дополнительная проверка на уничтожение
                    if (resource == null)
                    {
                        return;
                    }
                    
                    try
                    {
                        if (resource.Reserve(_drone.Id, _drone.Faction))
                        {
                            _drone.SetTargetResource(nearestResource);
                        }
                    }
                    catch (MissingReferenceException)
                    {
                        // Ресурс был уничтожен во время резервации
                        Debug.LogWarning($"[DroneSearchingState] Resource was destroyed during reservation for drone {_drone.Id}");
                        return;
                    }
                }
                else
                {
                    // Для других реализаций используем старый метод (обратная совместимость)
                    try
                    {
                        if (nearestResource.Reserve(_drone.Id))
                        {
                            _drone.SetTargetResource(nearestResource);
                        }
                    }
                    catch (MissingReferenceException)
                    {
                        // Ресурс был уничтожен во время резервации
                        Debug.LogWarning($"[DroneSearchingState] Resource was destroyed during reservation for drone {_drone.Id}");
                        return;
                    }
                }
            }
        }

        public void Exit()
        {
            // При выходе из состояния поиска ничего не делаем
        }
    }
}

