using Lua;
using Cutscene.Base;
using UnityEngine;

namespace Cutscene.Integration
{
    [LuaObject]
    public partial class LuaCustomEventBridge : BaseLuaCommand
    {
        public LuaCustomEventBridge(GameObject targetObject) : base(targetObject)
        {
            return;
        }
        
        /// <summary>
        /// Publish event (no parameters).
        /// </summary>
        [LuaMember("exec")]
        public void Publish(string eventName)
        {
            LuaCustomEventManager.Instance.Publish(eventName);
        }
        
        /// <summary>
        /// Publish event with parameters.
        /// </summary>
        [LuaMember("execP")]
        public void PublishWithParam(string eventName, object parameter)
        {
            LuaCustomEventManager.Instance.Publish(eventName, parameter);
        }
    }
}
