using UnityEngine;

public class PlayerDimensionReceiver : MonoBehaviour
{
    void OnEnable()
    {
        DimensionManager.Instance.OnDimensionChanged += HandleDimensionChange;
    }

    void OnDisable()
    {
        DimensionManager.Instance.OnDimensionChanged -= HandleDimensionChange;
    }

    private void HandleDimensionChange(int newDimension)
    {
        // Aquí aplicas el efecto visual/de gameplay al jugador:
        // - Cambiar material/shader
        // - Reproducir animación de transición
        // - Mostrar UI de pantalla
        Debug.Log($"Jugador forzado a dimensión {newDimension}");
    }
}