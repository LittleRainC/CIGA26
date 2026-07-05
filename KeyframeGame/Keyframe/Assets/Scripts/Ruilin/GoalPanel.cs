using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalPanel : MonoBehaviour
{
    public static GoalPanel Instance { get; private set; }

    [SerializeField] string nextSceneName;
    [SerializeField] Button nextButton;
    [SerializeField] [Range(0f, 1f)] float volume = 1f;

    AudioSource audioSource;
    bool isReady;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple GoalPanel instances found. Destroying duplicate.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureReady();
    }

    void OnDestroy()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(GoToNextLevel);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    void EnsureReady()
    {
        if (isReady)
        {
            return;
        }

        isReady = true;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(GoToNextLevel);
            nextButton.onClick.AddListener(GoToNextLevel);
        }
    }

    public void Show()
    {
        EnsureReady();
        gameObject.SetActive(true);
    }

    public void GoToNextLevel()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("Next Scene Name is empty. Set it on GoalPanel in the Inspector.", this);
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
