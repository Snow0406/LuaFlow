using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LuaFlow.Integration
{
    /// <summary>
    /// Lua script function manager
    /// </summary>
    public class LuaCustomActionManager : MonoBehaviour
    {
        public static LuaCustomActionManager Instance { get; private set; }

        #region Wrapper

        private interface IActionExecutor
        {
            void Execute(object parameter);
        }

        private interface IAsyncActionExecutor
        {
            UniTask ExecuteAsync(object parameter);
        }
        
        private class ActionExecutor<T> : IActionExecutor
        {
            private readonly Action<T> _action;
            
            public ActionExecutor(Action<T> action)
            {
                _action = action;
            }
            
            public void Execute(object parameter)
            {
                if (parameter is T typedParam)
                {
                    _action(typedParam);
                    return;
                }
                
                if (TryConvert(parameter, out T converted)) _action(converted);
                else Debug.LogError($"파라미터 타입 변환 실패: {parameter?.GetType()} → {typeof(T)}");
            }
        }
        
        private class NoParamActionExecutor : IActionExecutor
        {
            private readonly Action _action;
            
            public NoParamActionExecutor(Action action)
            {
                _action = action;
            }
            
            public void Execute(object parameter)
            {
                _action();
            }
        }
        
        private class AsyncActionExecutor<T> : IAsyncActionExecutor
        {
            private readonly Func<T, UniTask> _action;
            
            public AsyncActionExecutor(Func<T, UniTask> action)
            {
                _action = action;
            }
            
            public async UniTask ExecuteAsync(object parameter)
            {
                if (parameter is T typedParam)
                {
                    await _action(typedParam);
                    return;
                }
                
                if (TryConvert(parameter, out T converted)) await _action(converted);
                else Debug.LogError($"파라미터 타입 변환 실패: {parameter?.GetType()} → {typeof(T)}");
            }
        }
        
        private class NoParamAsyncActionExecutor : IAsyncActionExecutor
        {
            private readonly Func<UniTask> _action;
            
            public NoParamAsyncActionExecutor(Func<UniTask> action)
            {
                _action = action;
            }
            
            public async UniTask ExecuteAsync(object parameter)
            {
                await _action();
            }
        }
        
        private static bool TryConvert<T>(object value, out T result)
        {
            result = default;
            if (value == null) return !typeof(T).IsValueType;
                
            try
            {
                result = (T)Convert.ChangeType(value, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        private static readonly Dictionary<string, IActionExecutor> SyncActions = 
            new Dictionary<string, IActionExecutor>(16, StringComparer.Ordinal);
            
        private static readonly Dictionary<string, IAsyncActionExecutor> AsyncActions = 
            new Dictionary<string, IAsyncActionExecutor>(8, StringComparer.Ordinal);

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
        
        private void OnDestroy()
        {
            if (Instance != null) Instance = null;
            ClearAllFunctions();
        }

        #region Registration

        /// <summary>
        /// Register function, no parameters.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegisterFunction(string name, Action action)
        {
            if (SyncActions.ContainsKey(name))
                Debug.LogWarning($"LuaCustomActionManager: Function {name} already exists");
            else
                SyncActions[name] = new NoParamActionExecutor(action);
        }
        
        /// <summary>
        /// Register function, one parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegisterFunction<T>(string name, Action<T> action)
        {
            if (SyncActions.ContainsKey(name))
                Debug.LogWarning($"LuaCustomActionManager: Function {name} already exists");
            else
                SyncActions[name] = new ActionExecutor<T>(action);
        }
        
        /// <summary>
        /// Register asynchronous function, no parameters.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegisterAsyncFunction(string name, Func<UniTask> action)
        {
            if (AsyncActions.ContainsKey(name))
                Debug.LogWarning($"LuaCustomActionManager: Function {name} already exists");
            else
                AsyncActions[name] = new NoParamAsyncActionExecutor(action);
        }
        
        /// <summary>
        /// Register asynchronous function, one parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegisterAsyncFunction<T>(string name, Func<T, UniTask> action)
        {
            if (AsyncActions.ContainsKey(name))
                Debug.LogWarning($"LuaCustomActionManager: Function {name} already exists");
            else
                AsyncActions[name] = new AsyncActionExecutor<T>(action);
        }
        
        #endregion

        #region Execution
        
        public void ExecuteFunction(string name, object parameter = null)
        {
            if (SyncActions.TryGetValue(name, out var executor))
            {
                try
                {
                    executor.Execute(parameter);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"LuaCustomActionManager: Function execution error ({name}): {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"LuaCustomActionManager: Unregistered function: {name}");
            }
        }

        public async UniTask ExecuteAsyncFunction(string name, object parameter = null)
        {
            if (AsyncActions.TryGetValue(name, out var executor))
            {
                try
                {
                    await executor.ExecuteAsync(parameter);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"LuaCustomActionManager: Async function execution error ({name}): {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"LuaCustomActionManager: Unregistered async function: {name}");
            }
        }
        
        #endregion
        
        /// <summary>
        /// Unregister function for Lua scripts
        /// </summary>
        /// <param name="name"></param>
        public static void UnRegisterFunction(string name)
        {
            SyncActions.Remove(name);
            AsyncActions.Remove(name);
        }

        private static void ClearAllFunctions()
        {
            SyncActions.Clear();
            AsyncActions.Clear();
        }
    }
}
