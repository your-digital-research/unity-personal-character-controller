using System.Collections;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody body;

    [Header("Properties")]
    [SerializeField] [Range(0, 25)] private float movementSpeed;
    [SerializeField] [Range(0, 25)] private float rotationSpeed;
    [SerializeField] [Range(0, 1)] private float runningAcceleration;
    [SerializeField] [Range(0, 1)] private float runningDeceleration;
    [SerializeField] [Range(0, 1)] private float movementSpeedBoostPercentageWhileRunning;

    [Header("States")]
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isRunning;

    private float inputAmount;
    private float verticalInput;
    private float horizontalInput;
    private float runningTransition;
    private float currentMovementSpeed;

    private Vector3 moveDirection;
    private Coroutine movementRoutine;
    private Quaternion lastLookRotation;

    public bool IsWalking()
    {
        return isWalking;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public float GetInputAmount()
    {
        return inputAmount;
    }

    public float GetCurrentMovementSpeed()
    {
        return currentMovementSpeed;
    }

    public void StartMovementRoutine()
    {
        if (movementRoutine != null) StopMovementRoutine();
        movementRoutine = StartCoroutine(MovementRoutine());
    }

    public void StopMovementRoutine()
    {
        if (movementRoutine != null) StopMovementRoutine();
        movementRoutine = null;
    }

    private void UpdateInputs()
    {
        // Check if run pressed
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // Get Vertical & Horizontal inputs
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        // Make sure the input doesnt go negative or above 1;
        float inputMagnitude = Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput);
        inputAmount = Mathf.Clamp01(inputMagnitude);

        // Check for walking boolean
        isWalking = (inputAmount > 0 && !isRunning);
    }

    private void UpdateCameraLookRotation(Transform mainCameraTransform, Transform lookAtTransform)
    {
        // Calculate camera look direction & rotation
        Vector3 playerPosition = transform.position;
        Vector3 cameraPosition = mainCameraTransform.position;
        Vector3 cameraLookDirection = (playerPosition - cameraPosition).normalized;
        Quaternion cameraLookRotation = Quaternion.LookRotation(cameraLookDirection);

        // Assign camera look rotation to LookAt transform rotation
        // lookAtTransform.rotation = cameraLookRotation;
        CameraController.Instance.UpdateLookDirection(cameraLookRotation);
    }

    private void UpdateMoveDirection(Transform lookAtTransform)
    {
        // Calculate final inputs based on player input and camera rotation
        Vector3 correctedVertical = lookAtTransform.forward * verticalInput;
        Vector3 correctedHorizontal = lookAtTransform.right * horizontalInput;
        Vector3 combinedInput = correctedVertical + correctedHorizontal;

        // Getting movement direction
        float directionX = combinedInput.normalized.x;
        float directionZ = combinedInput.normalized.z;

        // Assign calculated direction
        moveDirection = new Vector3 (directionX, 0, directionZ);
    }

    private void UpdatePlayerLookRotation()
    {
        // Check for magnitude to avoid warning
        if (moveDirection.magnitude == 0)
        {
            transform.rotation = lastLookRotation;
        }
        else
        {
            // Calculate rotation
            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            Quaternion playerLookRotation = Quaternion.Slerp(currentRotation, targetRotation, inputAmount * rotationSpeed);

            // Assign calculated rotation
            transform.rotation = playerLookRotation;
            lastLookRotation = playerLookRotation;
        }
    }

    private void UpdatePlayerVelocity()
    {
        // Check inputs
        bool isAxisPressed = verticalInput != 0 || horizontalInput != 0;

        // Calculate running transition
        runningTransition += isRunning ? runningAcceleration : -runningDeceleration;
        runningTransition = Mathf.Clamp(runningTransition, 0, 1);

        // Calculate current movement speed
        float runningMovementSpeed = movementSpeed + (movementSpeed * movementSpeedBoostPercentageWhileRunning);
        currentMovementSpeed = Mathf.Lerp(movementSpeed, runningMovementSpeed, runningTransition);

        // Calculate direction
        Vector3 direction = isAxisPressed ? moveDirection : Vector3.zero;

        // Set velocity by multiplying movement direction and movement speed
        body.velocity = direction * (currentMovementSpeed * inputAmount);
    }

    private void Move()
    {
        // Update inputs
        UpdateInputs();

        // Get Main Camera & LookAt transforms
        Transform mainCameraTransform = CameraController.Instance.MainCamera.transform;
        Transform lookAtTransform = CameraController.Instance.LookAtTransform;

        // Update properties
        UpdateCameraLookRotation(mainCameraTransform, lookAtTransform);
        UpdateMoveDirection(lookAtTransform);
        UpdatePlayerLookRotation();
        UpdatePlayerVelocity();
    }

    private IEnumerator MovementRoutine()
    {
        while (true)
        {
            Move();

            yield return new WaitForFixedUpdate();
        }
    }
}