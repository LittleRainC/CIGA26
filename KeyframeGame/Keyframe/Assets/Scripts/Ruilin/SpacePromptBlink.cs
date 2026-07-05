using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SpacePromptBlink : MonoBehaviour
{
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;
        canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
    }
}
