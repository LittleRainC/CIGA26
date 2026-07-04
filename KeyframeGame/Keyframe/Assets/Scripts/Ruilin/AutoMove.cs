using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AutoMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 3f;

    [Header("Timeline")]
    [SerializeField] TimelineSystem timelineSystem;

    Rigidbody2D rb;
    Vector3 startPosition;
    Quaternion startRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (timelineSystem == null)
        {
            timelineSystem = TimelineSystem.Instance;
        }
    }

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        if (timelineSystem != null)
        {
            timelineSystem.TimelineReset += HandleTimelineReset;
        }
    }

    void OnDestroy()
    {
        if (timelineSystem != null)
        {
            timelineSystem.TimelineReset -= HandleTimelineReset;
        }
    }

    void FixedUpdate()
    {
        rb.angularVelocity = 0f;

        float horizontalSpeed = timelineSystem != null && timelineSystem.IsPlaying
            ? moveSpeed
            : 0f;

        rb.velocity = new Vector2(horizontalSpeed, rb.velocity.y);
    }

    void HandleTimelineReset()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
}
