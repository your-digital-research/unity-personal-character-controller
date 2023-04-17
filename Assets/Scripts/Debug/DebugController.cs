using System;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    public static DebugController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform debugContainer;

    public Transform DebugContainer => debugContainer;

    private void Start()
    {
        //
    }

    private void Awake()
    {
        Instance = this;
    }
}