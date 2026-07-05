using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AutoMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 3f;

    [Header("Timeline")]
    [SerializeField] TimelineSystem timelineSystem;

    [Header("Trap Death")]
    [SerializeField] AudioClip trapDeathClip;
    [SerializeField] [Range(0f, 1f)] float trapDeathVolume = 1f;

    Rigidbody2D rb;
    AudioSource audioSource;
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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
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

    void OnTriggerEnter2D(Collider2D other)
    {
        TryPlayTrapDeathSound(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryPlayTrapDeathSound(collision.gameObject);
    }

    void TryPlayTrapDeathSound(GameObject other)
    {
        if (trapDeathClip == null || audioSource == null || other == null)
        {
            return;
        }

        if (other.GetComponentInParent<SpikeTrap>() == null)
        {
            return;
        }

        audioSource.PlayOneShot(trapDeathClip, trapDeathVolume);
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
