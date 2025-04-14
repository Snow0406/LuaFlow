using Cysharp.Threading.Tasks;
using LuaFlow.Integration;
using UnityEngine;

/// <summary>
/// This is for testing purposes.
/// </summary>
public class ActionManagerTest : MonoBehaviour
{
    private void Awake()
    {
        LuaCustomActionManager.RegisterFunction("TestVoidAction1", TestVoidAction);
        LuaCustomActionManager.RegisterFunction("TestVoidAction2", () => Debug.Log($"TestVoidAction: Strawberry latte love !"));
        
        LuaCustomActionManager.RegisterFunction<int>("TestIntAction1", TestIntAction);
        LuaCustomActionManager.RegisterFunction("TestIntAction2", (int value) => Debug.Log($"TestIntAction2: Strawberry {value}"));
        
        LuaCustomActionManager.RegisterAsyncFunction<int>("TestIntActionAsync", TestIntActionAsync);
    }

    private void OnDestroy()
    {
        LuaCustomActionManager.UnRegisterFunction("TestVoidAction1");
        LuaCustomActionManager.UnRegisterFunction("TestVoidAction2");
        
        LuaCustomActionManager.UnRegisterFunction("TestIntAction1");
        LuaCustomActionManager.UnRegisterFunction("TestIntAction2");
        
        LuaCustomActionManager.UnRegisterFunction("TestIntActionAsync");
    }

    private void TestVoidAction()
    {
        Debug.Log($"TestVoidAction: Strawberry latte love !");
    }
    
    private void TestIntAction(int value)
    {
        Debug.Log($"TestIntAction1: Strawberry {value}");
    }

    private async UniTask TestIntActionAsync(int value)
    {
        Debug.Log($"TestVoidActionAsync: Start making strawberry latte {value}");
        await UniTask.WaitForSeconds(2f);
        Debug.Log($"TestVoidActionAsync: End making strawberry latte {value}");
    }
}