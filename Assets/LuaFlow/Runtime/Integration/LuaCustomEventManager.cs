using System;
using System.Collections.Generic;
using UnityEngine;

namespace LuaFlow.Integration
{
    /// <summary>
    /// Lua script event manager.
    /// This source code is designed based on Unity's Scene Adaptive approach (e.g., Chap1 scene, Player scene, Manager scene, ...) and projects managed with asmdef.
    /// </summary>
    public class LuaCustomEventManager : MonoBehaviour
    {
        public static LuaCustomEventManager Instance { get; private set; }
        
        private static readonly Dictionary<string, Action> Events = 
            new Dictionary<string, Action>(32, StringComparer.Ordinal);
        
        private static readonly Dictionary<string, List<Delegate>> ParameterizedEvents = 
            new Dictionary<string, List<Delegate>>(32, StringComparer.Ordinal);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Register event, no parameters.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="callback"></param>
        public static void Subscribe(string eventName, Action callback)
        {
            if (callback == null) return;

            if (!Events.TryAdd(eventName, callback)) 
                Events[eventName] += callback;
        }
        
        /// <summary>
        /// Register event, with parameters.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public static void Subscribe<T>(string eventName, Action<T> callback)
        {
            if (callback == null) return;
                
            if (!ParameterizedEvents.TryGetValue(eventName, out var delegates))
            {
                delegates = new List<Delegate>(4);
                ParameterizedEvents[eventName] = delegates;
            }
            
            delegates.Add(callback);
        }
        
        /// <summary>
        /// Unsubscribe from event, no parameters.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="callback"></param>
        public static void Unsubscribe(string eventName, Action callback)
        {
            if (callback == null || !Events.ContainsKey(eventName)) return;
                
            Events[eventName] -= callback;

            if (Events[eventName] == null)
                Events.Remove(eventName);
        }
        
        /// <summary>
        /// Unsubscribe from event, one parameter.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public static void Unsubscribe<T>(string eventName, Action<T> callback)
        {
            if (callback == null || !ParameterizedEvents.TryGetValue(eventName, out var delegates))
                return;
                
            delegates.Remove(callback);

            if (delegates.Count == 0)
                ParameterizedEvents.Remove(eventName);
        }
        
        /// <summary>
        /// Invoke event for LuaCustomEventBridge, no parameters.
        /// </summary>
        /// <param name="eventName"></param>
        public void Publish(string eventName)
        {
            if (!Events.TryGetValue(eventName, out var action) || action == null) 
                return;
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"LuaCustomEventManager - Publish Error ({eventName}): {ex.Message}");
            }
        }

        /// <summary>
        /// Invoke event for LuaCustomEventBridge, with parameters.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="parameter"></param>
        public void Publish<T>(string eventName, T parameter)
        {
            if (!ParameterizedEvents.TryGetValue(eventName, out var delegates))
                return;
            
            var count = delegates.Count;
            for (int i = 0; i < count; i++)
            {
                var del = delegates[i];
                if (del is Action<T> typedAction)
                {
                    try
                    {
                        typedAction(parameter);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"LuaCustomEventManager - Publish<T> Error ({eventName}): {ex.Message}");
                    }
                }
            }
        }

        public void ClearAllEvents()
        {
            Events.Clear();
            ParameterizedEvents.Clear();
        }
    }
}
