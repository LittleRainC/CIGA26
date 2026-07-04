using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalNextLevel : MonoBehaviour
{
    [SerializeField] private string nextSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        SceneManager.LoadScene(nextSceneName);
    }
}