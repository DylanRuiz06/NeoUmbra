using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Move : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private BoxCollider2D boxCollider;

    [Header("Move")]
    private float horizontal_Move = 0f;
    [SerializeField] private float move_Speed;
    [Range(0, 0.3f)][SerializeField] private float smoth_Move;

    [Header("Jump")]
    [SerializeField] private float jump_Power;
    [SerializeField] private LayerMask ground_Layer;
    [SerializeField] private float ray_Distance = 0.15f;

    [Header("Coyote Time")]
    [SerializeField] private float coyote_Time = 0.15f;

    private Coroutine activeAbilityCoroutine;

    private float coyote_Counter = 0f;
    private bool is_Grounded = false;
    private bool look_Right = true;
    private Vector3 speed = Vector3.zero;

    [UnitHeaderInspectable("Animation")]
    private Animator animator;

    [Header("Dash")]
    [SerializeField] private float dash_Speed = 25f;
    [SerializeField] private float time_Dash = 0.2f;
    private float ini_Gravity;
    private bool can_Dash = true;
    private bool can_Move = true;
    private bool isDashing = false;

    [Header("Habilidades Temporales")]
    private int jumpsLeft;
    private int maxJumps = 1;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        ini_Gravity = rb2D.gravityScale;

        maxJumps = 1;
        jumpsLeft = 1;
    }

    private void Update()
    {
        horizontal_Move = Input.GetAxisRaw("Horizontal") * move_Speed * 1000;
        is_Grounded = Is_Grounded();

        animator.SetFloat("Horizontal", Mathf.Abs(horizontal_Move));
        animator.SetFloat("Vertical", rb2D.linearVelocity.y);
        animator.SetBool("isFloor", is_Grounded);

        // Coyote Time
        if (is_Grounded)
        {
            coyote_Counter = coyote_Time;
        }
        else
        {
            coyote_Counter -= Time.deltaTime;
        }

        // Reset saltos
        if (is_Grounded && rb2D.linearVelocity.y <= 0.1f)
        {
            jumpsLeft = maxJumps;
        }

        // SALTO
        if (Input.GetButtonDown("Jump"))
        {
            if (is_Grounded || coyote_Counter > 0f || jumpsLeft > 0)
            {
                if (!is_Grounded && coyote_Counter > 0f && maxJumps == 1)
                {
                    jumpsLeft = 0;
                }

                rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jump_Power);
                animator.SetTrigger("Jumpp");

                jumpsLeft--;
                coyote_Counter = 0f;
            }
        }

        // Corte de salto
        if (Input.GetButtonUp("Jump") && rb2D.linearVelocity.y > 0f)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, rb2D.linearVelocity.y * 0.5f);
        }

        // DASH
        if (Input.GetKeyDown(KeyCode.LeftShift) && can_Dash)
        {
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate()
    {
        if (can_Move && !isDashing)
        {
            Movement(horizontal_Move * Time.fixedDeltaTime);
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        can_Move = false;
        can_Dash = false;

        float direction = look_Right ? 1f : -1f;

        rb2D.gravityScale = 0;
        rb2D.linearVelocity = new Vector2(dash_Speed * direction, 0);

        animator.SetTrigger("Dash");

        yield return new WaitForSeconds(time_Dash);

        rb2D.linearVelocity = Vector2.zero;
        rb2D.gravityScale = ini_Gravity;

        isDashing = false;
        can_Move = true;
        can_Dash = true;
    }

    public void EnableDoubleJump(float duration)
    {
        if (activeAbilityCoroutine != null) StopCoroutine(activeAbilityCoroutine);
        activeAbilityCoroutine = StartCoroutine(DoubleJumpRoutine(duration));
        jumpsLeft = 2;
    }

    private IEnumerator DoubleJumpRoutine(float duration)
    {
        maxJumps = 2;

        yield return new WaitForSeconds(duration);

        maxJumps = 1;

        if (jumpsLeft > 1) jumpsLeft = 1;

        Debug.Log("Salto Doble Expirado");
    }

    private bool Is_Grounded()
    {
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
        Vector3 target_Speed = new Vector2(move, rb2D.linearVelocity.y);
        rb2D.linearVelocity = Vector3.SmoothDamp(rb2D.linearVelocity, target_Speed, ref speed, smoth_Move);

        if (move > 0 && !look_Right)
            Turn();
        else if (move < 0 && look_Right)
            Turn();
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