using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image endingImage;
    [SerializeField] private GameObject spacePrompt;

    [Header("Sprites")]
    [SerializeField] private Sprite image1;
    [SerializeField] private Sprite image2;
    [SerializeField] private Sprite image3;

    [Header("Settings")]
    [SerializeField] private float displayTime = 1f;
    [SerializeField] private float spaceDelay = 2f;

    private bool canReturn = false;

    void Start()
    {
        spacePrompt.SetActive(false);
        StartCoroutine(PlayEnding());
    }

    void Update()
    {
        if (canReturn && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Menu");
        }
    }

    IEnumerator PlayEnding()
    {
        endingImage.sprite = image1;
        yield return new WaitForSeconds(displayTime);

        endingImage.sprite = image2;
        yield return new WaitForSeconds(displayTime);

        endingImage.sprite = image3;

        yield return new WaitForSeconds(spaceDelay);

        spacePrompt.SetActive(true);
        canReturn = true;
    }
}