using System.Collections;
using UnityEngine;

public class GoalNextLevel : MonoBehaviour
{
    [SerializeField] GoalPanel goalPanel;
    [SerializeField] AudioClip completeClip;
    [SerializeField] [Range(0f, 1f)] float completeVolume = 1f;
    [SerializeField] float showDelay = 1f;

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
        TimelineSystem.Instance?.Pause();

        Rigidbody2D playerRb = other.attachedRigidbody;
        if (playerRb != null)
        {
            playerRb.velocity = new Vector2(0f, playerRb.velocity.y);
        }

        PlayCompleteClip();
        StartCoroutine(ShowPanelAfterDelay());
    }

    void PlayCompleteClip()
    {
        if (completeClip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(completeClip, transform.position, completeVolume);
    }

    IEnumerator ShowPanelAfterDelay()
    {
        if (showDelay > 0f)
        {
            yield return new WaitForSeconds(showDelay);
        }

        if (goalPanel != null)
        {
            goalPanel.Show();
        }
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
