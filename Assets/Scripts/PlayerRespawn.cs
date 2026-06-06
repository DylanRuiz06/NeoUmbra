using System.Collections;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Reaparición")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float fallLimit = -10f; // Y mínima antes de morir

    void Update()
    {
        if (transform.position.y < fallLimit)
            StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        // Evita que Update dispare múltiples veces mientras respawnea
        enabled = false;

        // Si murió en dimensión 2, volver a dimensión 1
        if (DimensionManager.Instance.CurrentDimension != 0)
            DimensionManager.Instance.ShiftDimension(0);

        // Reubicar al jugador
        transform.position = respawnPoint.position;

        // Resetear velocidad para que no caiga al reaparecer
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        yield return null; // espera un frame para que todo se estabilice
        enabled = true;
    }
}