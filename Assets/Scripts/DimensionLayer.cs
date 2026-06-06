using UnityEngine;

public class DimensionLayer : MonoBehaviour
{
    [SerializeField] private int dimensionId = 0;

    void Start()
    {
        DimensionManager.Instance.OnDimensionChanged += HandleDimensionChange;

        // Aplica estado inicial a los hijos
        bool isActive = DimensionManager.Instance.CurrentDimension == dimensionId;
        SetChildrenActive(isActive);
    }

    void OnDestroy()
    {
        if (DimensionManager.Instance != null)
            DimensionManager.Instance.OnDimensionChanged -= HandleDimensionChange;
    }

    private void HandleDimensionChange(int newDimension)
    {
        SetChildrenActive(newDimension == dimensionId);
    }

    private void SetChildrenActive(bool active)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(active);
        }
    }
}