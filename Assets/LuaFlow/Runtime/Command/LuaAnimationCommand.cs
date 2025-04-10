using Cysharp.Threading.Tasks;
using Lua;
using LuaFlow.Base;
using LuaFlow.Interface;
using UnityEngine;

namespace LuaFlow.Command
{
    [LuaObject]
    public partial class LuaAnimationCommand : BaseLuaCommand
    {
        private IAnimationManager _baseAnimation;
        
        private string _animationName;
        private bool _isAnimManagerCached = false;
        
        public LuaAnimationCommand(GameObject targetObject) : base(targetObject)
        {
            return;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            if (!_isAnimManagerCached && TargetObject != null)
            {
                _baseAnimation = TargetObject.GetComponent<IAnimationManager>();
                _isAnimManagerCached = true;
            }
        }
        
        /// <summary>
        /// Execute animation.
        /// </summary>
        /// <param name="animationName">Name of the animation.</param>
        /// <param name="isAsync">Whether to wait until completion.</param>
        [LuaMember("play")]
        public async UniTask PlayAnimation(string animationName, bool isAsync = false)
        {
            _animationName = animationName;
            await _baseAnimation.PlayAnimationAsync(_animationName, isAsync);
        }

        /// <summary>
        /// Flip the character.
        /// </summary>
        /// <param name="isRight">True to turn right, false otherwise.</param>
        [LuaMember("flip")]
        public void Flip(bool isRight)
        {
            _baseAnimation.FlipSprite(isRight);
        }
    }
}