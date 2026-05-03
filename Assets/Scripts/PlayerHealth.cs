using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int health = 3;
    public float knockbackForce = 15f;
    public float knockbackDuration = 0.2f;
    public bool KBFromRight;

    private Rigidbody2D rb;
    private Move playermovement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playermovement = GetComponent<Move>();
    }



    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log("Vida: " + health);

        if (health <= 0) Die();
    }


    void Die()
    {
        Debug.Log("Reiniciando nivel...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}