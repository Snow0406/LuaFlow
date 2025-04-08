using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Lua;
using Cutscene.Base;
using UnityEngine;

namespace Cutscene.Integration
{
    /// <summary>
    /// Wrapper class to allow calling custom actions from Lua.
    /// </summary>
    [LuaObject]
    public partial class LuaCustomActionBridge : BaseLuaCommand
    {
        public LuaCustomActionBridge(GameObject targetObject) : base(targetObject)
        {
            return;
        }
        
        /// <summary>
        /// Execute custom action.
        /// </summary>
        [LuaMember("exec")]
        public void ExecuteAction(string functionName, [CanBeNull] object parameters = null)
        {
            LuaCustomActionManager.Instance.ExecuteFunction(functionName, parameters);
        }
        
        /// <summary>
        /// Execute asynchronous action.
        /// </summary>
        [LuaMember("execAsync")]
        public async UniTask ExecuteAsyncAction(string functionName, [CanBeNull] object parameters = null)
        {
            await LuaCustomActionManager.Instance.ExecuteAsyncFunction(functionName, parameters);
        }
    }
}
