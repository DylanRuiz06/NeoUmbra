using System.Collections;
using UnityEngine;

public class EnemyInactive : MonoBehaviour
{
    // Llamado por EnemyDimensionShifter.OnSteal() justo al instanciar
    public void Init(float recoveryTime, GameObject originalPrefab, GameObject activeInstance)
    {
        StartCoroutine(RecoveryRoutine(recoveryTime, originalPrefab, activeInstance));
    }

    private IEnumerator RecoveryRoutine(float recoveryTime, GameObject originalPrefab, GameObject activeInstance)
    {
        yield return new WaitForSeconds(recoveryTime);

        // Reactivar el enemigo original en la misma posición
        activeInstance.transform.position = transform.position;
        activeInstance.SetActive(true);

        // Destruir este prefab inactivo
        Destroy(gameObject);
    }
}