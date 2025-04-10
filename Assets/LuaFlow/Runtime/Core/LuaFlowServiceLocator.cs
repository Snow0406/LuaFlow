using System;
using System.Collections.Generic;
using UnityEngine;

namespace LuaFlow.Core
{
    /// <summary>
    /// Central registry for accessing various manager instances
    /// </summary>
    public static class LuaFlowServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        /// <summary>
        /// Register a manager instance.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            
            if (Services.ContainsKey(type))
            {
                Debug.LogWarning($"An instance of service type {type.Name} is already registered. Replacing the existing instance.");
            }
            
            Services[type] = service;
            Debug.Log($"Service {type.Name} registration completed");
        }

        /// <summary>
        /// Get a registered manager instance.
        /// </summary>
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            
            if (!Services.TryGetValue(type, out var service))
            {
                Debug.LogError($"Service {type.Name} could not be found. Make sure the service is registered first.");
                return null;
            }
            
            return (T)service;
        }

        /// <summary>
        /// Unregister a manager instance.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            Type type = typeof(T);
            
            if (Services.ContainsKey(type))
            {
                Services.Remove(type);
                Debug.Log($"Service {type.Name} unregistration completed");
            }
        }

        /// <summary>
        /// Clear all registered manager instances.
        /// </summary>
        public static void Clear()
        {
            Services.Clear();
            Debug.Log("All services unregistered successfully");
        }
    }
}
