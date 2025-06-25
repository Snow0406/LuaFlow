-- luaflow_api.lua - LuaFlow Unity API Global Definitions

-- ===== Global Functions =====
---@global
---@param name string GameObject name
---@return LuaGameObject GameObject wrapper
get = function(name) end

---@global
---@param message string Log message
log = function(message) end

---@global
---@param duration number Wait duration in seconds
wait = function(duration) end

-- ===== Command Classes =====

---@class LuaAnimationCommand Animation control class
local LuaAnimationCommand = {}

---@param animationName string Animation name
---@param isAsync? boolean Whether to wait until completion (default: false)
function LuaAnimationCommand:play(animationName, isAsync) end

---@param isRight boolean True to turn right, false to turn left
function LuaAnimationCommand:flip(isRight) end

---@class LuaCameraCommand Camera control class
local LuaCameraCommand = {}

---@param speed number Camera following speed
---@param isAsync boolean Whether to wait until target is reached
function LuaCameraCommand:follow(speed, isAsync) end

---@class LuaMovementCommand Movement control class
local LuaMovementCommand = {}

---@param speed number Movement speed to set
function LuaMovementCommand:speed(speed) end

---@param x number Target X position
---@param y number Target Y position
---@param speed? number Movement speed (optional, uses current speed if not specified)
function LuaMovementCommand:to(x, y, speed) end

---@param x number Target X position
---@param y number Target Y position
---@param speed? number Movement speed (optional, uses current speed if not specified)
function LuaMovementCommand:toSync(x, y, speed) end

---@class LuaTransformCommand Transform control class
local LuaTransformCommand = {}

---@param x number X position
---@param y number Y position
function LuaTransformCommand:pos(x, y) end

---@param x number X rotation
---@param y number Y rotation
function LuaTransformCommand:rot(x, y) end

---@param x number X scale
---@param y number Y scale
function LuaTransformCommand:scale(x, y) end

-- ===== Bridge Classes =====

---@class LuaCustomActionBridge Custom action bridge class
local LuaCustomActionBridge = {}

---@param functionName string Function name to execute
---@param parameters? any Function parameters (optional)
function LuaCustomActionBridge:exec(functionName, parameters) end

---@param functionName string Async function name to execute
---@param parameters? any Function parameters (optional)
function LuaCustomActionBridge:execAsync(functionName, parameters) end

---@class LuaCustomEventBridge Custom event bridge class
local LuaCustomEventBridge = {}

---@param eventName string Event name to publish
function LuaCustomEventBridge:exec(eventName) end

---@param eventName string Event name to publish
---@param parameter any Event parameter
function LuaCustomEventBridge:execP(eventName, parameter) end

-- ===== Main LuaGameObject Class =====

---@class LuaGameObject GameObject wrapper class
local LuaGameObject = {}

---@return LuaAnimationCommand Animation control object
function LuaGameObject:anim() end

---@return LuaTransformCommand Transform control object
function LuaGameObject:transform() end

---@return LuaMovementCommand Movement control object
function LuaGameObject:move() end

---@return LuaCameraCommand Camera control object
function LuaGameObject:camera() end

---@return LuaCustomActionBridge Custom action bridge object
function LuaGameObject:action() end

---@return LuaCustomEventBridge Custom event bridge object
function LuaGameObject:event() end

---@param active boolean Active state
function LuaGameObject:active(active) end