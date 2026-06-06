using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int health = 5;
    public float knockbackForce = 15f;
    public float knockbackDuration = 0.2f;
    public bool KBFromRight;

    private Rigidbody2D rb;
    private Move playermovement;

    public int maxHealth = 5;
    public int currentHealth;

    public HealthBar healthBar;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playermovement = GetComponent<Move>();
        healthBar.SetMaxHealth(maxHealth);
    }



    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log("Vida: " + health);
        healthBar.SetHealth(health);
        if (health <= 0) Die();
    }


    void Die()
    {
        Debug.Log("Reiniciando nivel...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}