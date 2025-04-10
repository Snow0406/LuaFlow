using UnityEngine;

namespace LuaFlow.Interface
{
    /// <summary>
    /// Interface LuaScript CameraManager
    /// </summary>
    public interface ICameraManager
    {
        /// <summary>
        /// Camera position offset
        /// </summary>
        Vector3 PositionOffset { get; set; }
        
        /// <summary>
        /// Camera transform
        /// </summary>
        Transform transform { get; }
        
        /// <summary>
        /// Set camera targeting.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="followSpeed"></param>
        void ChangeCameraTarget(Transform target, float followSpeed = 0.1f);
    }
}