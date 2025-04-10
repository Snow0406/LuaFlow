using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lua;
using LuaFlow.Interface;
using UnityEngine;

namespace LuaFlow.Core
{
    /// <summary>
    /// Cutscene management class.
    /// </summary>
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance { get; private set; }
        
        private LuaScriptManager _luaManager;
        
        private bool _isPlayingCutscene = false;
        private CancellationTokenSource _cutsceneCts;

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
            
            _luaManager = GetComponent<LuaScriptManager>();
        }

        /// <summary>
        /// Play cutscene using Lua script.
        /// </summary>
        /// <param name="chapter">Cutscene chapter.</param>
        /// <param name="cutsceneScriptName">Cutscene file name.</param>
        public async UniTask<bool> PlayCutsceneAsync(int chapter, string cutsceneScriptName)
        {
            if (_isPlayingCutscene)
            {
                Debug.LogWarning("A cutscene is already playing.");
                return false;
            }
            
            _isPlayingCutscene = true;
            _cutsceneCts = new CancellationTokenSource();
            
            try
            {
                bool success = await _luaManager.LoadAndRunScriptAsync(chapter, cutsceneScriptName, _cutsceneCts.Token);
                
                if (!success)
                {
                    Debug.LogError($"Failed to load script: {cutsceneScriptName}");
                    return false;
                }
                
                success = await _luaManager.CallFunctionAsync("playCutscene", _cutsceneCts.Token);
                
                return success;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Cutscene was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Cutscene play error: {e.Message}");
                return false;
            }
            finally
            {
                _isPlayingCutscene = false;
                _cutsceneCts?.Dispose();
                _cutsceneCts = null;
            }
        }
    }
}
