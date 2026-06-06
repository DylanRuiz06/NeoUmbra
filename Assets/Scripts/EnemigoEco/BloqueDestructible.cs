using UnityEngine;

public class BloqueDestructible : MonoBehaviour
{
    // Puedes arrastrar aquí un prefab de partículas de piedras rompiéndose si quieres
    public GameObject efectoParticulas;

    public void Romper()
    {
        Debug.Log("¡Bloque destruido por la onda del jugador!");

        if (efectoParticulas != null)
        {
            Instantiate(efectoParticulas, transform.position, Quaternion.identity);
        }

        // Esto borra el bloque de la escena
        Destroy(gameObject);
    }
}