using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class ButtonClickAudio : MonoBehaviour
{
    [SerializeField] AudioClip clip;
    [SerializeField] [Range(0f, 1f)] float volume = 1f;

    Button button;
    AudioSource source;

    void Awake()
    {
        button = GetComponent<Button>();
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        button.onClick.AddListener(PlayClick);
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClick);
        }
    }

    void PlayClick()
    {
        if (clip == null || source == null)
        {
            return;
        }

        source.PlayOneShot(clip, volume);
    }
}
