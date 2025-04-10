using Lua;
using LuaFlow.Base;
using UnityEngine;

namespace LuaFlow.Command
{
    [LuaObject]
    public partial class LuaTransformCommand : BaseLuaCommand
    {
        private Transform _transform;
        private bool _isTransformCached = false;
        
        public LuaTransformCommand(GameObject targetObject) : base(targetObject)
        {
            return;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            if (!_isTransformCached && TargetObject != null)
            {
                _transform = TargetObject.transform;
                _isTransformCached = true;
            }
        }
        
        /// <summary>
        /// Sets the Position of the TargetObject.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [LuaMember("setPosition")]
        public void SetPosition(float x, float y)
        {
            if (!_isTransformCached) Initialize();
            _transform.position = new Vector2(x, y);
        }
        
        /// <summary>
        /// Sets the Rotation of the TargetObject.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [LuaMember("setRotation")]
        public void SetRotation(float x, float y)
        {
            if (!_isTransformCached) Initialize();
            _transform.eulerAngles = new Vector2(x, y);
        }
        
        /// <summary>
        /// Sets the Scale of the TargetObject.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [LuaMember("setScale")]
        public void SetScale(float x, float y)
        {
            if (!_isTransformCached) Initialize();
            _transform.localScale = new Vector2(x, y);
        }
    }
}