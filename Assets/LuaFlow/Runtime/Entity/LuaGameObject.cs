using Lua;
using LuaFlow.Command;
using LuaFlow.Integration;
using UnityEngine;

namespace LuaFlow.Entity
{
    [LuaObject]
    public partial class LuaGameObject
    {
        private readonly GameObject _gameObject;
        
        private LuaAnimationCommand _animation;
        private LuaTransformCommand _transform;
        private LuaMovementCommand _movement;
        private LuaCameraCommand _camera;
        private LuaCustomActionBridge _action;
        private LuaCustomEventBridge _event;

        public LuaGameObject(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        [LuaMember("animation")]
        public LuaAnimationCommand Animation()
        {
            if (_animation == null)
            {
                _animation = new LuaAnimationCommand(_gameObject);
                _animation.Initialize();
            }

            return _animation;
        }

        [LuaMember("transform")]
        public LuaTransformCommand Transform()
        {
            if (_transform == null)
            {
                _transform = new LuaTransformCommand(_gameObject);
                _transform.Initialize();
            }

            return _transform;
        }
        
        [LuaMember("move")]
        public LuaMovementCommand Movement()
        {
            if (_movement == null)
            {
                _movement = new LuaMovementCommand(_gameObject);
                _movement.Initialize();
            }

            return _movement;
        }
        
        [LuaMember("camera")]
        public LuaCameraCommand Camera()
        {
            if (_camera == null)
            {
                _camera = new LuaCameraCommand(_gameObject);
                _camera.Initialize();
            }

            return _camera;
        }

        [LuaMember("action")]
        public LuaCustomActionBridge Action()
        {
            if (_action == null)
            {
                _action = new LuaCustomActionBridge(_gameObject);
                _action.Initialize();
            }

            return _action;
        }

        [LuaMember("event")]
        public LuaCustomEventBridge Event()
        {
            if (_event == null)
            {
                _event = new LuaCustomEventBridge(_gameObject);
                _event.Initialize();
            }

            return _event;
        }
        
        [LuaMember("setActive")]
        public void SetActive(bool active)
        {
            _gameObject.SetActive(active);
        }
    }
}