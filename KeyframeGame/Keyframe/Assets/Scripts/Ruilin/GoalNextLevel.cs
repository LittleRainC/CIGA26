using UnityEngine;

public class GoalNextLevel : MonoBehaviour
{
    [SerializeField] GoalPanel goalPanel;
    [SerializeField] AudioClip completeClip;

    bool completed;

    void Awake()
    {
        ResolveGoalPanel();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (completed || !other.CompareTag("Player"))
        {
            return;
        }

        ResolveGoalPanel();
        if (goalPanel == null)
        {
            Debug.LogWarning("GoalPanel not found. Add a GoalPanel under Canvas in this scene.", this);
            return;
        }

        completed = true;
        goalPanel.Show(completeClip);
    }

    void ResolveGoalPanel()
    {
        if (goalPanel != null)
        {
            return;
        }

        if (GoalPanel.Instance != null)
        {
            goalPanel = GoalPanel.Instance;
            return;
        }

        goalPanel = FindObjectOfType<GoalPanel>(true);
    }
}
