using System.Collections;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody body;

    [Header("Properties")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;

    [Header("States")]
    [SerializeField] private bool isRunning;

    [Header("Debug Properties")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private Vector3 currentMoveDirection;
    [SerializeField] private Quaternion currentLookDirection;

    private float inputAmount;
    private float verticalInput;
    private float horizontalInput;
    private Vector3 moveDirection;
    private Coroutine movementRoutine;
    private Quaternion lastLookRotation;

    public bool IsRunning()
    {
        return isRunning;
    }

    public float GetInputAmount()
    {
        return inputAmount;
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

            // Check inputs
            bool isAxisPressed = verticalInput != 0 || horizontalInput != 0;

            if (isAxisPressed)
            {
                transform.rotation = playerLookRotation;
                lastLookRotation = playerLookRotation;
            }
            else
            {
                transform.rotation = lastLookRotation;
            }
        }

        // Set debug properties
        currentLookDirection = lastLookRotation;
    }

    private void UpdatePlayerVelocity()
    {
        // Check inputs
        bool isAxisPressed = verticalInput != 0 || horizontalInput != 0;

        // Calculate movement speed
        float speed = isRunning ? movementSpeed * 2 : movementSpeed;

        // Calculate direction
        Vector3 direction = isAxisPressed ? moveDirection : Vector3.zero;

        // Set velocity by multiplying movement direction and movement speed
        body.velocity = direction * (speed * inputAmount);

        // Set debug properties
        currentSpeed = body.velocity.magnitude;
        currentMoveDirection = direction;
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