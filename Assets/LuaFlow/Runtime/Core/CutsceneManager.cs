using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LuaFlow.Core
{
    /// <summary>
    /// Cutscene management class.
    /// </summary>
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance { get; private set; }

        public event Action OnCutsceneCompleted;
        
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
                Debug.LogWarning("[<color=#83b3f6>LuaFlow</color>] A cutscene is already playing.");
                return false;
            }
            
            _isPlayingCutscene = true;
            _cutsceneCts = new CancellationTokenSource();
            
            try
            {
                bool success = await _luaManager.LoadAndRunScriptAsync(chapter, cutsceneScriptName, _cutsceneCts.Token);
                
                if (!success)
                {
                    Debug.LogError($"[<color=#83b3f6>LuaFlow</color>] Failed to load script: {cutsceneScriptName}");
                    return false;
                }
                
                success = await _luaManager.CallFunctionAsync("playCutscene", _cutsceneCts.Token);
                
                return success;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[<color=#83b3f6>LuaFlow</color>] Cutscene play error: {e.Message}");
                return false;
            }
            finally
            {
                _isPlayingCutscene = false;
                _cutsceneCts?.Dispose();
                _cutsceneCts = null;
                OnCutsceneCompleted?.Invoke();
            }
        }
        
        /// <summary>
        /// Skip Cutscene
        /// </summary>
        /// <param name="isAsync">Whether to wait until completion.</param>
        public async UniTask SkipCutscene(bool isAsync = false)
        {
            if (!_isPlayingCutscene || _cutsceneCts == null || _cutsceneCts.IsCancellationRequested) return;
            _cutsceneCts.Cancel();

            if (isAsync)
            {
                using var timeoutCts = new CancellationTokenSource(5000);
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    timeoutCts.Token,
                    this.GetCancellationTokenOnDestroy()
                );

                try
                {
                    var tcs = new UniTaskCompletionSource<bool>();

                    Action onCompleted = null;
                    onCompleted = () =>
                    {
                        OnCutsceneCompleted -= onCompleted;
                        tcs.TrySetResult(true);
                    };
                    
                    OnCutsceneCompleted += onCompleted;
                    await tcs.Task.AttachExternalCancellation(combinedCts.Token);
                }
                catch (OperationCanceledException)
                {
                    if (timeoutCts.IsCancellationRequested)
                    {
                        Debug.LogWarning("[<color=#83b3f6>LuaFlow</color>] Cutscene cancel timeout!");
                    }
                }
            }
        }
    }
}
