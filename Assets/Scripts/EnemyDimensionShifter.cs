using System.Collections;
using UnityEngine;

public class EnemyDimensionShifter : MonoBehaviour, IStoleable
{
    [Header("Cambio de dimensión")]
    [SerializeField] private float shiftInterval = 5f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private int totalDimensions = 2;
    [SerializeField] private float firstShiftBoost = 0.5f; // ← multiplica el timer inicial

    [Header("Sonido")]
    [SerializeField] private AudioClip dimensionShiftSound;
    private AudioSource audioSource;

    [Header("Sistema de robo")]
    [SerializeField] private GameObject inactivePrefab;
    [SerializeField] private float recoveryTime = 8f;

    [Header("Referencias")]
    [SerializeField] private Transform player;

    private float timer;
    private bool isActive = true;

    void Start()
    {
        // El primer cambio ocurre más rápido
        timer = shiftInterval * firstShiftBoost;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
            else return;
        }

        if (!isActive) return;
        if (!PlayerIsInRange()) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            TriggerShift();
            timer = shiftInterval;
        }
    }

    private void TriggerShift()
    {
        int current = DimensionManager.Instance.CurrentDimension;
        int next = (current + 1) % totalDimensions;
        DimensionManager.Instance.ShiftDimension(next);

        // Reproducir sonido
        if (dimensionShiftSound != null)
            audioSource.PlayOneShot(dimensionShiftSound);
    }

    public void OnSteal(Move player)
    {
        player.EnableDimensionShift(recoveryTime);

        if (inactivePrefab != null)
        {
            GameObject inactive = Instantiate(inactivePrefab, transform.position, transform.rotation);
            inactive.transform.localScale = transform.localScale;
            inactive.SetActive(true);

            EnemyInactive inactiveScript = inactive.GetComponent<EnemyInactive>();
            if (inactiveScript != null)
                inactiveScript.Init(recoveryTime, inactive, this.gameObject);
        }

        gameObject.SetActive(false);
    }

    private bool PlayerIsInRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= detectionRange;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}