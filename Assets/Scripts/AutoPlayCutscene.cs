using Cysharp.Threading.Tasks;
using LuaFlow.Core;
using UnityEngine;

/// <summary>
///     This is for testing purposes.
///     Please delete this and use the CutsceneTrigger class instead.
/// </summary>
public class AutoPlayCutscene : MonoBehaviour
{
    [SerializeField] private int chapter;
    [SerializeField] private string cutsceneName;

    private void Start()
    {
        AutoPlay().Forget();
    }

    private async UniTaskVoid AutoPlay()
    {
        Debug.Log("Auto Play Start");
        await UniTask.WaitForSeconds(2.0f);
        _ = CutsceneManager.Instance.PlayCutsceneAsync(chapter, cutsceneName);
    }
}