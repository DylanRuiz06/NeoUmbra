using System;
using UnityEngine;

public class DimensionManager : MonoBehaviour
{
    public static DimensionManager Instance { get; private set; }

    public event Action<int> OnDimensionChanged;

    [SerializeField] private int currentDimension = 0;
    public int CurrentDimension => currentDimension;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ShiftDimension(int newDimension)
    {
        if (newDimension == currentDimension) return;
        currentDimension = newDimension;
        OnDimensionChanged?.Invoke(currentDimension);
    }
}