using UnityEngine;

public class FallTarget : MonoBehaviour
{
    Rigidbody2D rb;
    Vector3 startPosition;
    Quaternion startRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }

        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        startPosition = transform.position;
        startRotation = transform.rotation;
        Freeze();
    }

    void OnEnable()
    {
        TimelineSystem system = TimelineSystem.Instance;
        if (system != null)
        {
            system.TimelineReset += HandleTimelineReset;
        }
    }

    void OnDisable()
    {
        TimelineSystem system = TimelineSystem.Instance;
        if (system != null)
        {
            system.TimelineReset -= HandleTimelineReset;
        }
    }

    public void SetHeld(bool held)
    {
        if (held)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.WakeUp();
        }
        else
        {
            transform.SetPositionAndRotation(startPosition, startRotation);
            Freeze();
        }
    }

    void HandleTimelineReset()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
        Freeze();
    }

    void Freeze()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;
    }
}
