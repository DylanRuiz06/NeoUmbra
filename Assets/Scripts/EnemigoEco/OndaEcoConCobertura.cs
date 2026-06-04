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
    public float tiempoEntreAtaques = 7f; // Tiempo en segundos
    private float cronometro = 0f;        // Mide el tiempo transcurrido

    [Header("Detección y Capas")]
    public LayerMask capaJugador;
    public LayerMask capaObstaculos;

    private float radioActual = 0f;
    private bool golpeoAlJugador = false;

    private void Start()
    {
        // Opcional: Empezar el cronómetro al máximo si quieres que 
        // ataque inmediatamente al iniciar el juego.
        cronometro = tiempoEntreAtaques;
    }

    private void Update()
    {
        // El cronómetro aumenta con el tiempo real del juego
        cronometro += Time.deltaTime;

        // Si pasan los 7 segundos, lanza la onda y reinicia el reloj
        if (cronometro >= tiempoEntreAtaques)
        {
            LanzarOnda();
            cronometro = 0f; // Resetea el contador a cero
        }
    }

    public void OnSteal(Move playerMove)
    {
        Debug.Log("¡El jugador me está robando la onda de eco!");

        // Le pasamos la habilidad al script Move del jugador
        playerMove.HabilitarOndaEco();

        // Opcional: Aquí puedes destruir al enemigo o hacer que muera
        Destroy(gameObject);
    }

    public void LanzarOnda()
    {
        // Detiene la onda anterior antes de iniciar una nueva
        StopAllCoroutines();
        StartCoroutine(ExpandirOnda());
    }

    private IEnumerator ExpandirOnda()
    {
        radioActual = 0f;
        golpeoAlJugador = false;
        LayerMask capasAfectadas = capaJugador | capaObstaculos;

        if (particulasOnda != null)
        {
            particulasOnda.Stop();
            particulasOnda.Play();
        }

        while (radioActual < radioMaximo)
        {
            radioActual += velocidadExpansion * Time.deltaTime;

            Collider2D jugadorDetectado = Physics2D.OverlapCircle(transform.position, radioActual, capaJugador);

            if (jugadorDetectado != null && !golpeoAlJugador)
            {
                Vector2 direccionHaciaJugador = jugadorDetectado.transform.position - transform.position;
                float distanciaAlJugador = direccionHaciaJugador.magnitude;

                Debug.DrawLine(transform.position, jugadorDetectado.transform.position, Color.green, 0.1f);

                RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionHaciaJugador.normalized, distanciaAlJugador, capasAfectadas);

                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.Log("¡Jugador alcanzado por el eco!");
                        golpeoAlJugador = true;

                        PlayerHealth vidaJugador = hit.collider.GetComponent<PlayerHealth>();
                        if (vidaJugador != null)
                        {
                            vidaJugador.TakeDamage(danio);
                        }
                    }
                    else
                    {
                        Debug.Log("El jugador está a salvo detrás de una cobertura.");
                    }
                }
            }

            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioMaximo);
    }
}