<div align="center">

## LuaFlow

[![License: MIT](https://img.shields.io/badge/License-MIT-skyblue.svg?style=for-the-badge&logo=github)](LICENSE)

[Korean Docs](README_KR.md)

LuaFlow is a Lua script-based Unity cutscene system built with [UniTask](https://github.com/Cysharp/UniTask) and [Lua-CSharp](https://github.com/kevthehermit/Lua-CSharp).

> This system is taken from part of the [Lilium](https://hyuki.dev/project/lilium/) project!

</div>

---

## Motivation

The motivation behind LuaFlow came from the frustration with JSON-based cutscene management. Managing cutscenes in Unity's inspector was just too cumbersome and complex, so I looked for alternatives and found Lua scripting and JSON. I thought using Lua for cutscene management would be really stylish, so I created this system !!

### Architectural Background

LuaFlow is from the [Lilium](https://hyuki.dev/project/lilium/) project, which employs a Scene Adaptive architecture. This approach separates game elements into different scenes (e.g., Chapter-specific scenes, Player scene, Manager scene) that can be loaded and unloaded independently.

This architecture influences LuaFlow's design in several key ways:

- **Singleton Pattern**: Managers like `GameEntityManager`, `CameraManager`, and `LuaCustomActionManager` use singletons for consistent access across different scenes.

- **Centralized Entity Management**: Since different scenes contain different game objects, the `GameEntityManager` provides a way to register and access objects across scene boundaries.

## Cutscene Example from Lilium Project

Here's an actual cutscene from the [Lilium](https://hyuki.dev/project/lilium/) project using LuaFlow:

```lua
--- Chap1-01 -> Chap1-02 Cutscene

-- Get references to required game objects
local player = get("Player")
local camera1 = get("CameraMove_1")
local camera2 = get("CameraMove_2")

function playCutscene()
    -- Follow camera1 at 0.5 speed, wait until arrival
    camera1:camera():follow(0.5, true)

    -- Screen fade in
    camera1:cinematic():fadeIn()
    camera2:camera():follow(0.03, true)

    -- Execute player movement stop function
    player:action():exec("PlayerMoveStop")

    -- Invoke chapter transition event
    player:event():exec("MovePlayerToNextChapter")

    -- Flip player direction to right
    player:animation():flip(true)
    player:camera():follow(1, true)

    -- Play player fall animation
    player:animation():play("FallDown")

    -- Screen fade out
    player:cinematic():fadeOut()

    -- Wait 3 seconds
    wait(3.0)

    -- Play player getting up animation and wait until completion
    player:animation():play("GetUp", true)
    player:animation():play("Idle")

    -- Re-enable player control function
    player:action():exec("PlayerMoveStart")
end
```

## Getting Started

### Installation

1. Clone this repository or download it as a ZIP file
2. Add the `Assets/Scripts/Cutscene/` folder from this project to your desired Unity project
3. Install the required packages:

#### Installing UniTask

> UniTask provides a more efficient alternative to Unity's coroutines, with better performance and cleaner syntax.

1. Open the Package Manager window (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`
5. Click "Add"

#### Installing NuGetForUnity

> NuGetForUnity allows you to use NuGet packages directly in your Unity projects.

1. Open the Package Manager window (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter `https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity`
5. Click "Add"

#### Installing Lua-CSharp

> Lua-CSharp enables seamless integration between C# and Lua!

**You need to download from both NuGetForUnity and Git URL for complete functionality.**

**Step 1: Install via NuGetForUnity**

1. After installing NuGetForUnity, open Unity menu and select NuGet > Manage NuGet Packages
2. In the search box, type "LuaCSharp"
3. Find the "LuaCSharp" package in the search results
4. Click the Install button
5. Verify that the package has been applied to your project after installation is complete

**Step 2: Install via Git URL**

1. Open the Package Manager window (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter `https://github.com/nuskey8/Lua-CSharp.git?path=src/Lua.Unity/Assets/Lua.Unity`
5. Click "Add"

> Both installation methods are required to ensure all dependencies and Unity-specific implementations are properly configured.

### Project Structure

```
Assets/
├── Scripts/
│   ├── Cutscene/
│   │   ├── Base/            # Base interfaces and classes
│   │   ├── Command/         # Command implementations (Animation, Camera, ...etc)
│   │   ├── Core/            # Core functionality (CutsceneManager, LuaScriptManager)
│   │   ├── Entity/          # Game entity wrappers
│   │   └── Integration/     # Custom action and event system
│   ├── BaseAnimation.cs     # Animation utility
│   ├── CameraManager.cs     # Camera control system
│   └── GameEntityManager.cs # Entity registration and retrieval
├── Cutscene/
│   └── Chap{X}/             # Lua script files organized by chapters
│       └── {cutscene_name}.lua
```

## Usage

### Game Entity Management

> The `GameEntityManager` provides a centralized way to register, access, and manage game objects across different scenes.

```csharp
// Register a game object (typically in Awake or Start)
GameEntityManager.RegisterGameObject("Player", playerGameObject);

// Get a registered game object (can be called from anywhere)
GameObject player = GameEntityManager.Instance.GetGameObject("Player");

// Unregister a game object when no longer needed
GameEntityManager.UnregisterGameObject("Player");

// Clear all registered objects (e.g., when changing scenes)
GameEntityManager.Instance.RemoveAllEntities();
```

In Lua scripts, you can access registered game objects using the `get` function:

```lua
-- Get a registered game object in Lua
local player = get("Player")
```

### Custom Events System

> The `LuaCustomEventManager` provides an event system that enables events between C# scripts and Lua scripts.

**In C#:**

```csharp
// Subscribe to an event without parameters
LuaCustomEventManager.Subscribe("PlayerDied", () => {
    Debug.Log("Player died event received!");
});

// Subscribe to an event with a parameter
LuaCustomEventManager.Subscribe<int>("ScoreChanged", (score) => {
    Debug.Log($"Score changed to: {score}");
});

// Unsubscribe from events
LuaCustomEventManager.Unsubscribe("PlayerDied", myCallback);
LuaCustomEventManager.Unsubscribe<int>("ScoreChanged", myScoreCallback);
```

**In Lua:**

```lua
-- Publish an event without parameters
myObject:event():exec("PlayerDied")

-- Publish an event with a parameter
myObject:event():execP("ScoreChanged", 100)
```

### Custom Actions System

> The `LuaCustomActionManager` provides a system that lets you register C# functions that can be called from Lua scripts.

**In C#:**

```csharp
// Register a simple function with no parameters
LuaCustomActionManager.RegisterFunction("ShowGameOver", () => {
    gameOverPanel.SetActive(true);
});

// Register a function with a parameter
LuaCustomActionManager.RegisterFunction("UpdateHealth", (object healthObj) => {
    if (healthObj is int health) {
        playerHealth.SetHealth(health);
    }
});

// Register an async function (using UniTask)
LuaCustomActionManager.RegisterAsyncFunction("FadeToBlack", async () => {
    await fadeScreen.FadeToBlackAsync(2.0f);
});

// Unregister a function when no longer needed
LuaCustomActionManager.UnRegisterFunction("ShowGameOver");
```

**In Lua:**

```lua
-- Execute a registered function with no parameters
myObject:action():exec("ShowGameOver")

-- Execute a registered function with a parameter
myObject:action():exec("UpdateHealth", 50)

-- Execute an async function (will wait for completion if second parameter is true)
myObject:action():execAsync("FadeToBlack", true)
```

### Creating a Cutscene

1. Create a new Lua script in the `Assets/Cutscene/Chap{X}/` directory
2. Use the following template to start:

```lua
--- Test Cutscene

-- Get references to game objects registered in GameEntityManager
local tg1 = get("Target1")
local tg2 = get("Target2")

-- Main cutscene function
function playCutscene()
    log("Test Cutscene Start")

    -- Wait 2 seconds
    wait(2.0)

    -- camera smoothly transition to follow Target1
    tg1:camera():follow(0.03, true)

    -- camera transition to follow Target2
    tg2:camera():follow(0.03, true)

    log("Test Cutscene End")
end
```

### Triggering a Cutscene

You can trigger cutscenes using the `CutsceneTrigger` component:

1. Create an empty GameObject in your scene
2. Add a CircleCollider2D component
3. Add the `CutsceneTrigger` script
4. Set the Chapter and Cutscene Name fields
5. When the player enters the trigger area, the cutscene will play

### Extending the System

> LuaFlow was designed with extensibility in mind. Adding new functionality is as simple as creating a new command class.

You can extend LuaFlow by creating new command classes:

1. Create a new class that inherits from `BaseLuaCommand`
2. Apply the `[LuaObject]` attribute
3. Implement your methods with the `[LuaMember]` attribute
4. Register your class in the appropriate entity wrapper

Example:

```csharp
[LuaObject]
public partial class LuaDialogueCommand : BaseLuaCommand
{
    public LuaDialogueCommand(GameObject targetObject) : base(targetObject)
    {
    }

    [LuaMember("say")]
    public void Say(string text)
    {
        // Implementation
    }
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

> This license permits commercial use, modification, distribution, and private use while providing limited liability and warranty.

---

Made with ♥ by hy
