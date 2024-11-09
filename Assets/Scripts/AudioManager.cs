using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] Sound[] sounds;

    AudioSource source;
    public static AudioManager instance;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        if (instance == null) { instance = this; }
    }

    public void PlaySound(string _name, float volume = 0.75f, float pitch = 1f, bool randomPitch = false)
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
                return;
            }
        }
        Debug.LogWarning(_name + " is not a valid sound in AudioManager");
    }

    public void PlayGrind()
    {
        source.Play();
    }

    public void StopGrind()
    {
        source.Stop();
    }

    //public void PlayContinuousSound(string _name, float volume = 0.75f, float pitch = 1f, bool randomPitch = false)
    //{
        
    //}

    //public void StopContinuousSound()
    //{
    //    source.Stop();
    //}
}
