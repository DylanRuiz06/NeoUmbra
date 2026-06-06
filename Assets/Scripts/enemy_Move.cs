using UnityEngine;
using System.Collections;

public class EnemyPatrol : MonoBehaviour
{
    // ─── Estados ────────────────────────────────────────────────────────────────

    private enum State { Patrolling, DoubleJumping, Returning }
    private State _state = State.Patrolling;

    // ─── Inspector ──────────────────────────────────────────────────────────────

    [Header("Patrol Settings")]
    [Tooltip("Distancia desde el punto de inicio hacia cada lado")]
    [SerializeField] private float patrolRange = 5f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Raycast Frontal (obstáculos / jugador)")]
    [SerializeField] private float raycastDistance = 1.5f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Offset vertical del origen del raycast frontal")]
    [SerializeField] private Vector2 raycastOriginOffset = new Vector2(0f, 0f);

    [Header("Raycast Trasero (jugador)")]
    [SerializeField] private float rearRaycastDistance = 2f;
    [SerializeField] private LayerMask rearPlayerLayer;
    [Tooltip("Offset vertical del origen del raycast trasero")]
    [SerializeField] private Vector2 rearRaycastOriginOffset = new Vector2(0f, 0f);

    [Header("Double Jump Settings")]
    [SerializeField] private float jumpForce = 8f;
    [Tooltip("Velocidad Y por debajo de la cual se considera que el enemigo llegó al apex del salto")]
    [SerializeField][Range(0f, 0.5f)] private float apexThreshold = 0.1f;
    [Tooltip("Tiempo máximo de espera para el segundo salto por si el apex no se detecta")]
    [SerializeField] private float maxApexWait = 0.6f;

    [Header("Flip Suavizado")]
    [Tooltip("Velocidad a la que el enemigo gira (mayor = más rápido)")]
    [SerializeField] private float flipSpeed = 10f;
    [Tooltip("Activa esto si el sprite está dibujado mirando a la izquierda en lugar de a la derecha")]
    [SerializeField] private bool spriteFlippedByDefault = false;

    [Header("Gizmos")]
    [SerializeField] private bool showGizmos = true;

    // ─── Estado interno ──────────────────────────────────────────────────────────

    private float _startX;
    private float _leftLimit;
    private float _rightLimit;
    private int _direction = 1;     // 1 = derecha, -1 = izquierda
    private float _targetScaleX = 1f;   // escala X objetivo para el flip suave

    private Rigidbody2D _rb;
    private Animator _animator;
    private bool _isDoubleJumping = false;
    private State _previousState = State.Patrolling;

    // Info pública del raycast frontal
    public bool IsObstacleDetected { get; private set; }
    public bool IsPlayerDetected { get; private set; }
    public RaycastHit2D LastHit { get; private set; }

