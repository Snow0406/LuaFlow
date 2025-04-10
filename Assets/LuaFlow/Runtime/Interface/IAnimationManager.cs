using Cysharp.Threading.Tasks;

namespace LuaFlow.Interface
{
    /// <summary>
    /// Interface LuaScript AnimationManager
    /// </summary>
    public interface IAnimationManager
    {
        /// <summary>
        /// Play animation (for cutscenes).
        /// </summary>
        /// <param name="animationName"></param>
        /// <param name="isAsync">true: Waits until completion.</param>
        UniTask PlayAnimationAsync(string animationName, bool isAsync);

        /// <summary>
        /// Flip the sprite (based on movement direction).
        /// </summary>
        /// <param name="flip">Flip state.</param>
        void FlipSprite(bool flip);
    }
}