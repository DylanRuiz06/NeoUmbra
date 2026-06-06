using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Move2 : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private BoxCollider2D boxCollider;

    [Header("Move")]
    private float horizontal_Move = 0f;
    [SerializeField] private float move_Speed;
    [Range(0, 0.3f)][SerializeField] private float smoth_Move;

    // CORRECCIÓN 1: Cambiamos Vector3 por float para suavizar solo un eje
    private float speedX = 0f;

    [Header("Jump")]
    [SerializeField] private float jump_Power;
    [SerializeField] private float jump_MinMultiplier = 0.4f;
    [SerializeField] private LayerMask ground_Layer;
    [SerializeField] private float ray_Distance = 0.15f;

    [Header("Coyote Time")]
    [SerializeField] private float coyote_Time = 0.15f;

    private float coyote_Counter = 0f;
    private bool is_Grounded = false;
    private bool is_Jumping = false;
    private bool look_Right = true;

    [UnitHeaderInspectable("Animation")]
    private Animator animator;

    [Header("Dash")]
    [SerializeField] private float dash_Speed;
    [SerializeField] private float time_Dash;
    private float ini_Gravity;
    private bool can_Dash = true;
    private bool can_Move = true;


    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        ini_Gravity = rb2D.gravityScale;
    }

    private void Update()
    {
        horizontal_Move = Input.GetAxisRaw("Horizontal") * move_Speed * 1000;

        is_Grounded = Is_Grounded();

        animator.SetFloat("Horizontal", Mathf.Abs(horizontal_Move));
        animator.SetFloat("Vertical", rb2D.linearVelocity.y);  // negativa = cayendo
        animator.SetBool("isFloor", is_Grounded);

        // para el coyote time
        if (is_Grounded)
        {
            coyote_Counter = coyote_Time;
            is_Jumping = false;
        }
        else
        {
            coyote_Counter -= Time.deltaTime;
        }

        // salto 
        if (Input.GetButtonDown("Jump") && coyote_Counter > 0f)
        {
            is_Jumping = true;
            coyote_Counter = 0f;
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, 0f);
            rb2D.AddForce(Vector2.up * jump_Power, ForceMode2D.Impulse);
        }

        if (Input.GetButtonUp("Jump") && is_Jumping && rb2D.linearVelocity.y > 0f)
        {
            rb2D.linearVelocity = new Vector2(
                rb2D.linearVelocity.x,
                rb2D.linearVelocity.y * jump_MinMultiplier
            );
        }

        // dash 
        if (Input.GetKeyDown(KeyCode.LeftShift) && can_Dash)
        {
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate()
    {
        if (can_Move)
        {
            Movement(horizontal_Move * Time.fixedDeltaTime);
        }
    }

    private bool Is_Grounded()
    {
        // CORRECCIÓN 3: Si el personaje está subiendo, ignoramos el suelo para evitar parpadeos en la animación.
        if (rb2D.linearVelocity.y > 0.1f) return false;

        Vector2 bottom_Center = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        Vector2 bottom_Left = new Vector2(boxCollider.bounds.min.x, boxCollider.bounds.min.y);
        Vector2 bottom_Right = new Vector2(boxCollider.bounds.max.x, boxCollider.bounds.min.y);

        RaycastHit2D hit_Center = Physics2D.Raycast(bottom_Center, Vector2.down, ray_Distance, ground_Layer);
        RaycastHit2D hit_Left = Physics2D.Raycast(bottom_Left, Vector2.down, ray_Distance, ground_Layer);
        RaycastHit2D hit_Right = Physics2D.Raycast(bottom_Right, Vector2.down, ray_Distance, ground_Layer);

        return hit_Center.collider != null || hit_Left.collider != null || hit_Right.collider != null;
    }

    private void Movement(float move)
    {
        // CORRECCIÓN 1: Suavizamos exclusivamente el eje X. El eje Y se queda con su valor actual intacto.
        float smoothedX = Mathf.SmoothDamp(rb2D.linearVelocity.x, move, ref speedX, smoth_Move);
        rb2D.linearVelocity = new Vector2(smoothedX, rb2D.linearVelocity.y);

        if (move > 0 && !look_Right)
            Turn();
        else if (move < 0 && look_Right)
            Turn();
    }

    private IEnumerator Dash()
    {
        can_Move = false;
        can_Dash = false;
        rb2D.gravityScale = 0;

        // CORRECCIÓN 2: Mathf.Sign asegura que la dirección sea siempre 1 o -1 exacto.
        float dashDirection = Mathf.Sign(transform.localScale.x);

        // Mantenemos la Y estrictamente en 0 durante el impulso para evitar rebotes con el piso.
        rb2D.linearVelocity = new Vector2(dash_Speed * dashDirection, 0f);
        animator.SetTrigger("Dash");

        yield return new WaitForSeconds(time_Dash);

        // Opcional pero recomendado: Frenar al terminar el dash para recuperar el control sin salir volando
        rb2D.linearVelocity = new Vector2(0f, rb2D.linearVelocity.y);

        can_Move = true;
        can_Dash = true;
        rb2D.gravityScale = ini_Gravity;
    }

    private void Turn()
    {
        look_Right = !look_Right;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    private void OnDrawGizmos()
    {
        if (boxCollider == null) return;

        Gizmos.color = is_Grounded ? Color.green : Color.red;

        Vector2 bottom_Center = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        Vector2 bottom_Left = new Vector2(boxCollider.bounds.min.x, boxCollider.bounds.min.y);
        Vector2 bottom_Right = new Vector2(boxCollider.bounds.max.x, boxCollider.bounds.min.y);

        Gizmos.DrawLine(bottom_Center, bottom_Center + Vector2.down * ray_Distance);
        Gizmos.DrawLine(bottom_Left, bottom_Left + Vector2.down * ray_Distance);
        Gizmos.DrawLine(bottom_Right, bottom_Right + Vector2.down * ray_Distance);
    }
}