using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SwitchPlate : MonoBehaviour
{
    public enum VisibilityMode
    {
        HideWhenPressed,
        ShowWhenPressed
    }

    [Header("References")]
    [SerializeField] Transform switchTransform;
    [SerializeField] Transform targetToMove;

    [Header("Visibility")]
    [Tooltip("HideWhenPressed：碰到消失，不碰出现。ShowWhenPressed：反过来。")]
    [SerializeField] VisibilityMode visibilityMode = VisibilityMode.HideWhenPressed;

    [Header("Timeline")]
    [SerializeField] TimelineSystem timelineSystem;

    Collider2D plateCollider;
    Collider2D switchCollider;
    int switchOverlapCount;

    void Awake()
    {
        if (timelineSystem == null)
        {
            timelineSystem = TimelineSystem.Instance;
        }

        if (switchTransform == null)
        {
            GameObject switchObject = GameObject.Find("Switch");
            if (switchObject != null)
            {
                switchTransform = switchObject.transform;
            }
        }

        plateCollider = GetComponent<Collider2D>();
        if (plateCollider != null)
        {
            plateCollider.isTrigger = true;
        }

        EnsureKinematicRigidbody(gameObject);

        if (switchTransform != null)
        {
            EnsureKinematicRigidbody(switchTransform.gameObject);
            switchCollider = switchTransform.GetComponent<Collider2D>();
        }
    }

    void Start()
    {
        switchOverlapCount = 0;
        ApplyVisibility(false);
    }

    void OnEnable()
    {
        if (timelineSystem != null)
        {
            timelineSystem.TimelineReset += HandleTimelineReset;
        }
    }

    void OnDisable()
    {
        if (timelineSystem != null)
        {
            timelineSystem.TimelineReset -= HandleTimelineReset;
        }
    }

    void Update()
    {
        if (!Application.isPlaying || plateCollider == null || switchCollider == null)
        {
            return;
        }

        bool switchOnPlate = plateCollider.IsTouching(switchCollider);
        if (switchOnPlate && switchOverlapCount == 0)
        {
            switchOverlapCount = 1;
            ApplyVisibility(true);
        }
        else if (!switchOnPlate && switchOverlapCount > 0)
        {
            switchOverlapCount = 0;
            ApplyVisibility(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsSwitchCollider(other))
        {
            return;
        }

        switchOverlapCount++;
        ApplyVisibility(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsSwitchCollider(other))
        {
            return;
        }

        switchOverlapCount = Mathf.Max(0, switchOverlapCount - 1);
        if (switchOverlapCount == 0)
        {
            ApplyVisibility(false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsSwitchCollider(collision.collider))
        {
            return;
        }

        switchOverlapCount++;
        ApplyVisibility(true);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!IsSwitchCollider(collision.collider))
        {
            return;
        }

        switchOverlapCount = Mathf.Max(0, switchOverlapCount - 1);
        if (switchOverlapCount == 0)
        {
            ApplyVisibility(false);
        }
    }

    void HandleTimelineReset()
    {
        switchOverlapCount = 0;
        ApplyVisibility(false);
    }

    void ApplyVisibility(bool switchPressed)
    {
        if (targetToMove == null)
        {
            return;
        }

        bool visible = visibilityMode == VisibilityMode.HideWhenPressed
            ? !switchPressed
            : switchPressed;

        targetToMove.gameObject.SetActive(visible);
    }

    bool IsSwitchCollider(Collider2D other)
    {
        if (switchTransform == null)
        {
            return other.transform.name == "Switch" || other.transform.root.name == "Switch";
        }

        Transform otherTransform = other.transform;
        return otherTransform == switchTransform || otherTransform.IsChildOf(switchTransform);
    }

    static void EnsureKinematicRigidbody(GameObject target)
    {
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = target.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
