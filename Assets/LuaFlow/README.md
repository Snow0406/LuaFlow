<div align="center">

## LuaFlow

[![License: MIT](https://img.shields.io/badge/License-MIT-skyblue.svg?style=for-the-badge&logo=github)](LICENSE)
![GitHub Repo stars](https://img.shields.io/github/stars/snow0406/luaFlow?style=for-the-badge&logo=github&color=%23ef8d9d)

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

- **Interface-based Design**: System components interact through interfaces rather than concrete implementations.

- **Service Locator Pattern**: Access various manager instances through `LuaFlowServiceLocator`.

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

You can install LuaFlow through UPM (Unity Package Manager):

1. Open Package Manager window (Window > Package Manager)
2. Click the "+" button in the upper-left corner
3. Select "Add package from git URL..."
4. Enter https://github.com/Snow0406/LuaFlow.git?path=Assets/LuaFlow
5. Click "Add"

Package dependencies (Lua-CSharp and UniTask) are defined in package.json and will be installed automatically.

> [Nuget Lua-CSharp v0.4.2](https://github.com/nuskey8/Lua-CSharp) is included as dll files.

### Project Structure

```
Assets/
├── Cutscene/
    └── Chap{X}/             # Lua script files organized by chapter
        └── {cutscene_name}.lua

LuaFlow/
├── Runtime/
│   ├── Base/            # Basic interfaces and classes
│   ├── Command/         # Command implementations (animation, camera, etc.)
│   ├── Core/            # Core functionality
│   ├── Entity/          # Game entity wrappers
│   ├── Integration/     # Custom action and event systems
│   ├── Interface/       # System component interfaces

```

## Architecture

### Interface-based Design

> `LuaFlow` provides high flexibility and extensibility through interface-based design.

1. **Interface Definition:**
```csharp
// Interface/ICameraManager.cs
public interface ICameraManager
{
    Vector3 PositionOffset { get; set; }
    Transform transform { get; }
    void ChangeCameraTarget(Transform target, float followSpeed = 0.1f);
}
```

2. **Implementation Class:**
```csharp
// CameraManager.cs
public class CameraManager : MonoBehaviour, ICameraManager
{
    public static CameraManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LuaFlowServiceLocator.Register<ICameraManager>(this);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // ICameraManager interface implementation
    // ...
}
```

This approach makes other parts depend on interfaces rather than concrete classes, making them easier to change or replace.

### Service Locator Pattern

> `LuaFlowServiceLocator` provides a central registry for accessing various manager instances. <br/>
> This reduces coupling between command classes and managers.

1. **Manager Registration:**

```csharp
// In the manager class's Awake method
LuaFlowServiceLocator.Register<ICameraManager>(this);

// Unregister when the manager is destroyed
private void OnDestroy()
{
LuaFlowServiceLocator.Unregister<ICameraManager>();
}
```

2. **Manager Usage:**

```csharp
// Access the registered manager instance from anywhere
ICameraManager cameraManager = LuaFlowServiceLocator.Get<ICameraManager>();
cameraManager.ChangeCameraTarget(targetTransform, 0.5f);
```

## Usage

### Game Entity Management

> The `GameEntityManager` provides a centralized way to register, access, and manage game objects across different scenes. <br/>
> You need to implement this directly. [Example]()

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
LuaCustomActionManager.RegisterFunction("UpdateHealth", (int health) => {
    playerHealth.SetHealth(health);
});

private void UpdateHealth(int health) 
{
    playerHealth.SetHealth(health);
}

LuaCustomActionManager.RegisterFunction<int>("UpdateHealth", UpdateHealth);

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

## Extending the System

### Adding Custom Commands

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

### Creating Custom Managers

1. **Define a New Interface:**
```csharp
// Interface/IDialogueManager.cs
public interface IDialogueManager
{
    void ShowDialogue(string text);
    void HideDialogue();
    bool IsDialogueActive { get; }
}
```

2. **Implement the Interface:**
```csharp
// DialogueManager.cs
public class DialogueManager : MonoBehaviour, IDialogueManager
{
    public static DialogueManager Instance { get; private set; }
    
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text dialogueText;
    
    public bool IsDialogueActive => dialoguePanel.activeSelf;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LuaFlowServiceLocator.Register<IDialogueManager>(this);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ShowDialogue(string text)
    {
        // ...
    }
    
    public void HideDialogue()
    {
        // ...
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            LuaFlowServiceLocator.Unregister<IDialogueManager>();
        }
    }
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Made with ♥ by hy
