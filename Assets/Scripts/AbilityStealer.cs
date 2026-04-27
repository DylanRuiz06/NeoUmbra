using UnityEngine;

public class AbilityStealer : MonoBehaviour
{
    private float range = 2.89f; 

    public LayerMask enemyLayer;
    private float offsetY = 1.8f;

    void Update()
    {
        Vector3 rayOrigin = transform.position + new Vector3(0, offsetY, 0);
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        // Visualización constante para que veas qué tan corto es el rayo ahora
        Debug.DrawRay(rayOrigin, direction * range, Color.green);

        if (Input.GetKeyDown(KeyCode.E))
        {
            // La lógica ahora usa el nuevo 'range' corto
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, range, enemyLayer);

            if (hit.collider != null)
            {
                IStoleable enemy = hit.collider.GetComponent<IStoleable>();
                if (enemy != null)
                {
                    enemy.OnSteal(GetComponent<Move>());
                }
            }
        }
    }
}