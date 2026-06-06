using UnityEngine;

public class dash : MonoBehaviour
{
    private Rigidbody2D rb2d;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            rb2d.linearVelocity = new Vector2(100, 0);
        }
    }
}
