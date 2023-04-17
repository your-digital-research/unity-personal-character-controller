using DG.Tweening;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    [SerializeField] private Ease ease;
    [SerializeField] [Range(0, 10)] private float delay;
    [SerializeField] [Range(0, 10)] private float duration;
    [SerializeField] private Vector3 moveDistance;

    private void StartMovement()
    {
        Vector3 currentPosition = transform.position;
        Vector3 endPosition = currentPosition + moveDistance;

        transform
            .DOMove(endPosition, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(delay)
            .SetEase(ease);
    }

    private void Start()
    {
        StartMovement();
    }

    private void Awake()
    {
        //
    }
}