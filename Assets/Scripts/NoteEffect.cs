using UnityEngine;

public class NoteEffect : MonoBehaviour
{
    private Vector3 velocity;
    private float minX = -3f;
    private float maxX = 3f;
    private float lifeTime = 1f;
    private float fadeSpeed;
    private SpriteRenderer sr;

    public void Initialize(Vector3 startPos, float angle, float speed)
    {
        transform.position = startPos;
        
        // Convert angle to direction vector
        float rad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        
        velocity = new Vector3(direction.x, direction.y, 0) * speed;
        sr = GetComponent<SpriteRenderer>();
        fadeSpeed = 1f / lifeTime;
        
        // Initial look: Gray and semi-transparent
        if (sr != null)
        {
            sr.color = new Color(0.5f, 0.5f, 0.5f, 2f);
        }
    }

    void Update()
    {
        // Movement
        transform.position += velocity * Time.deltaTime;

        // Bouncing logic
        if (transform.position.x <= minX || transform.position.x >= maxX)
        {
            velocity.x *= -1;
            // Clamp position to prevent getting stuck
            float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
        }

        // Fade out
        if (sr != null)
        {
            Color c = sr.color;
            c.a -= fadeSpeed * Time.deltaTime;
            sr.color = c;
        }

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
