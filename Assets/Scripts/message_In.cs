using UnityEngine;
using System.Collections;

public class ShowMessageTrigger : MonoBehaviour
{
    [Header("Configuración de Interfaz")]
    [Tooltip("Arrastra aquí el Panel de la UI que contiene el mensaje")]
    [SerializeField] private GameObject messagePanel;

    [Header("Configuración de Tiempo")]
    [Tooltip("¿Cuántos segundos estará visible el mensaje?")]
    [SerializeField] private float timeVisible = 3f;

    [Tooltip("¿El mensaje debe aparecer solo una vez y luego destruirse?")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void Start()
    {
        // Nos aseguramos de que el panel empiece apagado por si se te olvidó en el editor
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    // Esta función se ejecuta cuando algo entra en el cuadrado invisible
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificamos si lo que entró fue el jugador y si no se ha activado ya
        if (other.CompareTag("Player") && !hasTriggered)
        {
            if (triggerOnlyOnce)
            {
                hasTriggered = true; // Marcamos para que no vuelva a pasar
            }

            // Iniciamos la rutina de tiempo
            StartCoroutine(ShowMessageRoutine());
        }
    }

    private IEnumerator ShowMessageRoutine()
    {
        // 1. Encendemos el panel
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
        }

        // 2. Esperamos el tiempo indicado (el juego no se pausa, el jugador se puede seguir moviendo)
        yield return new WaitForSeconds(timeVisible);

        // 3. Apagamos el panel
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }

        // 4. (Opcional) Destruimos este trigger para que no ocupe memoria si ya no se va a usar
        if (triggerOnlyOnce)
        {
            Destroy(gameObject);
        }
    }
}