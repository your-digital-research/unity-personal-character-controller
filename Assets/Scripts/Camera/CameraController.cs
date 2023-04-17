using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CinemachineFreeLook mainCamera;
    [SerializeField] private Transform lookAtTransform;

    [Header("Debug Properties")]
    [SerializeField] private Quaternion currentLookDirection;

    public CinemachineFreeLook MainCamera => mainCamera;
    public Transform LookAtTransform => lookAtTransform;

    public void UpdateLookDirection(Quaternion newRotation)
    {
        lookAtTransform.rotation = newRotation;

        // Set debug properties
        currentLookDirection = newRotation;
    }

    private void Init()
    {
        //
    }

    private void Start()
    {
        Init();
    }

    private void Awake()
    {
        Instance = this;
    }
}