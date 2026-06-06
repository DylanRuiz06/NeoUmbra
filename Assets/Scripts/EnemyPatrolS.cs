using UnityEngine;

public class EnemyPatrols : MonoBehaviour
{
    [Header("Patrulla")]
    [SerializeField] private float leftDistance = 3f;
    [SerializeField] private float rightDistance = 3f;
    [SerializeField] private float speed = 3f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private float leftLimit;
    private float rightLimit;
    private int direction = 1; // 1 = derecha, -1 = izquierda

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        leftLimit  = transform.position.x - leftDistance;
        rightLimit = transform.position.x + rightDistance;
    }

    void FixedUpdate()
    {
        // Mover con velocidad constante
        rb.linearVelocity = new Vector2(speed * direction, rb.linearVelocity.y);

        // Voltear sprite
        spriteRenderer.flipX = direction == -1;

        // Cambiar dirección al llegar al límite
        if (direction == 1 && transform.position.x >= rightLimit)
            direction = -1;
        else if (direction == -1 && transform.position.x <= leftLimit)
            direction = 1;
    }

    void OnDrawGizmosSelected()
    {
        float cx = transform.position.x;
        float cy = transform.position.y;
        Vector2 a = new Vector2(cx - leftDistance, cy);
        Vector2 b = new Vector2(cx + rightDistance, cy);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(a, 0.2f);
        Gizmos.DrawWireSphere(b, 0.2f);
        Gizmos.DrawLine(a, b);
    }
}