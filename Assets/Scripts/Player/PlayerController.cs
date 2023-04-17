using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovementController playerMovementController;
    [SerializeField] private PlayerAnimationStateController playerAnimationStateController;

    private void Init()
    {
        StartMovement();
    }

    private void StartMovement()
    {
        playerMovementController.StartMovementRoutine();
        playerAnimationStateController.StartAnimationStateChecking();
    }

    private void StopMovement()
    {
        playerMovementController.StopMovementRoutine();
        playerAnimationStateController.StopAnimationStateChecking();
    }

    private void Start()
    {
        Init();
    }
}