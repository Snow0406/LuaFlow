using UnityEngine;

namespace Cutscene.Base
{
    public interface ILuaCommand
    {
        void Initialize();
    }

    /// <summary>
    /// Base Lua action class.
    /// </summary>
    public class BaseLuaCommand : ILuaCommand
    {
        protected readonly GameObject TargetObject;
        
        public BaseLuaCommand(GameObject targetObject)
        {
            TargetObject = targetObject;
        }
        
        public virtual void Initialize()
        {
            if (TargetObject == null)
            {
                Debug.LogError("The target object for the action is missing.");
            }
        }
    }
}