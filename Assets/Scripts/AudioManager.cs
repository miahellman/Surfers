using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public AudioClip clip;
        public string name;
    }

    [SerializeField] Sound[] sounds;

    AudioSource source;
    public static AudioManager instance;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        instance = this;
    }

    public void PlaySound(string _name, float volume = 0.5f, float pitch = 1f, bool randomPitch = false)
    {
        foreach (Sound sound in sounds)
        {
            if (sound.name == _name)
            {
                if (randomPitch)
                {
                    float rPitch = Random.Range(0.8f, 1.2f);
                    source.pitch = rPitch;
                }
                else
                {
                    source.pitch = pitch;
                }

                source.PlayOneShot(sound.clip, volume);
                break;
            }
        }
    }
}
