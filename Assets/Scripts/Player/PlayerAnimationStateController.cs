using System.Collections;
using UnityEngine;

public class PlayerAnimationStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovementController playerMovementController;

    private Coroutine animationStateCheckingRoutine;

    private static readonly int VelocityZHash = Animator.StringToHash("Velocity Z");
    private static readonly int IsWalkingHash = Animator.StringToHash("Is Walking");
    private static readonly int IsRunningZHash = Animator.StringToHash("Is Running");

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
        bool isWalking = playerMovementController.IsWalking();
        bool isRunning = playerMovementController.IsRunning();
        float inputAmount = playerMovementController.GetInputAmount();
        float currentMovementSpeed = playerMovementController.GetCurrentMovementSpeed();

        // Set the parameters to our local variable values
        animator.SetBool(IsWalkingHash, isWalking);
        animator.SetBool(IsRunningZHash, isRunning);
        animator.SetFloat(VelocityZHash, inputAmount * currentMovementSpeed);
    }
}