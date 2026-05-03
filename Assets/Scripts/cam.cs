using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;

    [Header("Offset")]
    [Tooltip("Desplazamiento horizontal (positivo = más a la derecha del jugador)")]
    [SerializeField] private float offsetX = 3f;
    [Tooltip("Desplazamiento vertical")]
    [SerializeField] private float offsetY = 0f;

    [Header("Suavizado")]
    [Tooltip("Velocidad de seguimiento (mayor = más pegado al jugador)")]
    [SerializeField] private float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(
            target.position.x + offsetX,
            target.position.y + offsetY,
            transform.position.z  // mantener la Z de la cámara
        );

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}