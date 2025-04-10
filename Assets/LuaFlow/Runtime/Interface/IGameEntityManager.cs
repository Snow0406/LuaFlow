using UnityEngine;

namespace LuaFlow.Interface
{
    /// <summary>
    /// Interface LuaScript GameEntityManager
    /// </summary>
    public interface IGameEntityManager
    {
        /// <summary>
        /// Get Game Object.
        /// </summary>
        /// <param name="key">Key of the object to find.</param>
        GameObject GetGameObject(string key);
    }
}