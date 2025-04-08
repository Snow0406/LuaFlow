using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Cutscene.Integration
{
    /// <summary>
    /// Lua script function manager.
    /// This source code is designed based on Unity's Scene Adaptive approach (e.g., Chap1 scene, Player scene, Manager scene, ...) and projects managed with asmdef.
    /// </summary>
    public class LuaCustomActionManager : MonoBehaviour
    {
        public static LuaCustomActionManager Instance { get; private set; }
        
        private class LuaActionWrapper
        {
            public UnityAction NoParam;
            public Action<object> Param;
            public Func<UniTask> AsyncNoParam;
            public Func<object, UniTask> AsyncParam;
        }

        private static readonly Dictionary<string, LuaActionWrapper> ActionMap = 
            new Dictionary<string, LuaActionWrapper>(32, StringComparer.Ordinal);

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

        #region Registration

        /// <summary>
        /// Register function, no parameters.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegisterFunction(string name, UnityAction action)
        {
            if (!ActionMap.TryAdd(name, new LuaActionWrapper { NoParam = action }))
                Debug.LogWarning($"LuaCustomActionManager: Function {name} already exists");
        }
        
        /// <summary>
        /// Register function, one parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegisterFunction(string name, Action<object> action)
        {
            if (!ActionMap.TryAdd(name, new LuaActionWrapper { Param = action }))
                Debug.LogWarning($"LuaCustomActionManager: Function {name} already exists");
        }
        
        /// <summary>
        /// Register asynchronous function, no parameters.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegisterAsyncFunction(string name, Func<UniTask> action)
        {
            if (!ActionMap.TryAdd(name, new LuaActionWrapper { AsyncNoParam = action }))
                Debug.LogWarning($"LuaCustomActionManager: Function {name} already exists");
        }
        
        /// <summary>
        /// Register asynchronous function, one parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegisterAsyncFunction(string name, Func<object, UniTask> action)
        {
            if (!ActionMap.TryAdd(name, new LuaActionWrapper { AsyncParam = action }))
                Debug.LogWarning($"LuaCustomActionManager: Function {name} already exists");
        }
        #endregion

        #region Execution
        
        public void ExecuteFunction(string name, object parameter = null)
        {
            if (!ActionMap.TryGetValue(name, out var wrapper)) 
                return;

            if (parameter == null)
            {
                if (wrapper.NoParam != null)
                {
                    wrapper.NoParam.Invoke();
                    return;
                }
                
                if (wrapper.Param != null)
                {
                    wrapper.Param.Invoke(null);
                    return;
                }
            }
            else
            {
                if (wrapper.Param != null)
                {
                    wrapper.Param.Invoke(parameter);
                    return;
                }
                
                if (wrapper.NoParam != null)
                {
                    wrapper.NoParam.Invoke();
                    return;
                }
            }
            
            Debug.LogWarning($"LuaCustomActionManager: Function {name} found but no compatible delegate exists");
        }
        
        public async UniTask ExecuteAsyncFunction(string name, object parameter = null)
        {
            if (!ActionMap.TryGetValue(name, out var wrapper)) 
                return;

            if (parameter == null)
            {
                if (wrapper.AsyncNoParam != null)
                {
                    await wrapper.AsyncNoParam();
                    return;
                }
                
                if (wrapper.AsyncParam != null)
                {
                    await wrapper.AsyncParam(null);
                    return;
                }
            }
            else
            {
                if (wrapper.AsyncParam != null)
                {
                    await wrapper.AsyncParam(parameter);
                    return;
                }
                
                if (wrapper.AsyncNoParam != null)
                {
                    await wrapper.AsyncNoParam();
                    return;
                }
            }
            
            Debug.LogWarning($"LuaCustomActionManager: Function {name} found but no compatible delegate exists");
        }
        #endregion
        
        public static void UnRegisterFunction(string name)
        {
            ActionMap.Remove(name);
        }
        
        public static void ClearAllFunctions()
        {
            ActionMap.Clear();
        }
    }
}
