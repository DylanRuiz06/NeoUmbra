using System.Collections;
using UnityEngine;

public class OndaEcoConCobertura : MonoBehaviour, IStoleable
{
    [Header("Visuales y Animación")]
    public ParticleSystem particulasOnda;

    [Header("Configuración de la Onda")]
    public float radioMaximo = 8f;
    public float velocidadExpansion = 10f;
    public int danio = 1;

    [Header("Ataque Automático")]
    public float tiempoEntreAtaques = 7f;
    private float cronometro = 0f;

    [Header("Detección y Capas")]
    public LayerMask capaJugador;
    public LayerMask capaObstaculos;

    [Header("Ajuste de Altura PSB")]
    [SerializeField] private float villanoOffsetY = 1.2f; // Sube el origen al pecho del villano

    private float radioActual = 0f;
    private bool golpeoAlJugador = false;

    [SerializeField] private Animator miAnimator;       // Arrastra aquí el Animator del villano
    [SerializeField] private string nombreTriggerAnim = "Atacar"; // Nombre del Trigger en el Animator
    [SerializeField] private float retrasoOndaFisica = 0.5f;

    [Header("Componentes a Deshabilitar al Robar")]
    [SerializeField] private GameObject objetoOjo; // Arrastra aquí el objeto del ojo del villano
    private bool estaDeshabilitado = false;

    private void Start()
    {
        cronometro = tiempoEntreAtaques;
    }

    private void Update()
    {
        if (estaDeshabilitado) return;
        cronometro += Time.deltaTime;

        if (cronometro >= tiempoEntreAtaques)
        {
            LanzarOnda();
            cronometro = 0f;
        }
    }

    public void OnSteal(Move playerMove)
    {
        Debug.Log("¡El jugador me está robando la onda de eco!");

        // 1. Le pasamos 'this' (este enemigo) al jugador para que sepa a quién apagar y encender
        playerMove.HabilitarOndaEco(this);

        // 2. Apagamos el ojo y el Animator
        DesactivarEnemigo();

        // ¡¡OJO!! NO PONGAS Destroy(gameObject); aquí, 
        // porque si lo destruyes, el enemigo desaparecerá para siempre y no podrá despertar.
    }
    private void DesactivarEnemigo()
    {
        estaDeshabilitado = true;
        StopAllCoroutines(); // Detiene cualquier ataque u onda en curso

        // Apagamos el objeto del ojo
        if (objetoOjo != null) objetoOjo.SetActive(false);

        // Apagamos el Animator para que deje de hacer cualquier animación
        if (miAnimator != null) miAnimator.enabled = false;

        Debug.Log("Villano deshabilitado: Ojo apagado y animaciones detenidas.");
    }

    public void ReactivarEnemigo()
    {
        estaDeshabilitado = false;
        cronometro = 0f; // Reinicia su reloj desde cero al despertar

        // Volvemos a encender el ojo
        if (objetoOjo != null) objetoOjo.SetActive(true);

        // Volvemos a encender el Animator
        if (miAnimator != null) miAnimator.enabled = true;

        Debug.Log("¡El villano ha despertado! El ojo y las animaciones vuelven a funcionar.");
    }

    public void LanzarOnda()
    {
        StopAllCoroutines();
        // Ahora la corrutina principal se encarga de la animación y del eco
        StartCoroutine(SecuenciaDeAtaque());
    }

    private IEnumerator SecuenciaDeAtaque()
    {
        // 1. Activamos la animación en el Animator
        if (miAnimator != null)
        {
            miAnimator.SetTrigger(nombreTriggerAnim);
        }

        // 2. Esperamos a que la animación se reproduzca un poco antes de soltar el poder físico
        yield return new WaitForSeconds(retrasoOndaFisica);

        // 3. Comenzamos la expansión física del eco (lo que hacía tu código original)
        StartCoroutine(ExpandirOnda());
    }

    private IEnumerator ExpandirOnda()
    {
        radioActual = 0f;
        golpeoAlJugador = false;

        // Calculamos el centro real (en el pecho del villano) sumando el offset
        Vector3 centroOnda = transform.position + new Vector3(0, villanoOffsetY, 0);

        if (particulasOnda != null)
        {
            particulasOnda.Stop();
            particulasOnda.Play();
        }

        while (radioActual < radioMaximo)
        {
            radioActual += velocidadExpansion * Time.deltaTime;

            // Buscamos usando el centroOnda para que el círculo no esté en el piso
            Collider2D jugadorDetectado = Physics2D.OverlapCircle(centroOnda, radioActual, capaJugador);

            if (jugadorDetectado != null && !golpeoAlJugador)
            {
                // La dirección se calcula desde el pecho del villano
                Vector2 direccionHaciaJugador = (Vector2)jugadorDetectado.transform.position - (Vector2)centroOnda;
                float distanciaAlJugador = direccionHaciaJugador.magnitude;

                Debug.DrawLine(centroOnda, jugadorDetectado.transform.position, Color.green, 0.1f);

                // TRUCO CLAVE: Buscamos únicamente obstáculos intermedios (paredes reales)
                RaycastHit2D hitObstaculo = Physics2D.Raycast(centroOnda, direccionHaciaJugador.normalized, distanciaAlJugador, capaObstaculos);

                // Si no hay ningún muro intermedio real tapándote en línea recta...
                if (hitObstaculo.collider == null)
                {
                    Debug.Log("¡Jugador alcanzado por el eco!");
                    golpeoAlJugador = true;

                    // Usamos GetComponentInParent por si golpea un colisionador hijo del PSB del Player
                    PlayerHealth vidaJugador = jugadorDetectado.GetComponentInParent<PlayerHealth>();
                    if (vidaJugador != null)
                    {
                        vidaJugador.TakeDamage(danio);
                    }
                }
                else
                {
                    Debug.Log("El jugador está a salvo detrás del obstáculo: " + hitObstaculo.collider.name);
                }
            }

            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Dibujamos el Gizmo centrado en el pecho para ajustar el offset en el editor
        Vector3 centroOnda = transform.position + new Vector3(0, villanoOffsetY, 0);
        Gizmos.DrawWireSphere(centroOnda, radioMaximo);
    }
}