using System.Collections;
using UnityEngine;

public class PlayerAnimationStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovementController playerMovementController;

    private Coroutine animationStateCheckingRoutine;
    private static readonly int VelocityZHash = Animator.StringToHash("Velocity Z");

    public void StartAnimationStateChecking()
    {
        if (animationStateCheckingRoutine != null) StopCoroutine(animationStateCheckingRoutine);
        animationStateCheckingRoutine = StartCoroutine(AnimationStateCheckingRoutine());
    }

    public void StopAnimationStateChecking()
    {
        if (animationStateCheckingRoutine != null) StopCoroutine(animationStateCheckingRoutine);
        animationStateCheckingRoutine = null;
    }

    private IEnumerator AnimationStateCheckingRoutine()
    {
        while (true)
        {
            UpdateState();

            yield return new WaitForEndOfFrame();
        }
    }
    private void UpdateState()
    {
        // Get running state and input amount
        bool isRunning = playerMovementController.IsRunning();
        float inputAmount = playerMovementController.GetInputAmount();

        // Set the parameters to our local variable values
        animator.SetFloat(VelocityZHash, isRunning ? inputAmount * 2 : inputAmount);
    }
}