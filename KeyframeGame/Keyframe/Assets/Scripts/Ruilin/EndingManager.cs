using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class EndingManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image endingImage;
    [SerializeField] private GameObject spacePrompt;

    [Header("Sprites")]
    [SerializeField] private Sprite image1;
    [SerializeField] private Sprite image2;
    [SerializeField] private Sprite image3;

    [Header("Audio")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private bool loopMusic = true;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 1f;

    [Header("Settings")]
    [SerializeField] private float displayTime = 1f;
    [SerializeField] private float spaceDelay = 2f;

    private bool canReturn = false;
    private AudioSource musicSource;

    void Awake()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
    }

    void Start()
    {
        spacePrompt.SetActive(false);
        PlayMusic();
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

    void PlayMusic()
    {
        if (musicClip == null || musicSource == null)
        {
            return;
        }

        musicSource.clip = musicClip;
        musicSource.loop = loopMusic;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }
}