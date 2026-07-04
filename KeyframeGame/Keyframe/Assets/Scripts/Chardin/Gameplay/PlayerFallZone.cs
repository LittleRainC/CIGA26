using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerFallZone : MonoBehaviour
{
    [SerializeField] FallTarget target;

    int playerCount;

    void Awake()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerCount++;
        if (target != null)
        {
            target.SetHeld(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerCount = Mathf.Max(0, playerCount - 1);
        if (playerCount == 0 && target != null)
        {
            target.SetHeld(false);
        }
    }
}
