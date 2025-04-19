using Cysharp.Threading.Tasks;
using Lua;
using LuaFlow.Base;
using UnityEngine;

namespace LuaFlow.Command
{
    [LuaObject]
    public partial class LuaMovementCommand : BaseLuaCommand
    {
        private bool _isInitialized = false;
        private float _speed = 5f;
        private const float DISTANCE_THRESHOLD = 0.01f;
        
        public LuaMovementCommand(GameObject targetObject) : base(targetObject)
        {
        }
        
        public override void Initialize()
        {
            base.Initialize();
            if (!_isInitialized && TargetObject != null)
            {
                _isInitialized = true;
            }
        }
        
        /// <summary>
        /// Set the movement speed of the object.
        /// </summary>
        /// <param name="speed">The speed value to set.</param>
        [LuaMember("speed")]
        private void SetSpeed(float speed)
        {
            _speed = speed;
        }
        
        /// <summary>
        /// Move the object to the specified position asynchronously.
        /// </summary>
        /// <param name="x">Target X position.</param>
        /// <param name="y">Target Y position.</param>
        /// <param name="speed">Movement speed (optional, uses current speed if not specified).</param>
        [LuaMember("to")]
        private async UniTask MoveTo(float x, float y, float speed = -1f)
        {
            if (!_isInitialized && TargetObject != null) 
                Initialize();
            
            float lastSpeed = _speed;
            if (speed >= 0f) 
                _speed = speed;
            
            await MoveWithTransform(_speed, new Vector2(x, y));
            
            _speed = lastSpeed;
        }

        /// <summary>
        /// Move the object to the specified position synchronously (does not wait for completion).
        /// </summary>
        /// <param name="x">Target X position.</param>
        /// <param name="y">Target Y position.</param>
        /// <param name="speed">Movement speed (optional, uses current speed if not specified).</param>
        [LuaMember("toSync")]
        private void MoveToSync(float x, float y, float speed = -1f)
        {
            if (!_isInitialized && TargetObject != null) 
                Initialize();
            
            float lastSpeed = _speed;
            if (speed >= 0f) 
                _speed = speed;
            
            _ = MoveWithTransform(_speed, new Vector2(x, y));
            
            _speed = lastSpeed;
        }
        
        private async UniTask MoveWithTransform(float speed, Vector2 targetPosition)
        {
            Transform transform = TargetObject.transform;
            
            float startDistance = Vector2.Distance(transform.position, targetPosition);
            float currentThreshold = Mathf.Min(DISTANCE_THRESHOLD, startDistance * 0.01f);
            
            while (Vector2.Distance(transform.position, targetPosition) > currentThreshold)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position, 
                    targetPosition,
                    speed * Time.fixedDeltaTime
                );
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            }
            
            transform.position = targetPosition;
        }
    }
}