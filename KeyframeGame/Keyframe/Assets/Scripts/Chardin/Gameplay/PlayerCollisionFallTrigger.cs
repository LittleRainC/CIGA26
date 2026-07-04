using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerCollisionFallTrigger : MonoBehaviour
{
    [SerializeField] FallTarget fallTarget;
    [SerializeField] bool debugLog;

    Collider2D zoneCollider;
    Collider2D playerCollider;
    bool playerTouching;

    void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (fallTarget == null)
        {
            fallTarget = FindObjectOfType<FallTarget>();
        }

        ResolvePlayerCollider();

        if (fallTarget == null)
        {
            Debug.LogWarning($"{nameof(PlayerCollisionFallTrigger)} on {name}: 请指定 Fall Target（Trap (2) 上的 FallTarget）。", this);
        }

        if (playerCollider == null)
        {
            Debug.LogWarning($"{nameof(PlayerCollisionFallTrigger)} on {name}: Player 需要 Tag 为 Player。", this);
        }
    }

    void Update()
    {
        if (fallTarget == null || zoneCollider == null || playerCollider == null)
        {
            return;
        }

        bool touching = IsPlayerInside();
        if (touching == playerTouching)
        {
            return;
        }

        playerTouching = touching;
        fallTarget.SetHeld(playerTouching);

        if (debugLog)
        {
            Debug.Log($"[PlayerCollisionFallTrigger] Player {(playerTouching ? "进入" : "离开")} {name} → Trap {(playerTouching ? "Dynamic" : "Kinematic")}", this);
        }
    }

    bool IsPlayerInside()
    {
        Bounds bounds = zoneCollider.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (IsPlayerCollider(hits[i]))
            {
                return true;
            }
        }

        return false;
    }

    void ResolvePlayerCollider()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            playerObject = GameObject.Find("Player");
        }

        if (playerObject != null)
        {
            playerCollider = playerObject.GetComponent<Collider2D>();
        }
    }

    bool IsPlayerCollider(Collider2D col)
    {
        return col != null && (col.CompareTag("Player") || col.gameObject.name == "Player");
    }
}
