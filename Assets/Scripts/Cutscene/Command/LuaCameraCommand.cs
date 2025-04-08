using System.Threading;
using Cysharp.Threading.Tasks;
using Lua;
using Cutscene.Base;
using UnityEngine;

namespace Cutscene.Command
{
    [LuaObject]
    public partial class LuaCameraCommand : BaseLuaCommand
    {
        private CameraManager _cameraManager;
        
        private Vector3 _cameraPositionOffset; 
        private bool _isInitialized = false;
        
        public LuaCameraCommand(GameObject targetObject) : base(targetObject)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            if (!_isInitialized && TargetObject != null)
            {
                _cameraManager = CameraManager.Instance;
                _cameraPositionOffset = _cameraManager.PositionOffset;
                _isInitialized = true;
            }
        }
        
        /// <summary>
        /// Set camera target.
        /// </summary>
        /// <param name="speed">Camera following speed.</param>
        /// <param name="isAsync">Whether to wait until the target is reached.</param>
        [LuaMember("follow")]
        public async UniTask FollowTarget(float speed, bool isAsync)
        {
            _cameraManager.ChangeCameraTarget(TargetObject.transform, speed);

            if (isAsync) 
                await WaitForCameraToReachTarget(token: CancellationToken.None);
        }
        
        /// Waits until the camera reaches the target position.
        private async UniTask WaitForCameraToReachTarget(CancellationToken token)
        {
            Transform camera = _cameraManager.transform;
            Transform target = TargetObject.transform;
            while (!token.IsCancellationRequested)
            {
                Vector3 cameraPos = camera.position;
                Vector3 targetPos = target.position + _cameraPositionOffset;
                float distance = Vector2.Distance(
                    new Vector2(cameraPos.x, cameraPos.y),
                    new Vector2(targetPos.x, targetPos.y)
                );
                
                if (distance < 0.1f) break;
                await UniTask.Yield();
            }
        }
    }
}