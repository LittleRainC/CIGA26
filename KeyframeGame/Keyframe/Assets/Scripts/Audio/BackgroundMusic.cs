using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] AudioClip clip;
    [SerializeField] bool loop = true;
    [SerializeField] [Range(0f, 1f)] float volume = 0.5f;

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
        source.spatialBlend = 0f;
        source.clip = clip;
        source.Play();
    }
}
