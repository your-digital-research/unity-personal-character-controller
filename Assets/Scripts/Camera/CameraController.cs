using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CinemachineFreeLook mainCamera;
    [SerializeField] private GameObject visualContainer;

    public Transform GetMainCameraTransform()
    {
        return mainCamera.transform;
    }

    public Transform GetVisualContainerTransform()
    {
        return visualContainer.transform;
    }

    public void UpdateLookRotation(Quaternion newRotation)
    {
        visualContainer.transform.rotation = newRotation;
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