    // ─── Unity ──────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        _startX = transform.position.x;
        _leftLimit = _startX - patrolRange;
        _rightLimit = _startX + patrolRange;
        // Si el sprite está dibujado al revés, arrancamos con escala negativa
        float baseScale = Mathf.Abs(transform.localScale.x);
        _targetScaleX = spriteFlippedByDefault ? -baseScale : baseScale;
        Vector3 s = transform.localScale;
        s.x = _targetScaleX;
        transform.localScale = s;
    }

    private void Update()
    {
        DetectFront();
        DetectRear();
        SmoothFlip();

        switch (_state)
        {
            case State.Patrolling: HandlePatrol(); break;
            case State.Returning: HandleReturn(); break;
                // State.DoubleJumping se gestiona por corrutina
        }
    }

    // ─── Detección frontal ───────────────────────────────────────────────────────

    private void DetectFront()
    {
        Vector2 origin = (Vector2)transform.position + raycastOriginOffset;
        Vector2 direction = Vector2.right * _direction;

        RaycastHit2D playerHit = Physics2D.Raycast(origin, direction, raycastDistance, playerLayer);
        RaycastHit2D obstacleHit = Physics2D.Raycast(origin, direction, raycastDistance, obstacleLayer);

        IsPlayerDetected = playerHit.collider != null;
        IsObstacleDetected = obstacleHit.collider != null;
        LastHit = IsPlayerDetected ? playerHit : obstacleHit;
    }

    // ─── Detección trasera ───────────────────────────────────────────────────────

    private void DetectRear()
    {
        if (_state != State.Patrolling) return;

        Vector2 origin = (Vector2)transform.position + rearRaycastOriginOffset;
        Vector2 direction = Vector2.right * -_direction;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, rearRaycastDistance, rearPlayerLayer);
        if (hit.collider == null) return;

        float playerX = hit.collider.transform.position.x;
        bool inArea = playerX >= _leftLimit && playerX <= _rightLimit;

        if (inArea)
        {
            Flip();
            TriggerDoubleJump(State.Patrolling);
        }
    }

    // ─── Patrullaje ──────────────────────────────────────────────────────────────

    private void HandlePatrol()
    {
        float posX = transform.position.x;

        if (IsObstacleDetected || IsPlayerDetected)
        {
            TriggerDoubleJump(State.Patrolling);
            return;
        }

        if (posX > _rightLimit || posX < _leftLimit)
        {
            _state = State.Returning;
            return;
        }

        if (_direction == 1 && posX >= _rightLimit) { Flip(); return; }
        if (_direction == -1 && posX <= _leftLimit) { Flip(); return; }

        transform.Translate(Vector2.right * (_direction * moveSpeed * Time.deltaTime));
    }

    // ─── Regreso al área ─────────────────────────────────────────────────────────

    private void HandleReturn()
    {
        float posX = transform.position.x;

        if (posX >= _leftLimit && posX <= _rightLimit)
        {
            _state = State.Patrolling;
            return;
        }

        int dirToCenter = posX < _startX ? 1 : -1;
        if (dirToCenter != _direction) Flip();

        if (IsObstacleDetected)
        {
            TriggerDoubleJump(State.Returning);
            return;
        }

        transform.Translate(Vector2.right * (_direction * moveSpeed * Time.deltaTime));
    }

    // ─── Flip suavizado ──────────────────────────────────────────────────────────

    /// <summary>
    /// Registra la intención de girar; SmoothFlip() lo aplica gradualmente en Update.
    /// </summary>
    private void Flip()
    {
        _direction *= -1;
        _targetScaleX = Mathf.Sign(_direction) * Mathf.Abs(transform.localScale.x);
    }

    /// <summary>
    /// Interpola la escala X actual hacia _targetScaleX cada frame.
    /// </summary>
    private void SmoothFlip()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Lerp(scale.x, _targetScaleX, flipSpeed * Time.deltaTime);
        transform.localScale = scale;
    }

    // ─── Doble salto ─────────────────────────────────────────────────────────────

    private void TriggerDoubleJump(State returnTo = State.Patrolling)
    {
        if (_isDoubleJumping) return;
        _previousState = returnTo;
        StartCoroutine(DoubleJumpRoutine());
    }

    private IEnumerator DoubleJumpRoutine()
    {
        _isDoubleJumping = true;
        _state = State.DoubleJumping;

        // — Primer salto —
        if (_animator != null) _animator.SetTrigger("jum");
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        float elapsed = 0f;
        while (elapsed < maxApexWait)
        {
            elapsed += Time.deltaTime;
            if (Mathf.Abs(_rb.linearVelocity.y) <= apexThreshold) break;
            yield return null;
        }

        // — Segundo salto —
        if (_animator != null) _animator.SetTrigger("jum");
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        yield return new WaitUntil(() => _rb.linearVelocity.y < 0f);
        yield return new WaitUntil(() => Mathf.Abs(_rb.linearVelocity.y) < 0.15f);

        _isDoubleJumping = false;

        float posX = transform.position.x;
        _state = (posX >= _leftLimit && posX <= _rightLimit)
            ? State.Patrolling
            : _previousState;
    }

    // ─── Gizmos ──────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        float originX = Application.isPlaying ? _startX : transform.position.x;
        float posY = transform.position.y;

        Vector3 left = new Vector3(originX - patrolRange, posY, 0f);
        Vector3 right = new Vector3(originX + patrolRange, posY, 0f);
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);
        Gizmos.DrawLine(left, right);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(left, 0.15f);
        Gizmos.DrawSphere(right, 0.15f);

        int dir = Application.isPlaying ? _direction : 1;

        // Raycast frontal (rojo)
        Vector2 frontOrigin = (Vector2)transform.position + raycastOriginOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(frontOrigin, Vector2.right * dir * raycastDistance);
        Gizmos.DrawWireSphere(frontOrigin + Vector2.right * dir * raycastDistance, 0.08f);

        // Raycast trasero (amarillo)
        Vector2 rearOrigin = (Vector2)transform.position + rearRaycastOriginOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rearOrigin, Vector2.right * -dir * rearRaycastDistance);
        Gizmos.DrawWireSphere(rearOrigin + Vector2.right * -dir * rearRaycastDistance, 0.08f);
    }
}