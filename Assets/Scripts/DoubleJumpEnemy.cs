using UnityEngine;
using System.Collections;
public class DoubleJumpEnemy : MonoBehaviour, IStoleable
{
    [Header("Configuración de Robo")]
    [SerializeField] private float duration = 5f; // Cuánto tiempo le dará al jugador
    [SerializeField] private float respawnTime = 5f;

    private bool isActive = true;

    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
    }

    public void OnSteal(Move player)
    {
        if (!isActive) return;
        player.EnableDoubleJump(duration);
        StartCoroutine(DisableAndRespawn());
    }

    // Para identificarlo visualmente en el editor
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.1f);
    }

    private IEnumerator DisableAndRespawn()
    {
        isActive = false;
        int originalLayer = gameObject.layer;

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        spriteRenderer.color = new Color(1, 1, 1, 0.3f);

        Debug.Log("Enemigo desactivado...");

        // Esperar el tiempo de respawn
        yield return new WaitForSeconds(respawnTime);

        gameObject.layer = originalLayer;
        spriteRenderer.color = Color.white;
        isActive = true;

        Debug.Log("Enemigo reactivado y listo para ser robado de nuevo.");
    }
}