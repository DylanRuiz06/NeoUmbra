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
    private bool is_Jumping = false;
    private bool look_Right = true;
    private Vector3 speed = Vector3.zero;

    [UnitHeaderInspectable("Animation")]
    private Animator animator;

    [Header("Dash")]
    [SerializeField] private float dash_Speed;
    [SerializeField] private float time_Dash;
    private float ini_Gravity;
    private bool can_Dash = true;
    private bool can_Move = true;

    [Header("Habilidades Temporales")]
    private int jumpsLeft;
    private int maxJumps = 1; // 1 = Normal, 2 = Salto Doble


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
        animator.SetFloat("Horizontal", Mathf.Abs(horizontal_Move));

        is_Grounded = Is_Grounded();

        animator.SetFloat("Horizontal", Mathf.Abs(horizontal_Move));
        animator.SetFloat("Vertical", rb2D.linearVelocity.y);  
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

        if (is_Grounded && rb2D.linearVelocity.y <= 0.1f)
        {
            jumpsLeft = maxJumps;
        }

        if (Input.GetButtonDown("Jump"))
        {
            // Saltamos si: estamos en el suelo, o estamos en Coyote Time, o tenemos saltos dobles extra
            if (is_Grounded || coyote_Counter > 0f || jumpsLeft > 0)
            {
                // Si saltamos usando Coyote Time pero no estábamos en el suelo, 
                // gastamos un salto para que no sea infinito
                if (!is_Grounded && coyote_Counter > 0f && maxJumps == 1)
                {
                    jumpsLeft = 0;
                }

                rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jump_Power);
                animator.SetTrigger("Jumpp");

                jumpsLeft--;
                coyote_Counter = 0f; // Gastamos el coyote time al saltar
            }
        }

        if (Input.GetButtonUp("Jump") && rb2D.linearVelocity.y > 0f)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, rb2D.linearVelocity.y * 0.5f);

        }

        // dash 
        if (Input.GetKeyDown(KeyCode.LeftShift) && can_Dash)
        {
            StartCoroutine(Dash());
        }

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
        // Si el jugador estaba en el aire, evitamos que se quede sin saltos de golpe
        if (jumpsLeft > 1) jumpsLeft = 1;

        Debug.Log("Salto Doble Expirado");
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
        // los rasycast para que funcione bien el coyote time

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
        Vector3 tarjet_Speed = new Vector2(move, rb2D.linearVelocity.y);
        rb2D.linearVelocity = Vector3.SmoothDamp(rb2D.linearVelocity, tarjet_Speed, ref speed, smoth_Move);

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
        rb2D.linearVelocity = new Vector2(dash_Speed * transform.localScale.x, 0);
        animator.SetTrigger("Dash");

        yield return new WaitForSeconds(time_Dash);

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

    // para que se vean los gizmos en el editor
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