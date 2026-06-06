using UnityEngine;
using UnityEngine.SceneManagement; // LÍNEA OBLIGATORIA PARA CAMBIAR DE ESCENA

public class CambioEscenaPlataforma : MonoBehaviour
{
    [Header("Configuración de la Escena")]
    [SerializeField] private string nombreEscenaDestino;

    // Este método de Unity se activa automáticamente cuando algo entra en el Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos si el objeto que pisó la plataforma tiene la etiqueta "Player"
        if (collision.CompareTag("Player"))
        {
            Debug.Log("¡El jugador pisó la plataforma! Cargando: " + nombreEscenaDestino);

            // Cambiamos de escena
            SceneManager.LoadScene(nombreEscenaDestino);
        }
    }
}