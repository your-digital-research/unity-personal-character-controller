using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Init()
    {
        DOTween.Init();
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