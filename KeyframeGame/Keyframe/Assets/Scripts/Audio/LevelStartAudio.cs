using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LevelStartAudio : MonoBehaviour
{
    [SerializeField] AudioClip clip;
    [SerializeField] bool loop;
    [SerializeField] [Range(0f, 1f)] float volume = 1f;

    void Start()
    {
        if (clip == null)
        {
            return;
        }

        AudioSource source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.volume = volume;
        source.clip = clip;
        source.Play();
    }
}
