using UnityEngine;

public class AbilityStealer : MonoBehaviour
{
    [Header("Rango")]
    [SerializeField] private float range = 2.89f;

    private Animator animator;
    private Move move;



    public LayerMask enemyLayer;
    [Header("Altura")]
    [SerializeField] private float offsetY = 1.8f;


    void Start()
    {
        animator = GetComponent<Animator>();
        move = GetComponent<Move>();
    }

    void Update()
    {
        Vector3 rayOrigin = transform.position + new Vector3(0, offsetY, 0);
        Vector2 direction = move.look_Right ? Vector2.right : Vector2.left; 

        // Visualización constante para que veas qué tan corto es el rayo ahora
        Debug.DrawRay(rayOrigin, direction * range, Color.green);

        if (Input.GetKeyDown(KeyCode.J))
        {
            // La lógica ahora usa el nuevo 'range' corto
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, range, enemyLayer);
            animator.SetTrigger("Atac");


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