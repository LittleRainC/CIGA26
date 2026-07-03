using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimelineRulerUI : MonoBehaviour
{
    [SerializeField] TimelineSystem timelineSystem;
    [SerializeField] RectTransform rulerArea;
    [SerializeField] TextMeshProUGUI tickLabelPrefab;
    [SerializeField] float labelTopPadding;

    readonly List<TextMeshProUGUI> tickLabels = new List<TextMeshProUGUI>();

    void Awake()
    {
        if (timelineSystem == null)
        {
            timelineSystem = TimelineSystem.Instance;
        }

        if (rulerArea == null)
        {
            rulerArea = transform as RectTransform;
        }
    }

    void OnEnable()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        ClearTicks();

        if (timelineSystem == null || rulerArea == null || tickLabelPrefab == null)
        {
            return;
        }

        float interval = timelineSystem.GridInterval;
        if (interval <= Mathf.Epsilon)
        {
            return;
        }

        for (float time = 0f; time <= timelineSystem.Duration + 0.001f; time += interval)
        {
            TextMeshProUGUI label = Instantiate(tickLabelPrefab, rulerArea);
            label.gameObject.SetActive(true);
            label.text = $"{time:0}s";

            RectTransform rect = label.rectTransform;
            float normalized = timelineSystem.TimeToNormalized(time);

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(
                normalized * rulerArea.rect.width,
                -labelTopPadding);
            tickLabels.Add(label);
        }
    }

    void ClearTicks()
    {
        for (int i = 0; i < tickLabels.Count; i++)
        {
            if (tickLabels[i] != null)
            {
                Destroy(tickLabels[i].gameObject);
            }
        }

        tickLabels.Clear();
    }
}
