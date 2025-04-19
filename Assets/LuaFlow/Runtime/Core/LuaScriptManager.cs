using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lua;
using Lua.Unity;
using LuaFlow.Entity;
using LuaFlow.Interface;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LuaFlow.Core
{
    /// <summary>
    /// Lua-CSharp based script manager.
    /// </summary>
    public class LuaScriptManager : MonoBehaviour
    {
        private LuaState _currentState;
        private IGameEntityManager _gameEntityManager;

        private void Start()
        {
            _gameEntityManager = LuaFlowServiceLocator.Get<IGameEntityManager>();
        }

        private void OnDestroy()
        {
            CleanupCurrentState();
        }

        /// <summary>
        /// Load and run Lua script file.
        /// </summary>
        public async UniTask<bool> LoadAndRunScriptAsync(int chapter, string cutsceneName, CancellationToken cancellationToken = default)
        {
            try
            {
                CleanupCurrentState();
                
                _currentState = CreateLuaState();
                
                string scriptContent = await LoadScriptContentAsync(chapter, cutsceneName);
                await _currentState.DoStringAsync(scriptContent, cancellationToken: cancellationToken);
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Lua script load error: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create and initialize a new Lua state.
        /// </summary>
        private LuaState CreateLuaState()
        {
            var state = LuaState.Create();
            
            RegisterUnityFunctions(state);

            return state;
        }

        /// <summary>
        /// Load script content asynchronously.
        /// </summary>
        private async UniTask<string> LoadScriptContentAsync(int chapter, string cutsceneName)
        {
            var cutsceneFilePath = $"Assets/Cutscene/Chap{chapter}/{cutsceneName}.lua";
            
            var cutsceneHandle = Addressables.LoadAssetAsync<LuaAsset>(cutsceneFilePath);
            await cutsceneHandle.Task;
            
            if (cutsceneHandle.Result == null)
            {
                Debug.LogError($"Failed to load Lua script.");
                return null;
            }

            return cutsceneHandle.Result.Text;
        }

        /// <summary>
        /// Register Unity-related functions to Lua.
        /// </summary>
        private void RegisterUnityFunctions(LuaState state)
        {
            state.Environment["wait"] = new LuaFunction(async (context, buffer, ct) => {
                var seconds = context.GetArgument<float>(0);
                await UniTask.WaitForSeconds(seconds, cancellationToken: ct);
                return 0;
            });
            
            state.Environment["log"] = new LuaFunction(async (context, buffer, ct) => {
                var content = context.GetArgument<string>(0);
                Debug.Log(content);
                await UniTask.CompletedTask;
                return 0;
            });
            
            state.Environment["get"] = new LuaFunction(async (context, buffer, ct) => {
                var key = context.GetArgument<string>(0);
        
                GameObject gameObj = _gameEntityManager.GetGameObject(key);
                Debug.Log($"GetGameObject called: {key}, result: {(gameObj != null ? gameObj.name : "null")}");
                if (gameObj != null)
                {
                    buffer.Span[0] = (LuaValue)new LuaGameObject(gameObj);
                    await UniTask.CompletedTask;
                    return 1;
                }
        
                Debug.LogError($"Cannot find game object: {key}");
                await UniTask.CompletedTask;
                return 0;
            });
        }

        /// <summary>
        /// Call a specific function from the script.
        /// </summary>
        public async UniTask<bool> CallFunctionAsync(string functionName, CancellationToken cancellationToken = default, params object[] args)
        {
            if (_currentState == null)
            {
                Debug.LogError("No active Lua state.");
                return false;
            }
            try
            {
                var luaFuncValue = _currentState.Environment[functionName];
                if (luaFuncValue.TryRead<LuaFunction>(out var luaFunction))
                {
                    // Function call
                    LuaValue[] luaArgs = Array.ConvertAll(args, arg => (LuaValue)arg);
                    await luaFunction.InvokeAsync(_currentState, luaArgs, cancellationToken);
                    return true;
                }
                else
                {
                    Debug.LogError($"{functionName} is not a function.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Function call error: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clean up the current script.
        /// </summary>
        public void CleanupCurrentState()
        {
            if (_currentState != null)
            {
                try
                {
                    // This is not tested yet.
                    _currentState.DoStringAsync(@"
                       for k in pairs(_G) do _G[k] = nil end
                    ");
                }
                catch {  }
                
                _currentState = null;
            }
        }
    }
}
