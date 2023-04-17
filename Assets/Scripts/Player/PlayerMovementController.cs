using System.Collections;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody body;

    [Header("Properties")]
    [SerializeField] [Range(0, 5)] private float floorOffsetY;
    [SerializeField] [Range(0, 25)] private float movementSpeed;
    [SerializeField] [Range(0, 25)] private float rotationSpeed;
    [SerializeField] [Range(0, 1)] private float runningAcceleration;
    [SerializeField] [Range(0, 1)] private float runningDeceleration;
    [SerializeField] [Range(0, 1)] private float movementSpeedBoostPercentageWhileRunning;

    [Header("States")]
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isRunning;
    [SerializeField] private bool isGrounded;

    [Header("Raycasts")]
    [SerializeField] [Range(0, 5)] private float raycastWidth;
    [SerializeField] [Range(0, 5)] private float firstRaycastDistance;
    [SerializeField] [Range(0, 5)] private float secondRaycastDistance;

    [Header("Debug")]
    [SerializeField] private bool drawRaycasts;
    [SerializeField] private bool drawPositionPoints;
    [SerializeField] [Range(0, 5)] private float pointsDrawDuration;
    [SerializeField] private Transform debugContainer;

    private float inputAmount;
    private float verticalInput;
    private float horizontalInput;
    private float runningTransition;
    private float currentMovementSpeed;

    private Vector3 gravity;
    private Vector3 floorMovement;
    private Vector3 moveDirection;
    private Vector3 combinedRaycast;
    private Vector3 raycastFloorPosition;
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

    private Vector3 FindFloor()
    {
        // Width of raycasts around the centre of your character
        float width = raycastWidth;

        // Check floor on 5 raycasts, get the average when not Vector3.zero
        int floorAverage = 1;

        combinedRaycast = FloorRaycasts(0, 0, firstRaycastDistance);
        floorAverage += (GetFloorAverage(width, 0) + GetFloorAverage(-width, 0) +
                         GetFloorAverage(0, width) + GetFloorAverage(0, -width));

        return combinedRaycast / floorAverage;
    }

    // Only add to average floor position if its not Vector3.zero
    private int GetFloorAverage(float offsetX, float offsetZ)
    {
        if (FloorRaycasts(offsetX, offsetZ, firstRaycastDistance) == Vector3.zero) return 0;

        combinedRaycast += FloorRaycasts(offsetX, offsetZ, firstRaycastDistance);

        return 1;
    }

    private Vector3 FloorRaycasts(float offsetX, float offsetZ, float raycastLength)
    {
        // Move raycast
        raycastFloorPosition = transform.TransformPoint(0 + offsetX, 0 + 0.5f, 0 + offsetZ);

        // Debug for raycasts
        if (drawRaycasts) Debug.DrawRay(raycastFloorPosition, Vector3.down * raycastLength, Color.yellow);

        return Physics.Raycast(raycastFloorPosition, Vector3.down, out var hit, raycastLength)
            ? hit.point
            : Vector3.zero;
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

    private void UpdateCameraLookRotation()
    {
        // Get main camera transform for rotation calculation
        Transform mainCameraTransform = CameraController.Instance.GetMainCameraTransform();

        // Calculate camera look direction & rotation
        Vector3 playerPosition = transform.position;
        Vector3 cameraPosition = mainCameraTransform.position;
        Vector3 cameraLookDirection = (playerPosition - cameraPosition).normalized;
        Quaternion cameraLookRotation = Quaternion.LookRotation(cameraLookDirection);

        // Update camera look direction
        CameraController.Instance.UpdateLookRotation(cameraLookRotation);
    }

    private void UpdateMoveDirection()
    {
        // Get camera visual container transform for direction calculation
        Transform cameraVisualContainerTransform = CameraController.Instance.GetVisualContainerTransform();

        // Calculate final inputs based on player input and camera rotation
        Vector3 correctedVertical = cameraVisualContainerTransform.forward * verticalInput;
        Vector3 correctedHorizontal = cameraVisualContainerTransform.right * horizontalInput;
        Vector3 combinedInput = correctedVertical + correctedHorizontal;

        // Getting movement direction
        float directionX = combinedInput.normalized.x;
        float directionZ = combinedInput.normalized.z;
        Vector3 newDirection = new Vector3(directionX, 0, directionZ);

        // Assign calculated direction
        moveDirection = newDirection;
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
            float transition = inputAmount * rotationSpeed;
            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            Quaternion playerLookRotation = Quaternion.Slerp(currentRotation, targetRotation, transition);

            // Assign calculated rotation
            transform.rotation = playerLookRotation;
            lastLookRotation = playerLookRotation;
        }
    }

    private void ApplyGravityAndUpdateRigidbody()
    {
        // If not grounded, increase down force
        if (FloorRaycasts(0, 0, secondRaycastDistance) == Vector3.zero)
        {
            isGrounded = false;
            gravity += Vector3.up * (Physics.gravity.y * Time.fixedDeltaTime);
        }

        // Find the Y position via raycasts
        Vector3 floor = FindFloor();
        Vector3 raycastPosition = new Vector3(body.position.x, floor.y, body.position.z);
        Vector3 floorPosition = new Vector3(body.position.x, floor.y + floorOffsetY, body.position.z);

        // Assign calculated floor position
        floorMovement = floorPosition;

        // Debug for position points
        if (drawPositionPoints)
        {
            Transform debugContainer = DebugController.Instance.DebugContainer;

            GameObject floorPositionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            floorPositionSphere.GetComponent<SphereCollider>().enabled = false;
            floorPositionSphere.GetComponent<Renderer>().material.color = Color.cyan;
            floorPositionSphere.transform.name = "FloorPositionSphere";
            floorPositionSphere.transform.localScale = Vector3.one * 0.1f;
            floorPositionSphere.transform.position = floorPosition;
            floorPositionSphere.transform.parent = debugContainer;
            Destroy(floorPositionSphere, pointsDrawDuration);

            GameObject raycastPositionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            raycastPositionSphere.GetComponent<SphereCollider>().enabled = false;
            raycastPositionSphere.GetComponent<Renderer>().material.color = Color.green;
            raycastPositionSphere.transform.name = "RaycastPositionSphere";
            raycastPositionSphere.transform.localScale = Vector3.one * 0.1f;
            raycastPositionSphere.transform.position = raycastPosition;
            raycastPositionSphere.transform.parent = debugContainer;
            Destroy(raycastPositionSphere, pointsDrawDuration);
        }

        // Only stick to floor when grounded
        if (FloorRaycasts(0, 0, secondRaycastDistance) == Vector3.zero || floorMovement == body.position) return;

        // Move the rigidbody to the floor
        body.MovePosition(floorMovement);
        isGrounded = true;
        gravity.y = 0;
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
        body.velocity = direction * (currentMovementSpeed * inputAmount) + gravity;
    }

    private void Move()
    {
        // Update inputs
        UpdateInputs();

        // Update rotations and directions
        UpdateCameraLookRotation();
        UpdateMoveDirection();
        UpdatePlayerLookRotation();

        // Update Rigidbody and player velocity
        ApplyGravityAndUpdateRigidbody();
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