using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level1";

    private void OnMouseDown()
    {
        Debug.Log("StartButton clicked");
        SceneManager.LoadScene(sceneName);
    }
}
