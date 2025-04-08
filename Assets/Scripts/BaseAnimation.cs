using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Base class for animations.
/// This class is used for game objects with complex animations (e.g., player, enemies, etc.).
/// </summary>
public class BaseAnimation : MonoBehaviour
{
    public bool FlipX { get; private set; }

    protected Animator Animator;
    private SpriteRenderer _spriteRenderer;

    protected virtual void Awake()
    {
        Animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Play animation (for cutscenes).
    /// </summary>
    /// <param name="animationName"></param>
    /// <param name="isAsync">true: Waits until completion.</param>
    public async UniTask PlayAnimationAsync(string animationName, bool isAsync)
    {
        // Play animation
        Animator.Play(animationName);

        if (!isAsync) return;

        await UniTask.WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).IsName(animationName));

        var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        var animationLength = stateInfo.length;
        await UniTask.WaitForSeconds(animationLength);
    }

    /// <summary>
    /// Flip the sprite (based on movement direction).
    /// </summary>
    /// <param name="flip">Flip state.</param>
    public void FlipSprite(bool flip)
    {
        _spriteRenderer.flipX = flip;
        FlipX = flip;
    }
}