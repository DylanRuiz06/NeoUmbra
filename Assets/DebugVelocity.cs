using UnityEngine;

public class DebugVelocity : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Presiona H para forzar velocidad horizontal directa
        if (Input.GetKeyDown(KeyCode.H))
        {
            rb.linearVelocity = new Vector2(25f, rb.linearVelocity.y);
            Debug.Log("Velocidad aplicada: " + rb.linearVelocity);
        }

        /*  Log continuo para ver si algo la resetea
        if (rb.linearVelocity.x != 0)
        {
            Debug.Log($"Frame:{Time.frameCount} vel.x={rb.linearVelocity.x}");
        }
    */
    }
}