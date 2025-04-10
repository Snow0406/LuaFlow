using UnityEngine;

namespace LuaFlow.Core
{
    /// <summary>
    /// Trigger cutscene when player enters the trigger area.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class CutsceneTrigger : MonoBehaviour
    {
        [SerializeField] private int chapter;
        [SerializeField] private string cutsceneName;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _ = CutsceneManager.Instance.PlayCutsceneAsync(chapter, cutsceneName);
            gameObject.GetComponent<CircleCollider2D>().enabled = false; // Disable after use to make it one-time only
        }
    }
}