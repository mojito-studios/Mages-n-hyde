using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    public static BackgroundMusicController instance;
    private double musicDuration;
    private double goalTime;
    private int audioToggle = 0;
    public AudioSource[] _audioSources;
    [SerializeField] private AudioClip currentClip;

    private void Awake()
    {
        if (instance != null && instance != this)
        { 
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            _audioSources[audioToggle].loop = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _audioSources[audioToggle].Play();
    }

    // Update is called once per frame
    private void Update()
    {
        /*if(AudioSettings.dspTime > goalTime - 1)
        {
            PlayScheduledClip();
        }*/
    }
    private void PlayScheduledClip()
    {
        //_audioSources[1 - audioToggle].Pause();
        _audioSources[audioToggle].clip = currentClip;
        _audioSources[audioToggle].PlayScheduled(goalTime);

        musicDuration = (double)currentClip.samples / currentClip.frequency;
        goalTime = goalTime + musicDuration;

        audioToggle = 1 - audioToggle;
    }

    public void SetCurrentClip(AudioClip clip)
    {
        currentClip = clip;
        _audioSources[audioToggle].clip = currentClip;
        _audioSources[audioToggle].Play();
        //PlayScheduledClip();
    }
}
