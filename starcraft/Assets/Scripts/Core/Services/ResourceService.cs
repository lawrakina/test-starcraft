using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Interfaces;
using DroneResourceCollection.Entities.Resource;
using UnityEngine;

namespace Core.Services
{
    public class ResourceService : IResourceService
    {
        private readonly List<IResource> _resources = new();

        /// <summary>
        /// Проверяет, является ли ресурс валидным (не null и не уничтожен для Unity объектов)
        /// </summary>
        private bool IsResourceValid(IResource resource)
        {
            if (resource == null)
            {
                return false;
            }
            
            // Если ресурс - MonoBehaviour, проверяем, что он не уничтожен
            if (resource is MonoBehaviour resourceMono)
            {
                return resourceMono != null;
            }
            
            // Для других реализаций считаем валидным, если не null
            return true;
        }
        
        /// <summary>
        /// Очищает список ресурсов от уничтоженных объектов
        /// </summary>
        private void CleanupDestroyedResources()
        {
            int removedCount = _resources.RemoveAll(r => !IsResourceValid(r));
            if (removedCount > 0)
            {
                Debug.Log($"[ResourceService] Cleaned up {removedCount} destroyed resources from the list");
            }
        }

        public int AvailableResourceCount
        {
            get
            {
                CleanupDestroyedResources();
                return _resources.Count(r => IsResourceValid(r) && !r.IsReserved && !r.IsCollected);
            }
        }

        public IResource FindNearestAvailableResource(Vector3 position, FactionType? faction = null)
        {
            // Очищаем уничтоженные ресурсы перед поиском
            CleanupDestroyedResources();
            
            // Если указана фракция, фильтруем ресурсы, игнорируя резервации других фракций
            var availableResources = _resources
                .Where(r => 
                {
                    // Проверяем, что ресурс валиден (не уничтожен)
                    if (!IsResourceValid(r))
                    {
                        return false;
                    }
                    
                    if (r.IsCollected)
                        return false;
                    
                    // Если указана фракция, проверяем только резервации этой фракции
                    if (faction.HasValue)
                    {
                        // Ресурс доступен, если он не зарезервирован этой фракцией
                        if (r is Resource resource)
                        {
                            // Дополнительная проверка на уничтожение перед доступом к методам
                            if (!IsResourceValid(resource))
                            {
                                return false;
                            }
                            return !resource.IsReservedByFaction(faction.Value);
                        }
                        // Для других реализаций используем старую логику
                        return !r.IsReserved;
                    }
                    
                    // Если фракция не указана, используем старую логику
                    return !r.IsReserved;
                })
                .ToList();

            if (availableResources.Count == 0)
            {
                return null;
            }

            IResource nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var resource in availableResources)
            {
                // Дополнительная проверка перед обращением к Position
                if (!IsResourceValid(resource))
                {
                    continue;
                }
                
                try
                {
                    float distance = Vector3.Distance(position, resource.Position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = resource;
                    }
                }
                catch (MissingReferenceException)
                {
                    // Ресурс был уничтожен во время итерации - пропускаем его
                    Debug.LogWarning($"[ResourceService] Resource was destroyed during search, skipping");
                    continue;
                }
            }

            return nearest;
        }

        public List<IResource> GetAvailableResources()
        {
            CleanupDestroyedResources();
            return _resources.Where(r => IsResourceValid(r) && !r.IsReserved && !r.IsCollected).ToList();
        }

        public void RegisterResource(IResource resource)
        {
            if (resource != null && !_resources.Contains(resource))
            {
                _resources.Add(resource);
            }
        }

        public void UnregisterResource(IResource resource)
        {
            _resources.Remove(resource);
        }
    }
}

