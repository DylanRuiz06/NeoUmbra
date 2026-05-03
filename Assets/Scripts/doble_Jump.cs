using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class DoubleJumpEnemy : MonoBehaviour, IStoleable
{
    [Header("Configuración de Robo")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private float respawnTime = 5f;

    [Header("Prefabs")]
    [SerializeField] private GameObject activePrefab;
    [SerializeField] private GameObject inactivePrefab;

    [Header("Posicionamiento Visual")]
    [Tooltip("Offset del prefab activo respecto al pivot del objeto (no mueve el collider)")]
    [SerializeField] private Vector3 offsetPosicion = new Vector3(0f, -1f, 0f);
    [Tooltip("Cuánto baja en Y el prefab inactivo respecto a la posición del enemigo activo")]
    [SerializeField] private float inactiveYOffset = 0.5f;

    // ─── Estado interno ──────────────────────────────────────────────────────────

    private bool _isActive = true;
    private GameObject _activeInstance;
    private GameObject _inactiveInstance;

    // ─── Unity ──────────────────────────────────────────────────────────────────

    void Awake()
    {
        // nada por ahora
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        RefreshEditorPreview();
#endif
    }

    void OnDisable()
    {
        // En Play, OnDisable se llama también al hacer SetActive(false).
        // Solo destruimos las instancias si realmente se está saliendo del modo Play
        // o destruyendo el objeto, no cuando simplemente se desactiva.
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyPreview();
#endif
    }

    void OnDestroy()
    {
        // Limpiar el inactivo cuando el enemigo se destruye
        if (_inactiveInstance != null)
            Destroy(_inactiveInstance);
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        RefreshEditorPreview();
#endif
    }

    void Start()
    {
        if (!Application.isPlaying) return;

        DestroyPreview();

        // Activo: hijo del enemigo para que siga su posición/escala (flip)
        if (activePrefab != null)
        {
            _activeInstance = Instantiate(activePrefab, transform.position + offsetPosicion, Quaternion.identity, transform);
            _activeInstance.transform.localPosition = offsetPosicion;
        }
        else
            Debug.LogWarning("[DoubleJumpEnemy] activePrefab no asignado en el Inspector.");

        // Inactivo: SIN padre, para que no herede el movimiento de EnemyPatrol
        if (inactivePrefab != null)
        {
            Vector3 inactivePos = transform.position + offsetPosicion - new Vector3(0f, inactiveYOffset, 0f);
            _inactiveInstance = Instantiate(inactivePrefab, inactivePos, Quaternion.identity);
            _inactiveInstance.SetActive(false); // oculto desde el primer frame
        }
        else
            Debug.LogWarning("[DoubleJumpEnemy] inactivePrefab no asignado en el Inspector.");
    }

    // ─── Previsualización en el Editor ──────────────────────────────────────────

#if UNITY_EDITOR
    private void RefreshEditorPreview()
    {
        if (Application.isPlaying) return;

        DestroyPreview();

        if (activePrefab != null)
        {
            _activeInstance = (GameObject)PrefabUtility.InstantiatePrefab(activePrefab, transform);
            _activeInstance.transform.localPosition = offsetPosicion;
            _activeInstance.hideFlags = HideFlags.DontSave;
        }
    }
#endif

    private void DestroyPreview()
    {
        if (_activeInstance != null)
        {
            if (Application.isPlaying) Destroy(_activeInstance);
            else DestroyImmediate(_activeInstance);
            _activeInstance = null;
        }

        if (_inactiveInstance != null)
        {
            if (Application.isPlaying) Destroy(_inactiveInstance);
            else DestroyImmediate(_inactiveInstance);
            _inactiveInstance = null;
        }
    }

    // ─── IStoleable ──────────────────────────────────────────────────────────────

    public void OnSteal(Move player)
    {
        if (!_isActive) return;
        player.EnableDoubleJump(duration);
        StartCoroutine(DisableAndRespawn());
    }

    private IEnumerator DisableAndRespawn()
    {
        _isActive = false;

        // Fijar posición del inactivo ANTES de desactivar el padre
        if (_inactiveInstance != null)
        {
            _inactiveInstance.transform.position = transform.position + offsetPosicion - new Vector3(0f, inactiveYOffset, 0f);
            _inactiveInstance.SetActive(true);
        }

        // Desactivar el objeto padre completo: oculta el modelo activo
        // y pausa EnemyPatrol, Rigidbody2D y todo lo demás
        // Usamos Invoke para que el respawn ocurra aunque el objeto esté inactivo
        Invoke(nameof(Respawn), respawnTime);
        gameObject.SetActive(false);

        Debug.Log("Enemigo desactivado...");

        yield break; // la corrutina termina aquí; el resto lo maneja Invoke
    }

    private void Respawn()
    {
        gameObject.SetActive(true);

        if (_inactiveInstance != null) _inactiveInstance.SetActive(false);

        _isActive = true;

        Debug.Log("Enemigo reactivado y listo para ser robado de nuevo.");
    }

    // ─── Gizmos ──────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.1f);

        Gizmos.color = Color.green;
        Vector3 visualPos = transform.position + offsetPosicion;
        Gizmos.DrawWireSphere(visualPos, 0.2f);
        Gizmos.DrawLine(transform.position, visualPos);

        Gizmos.color = Color.yellow;
        Vector3 inactivePos = visualPos - new Vector3(0f, inactiveYOffset, 0f);
        Gizmos.DrawWireSphere(inactivePos, 0.2f);
        Gizmos.DrawLine(visualPos, inactivePos);
    }
}