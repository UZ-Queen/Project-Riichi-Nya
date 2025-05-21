using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;



// 오디오매니저의 역할은 효과음과 음악을 재생하는 것!
public class AudioManager : MonoBehaviour
{
    public enum AudioChannel { Master, Music, Sfx };
    // [SerializeField] private AudioChannel channel = AudioChannel.Master;
    [SerializeField] public float masterVolume { get; private set; } = 1.0f;
    [SerializeField] public float musicVolume { get; private set; } = 0.6f;
    [SerializeField] public float sfxVolume { get; private set; } = 1.0f;



    SoundArchive soundArchive;
    private Transform audioListener;
    Transform playerT;
    AudioSource[] audioSources;
    AudioSource sfx2DSource;
    int currentAudioSrcIndex = 0;
    public static AudioManager instance;



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        soundArchive = GetComponent<SoundArchive>();

        audioListener = FindObjectOfType<AudioListener>().transform;
        transform.SetParent(audioListener);
        audioSources = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject audioSource = new GameObject($"오디오 아하({i})");
            audioSources[i] = audioSource.AddComponent<AudioSource>();
            audioSources[i].transform.SetParent(transform);
        }

        GameObject audioSource2D = new GameObject("2D용 오디오 아하");
        sfx2DSource = audioSource2D.AddComponent<AudioSource>();
        sfx2DSource.transform.SetParent(transform);

        //이렇게 쓰는거였군요 형님!!
        masterVolume = PlayerPrefs.GetFloat("Master Vol", masterVolume);
        musicVolume = PlayerPrefs.GetFloat("Music Vol", musicVolume);
        sfxVolume = PlayerPrefs.GetFloat("Sfx Vol", sfxVolume);


        

    }

    void Start(){
    }

    public void SetVolume(float percent, AudioChannel channel)
    {
        percent = Mathf.Clamp(percent, 0f, 1f);
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolume = percent;
                break;
            case AudioChannel.Music:
                musicVolume = percent;
                break;
            case AudioChannel.Sfx:
                sfxVolume = percent;
                break;
        }


        audioSources[0].volume = masterVolume * musicVolume;
        audioSources[1].volume = masterVolume * musicVolume;

        PlayerPrefs.SetFloat("Master Vol", masterVolume);
        PlayerPrefs.SetFloat("Music Vol", musicVolume);
        PlayerPrefs.SetFloat("Sfx Vol", sfxVolume);
        PlayerPrefs.Save();
    }

    public void PlaySFX(AudioClip clip, Vector3 pos)
    {
        if (clip == null)
            return;
        AudioSource.PlayClipAtPoint(clip, pos, masterVolume * sfxVolume);
    }

    public void PlaySFX(string clipName, Vector3 pos, int index = -1)
    {
        PlaySFX(soundArchive.GetAudioClipByName(clipName, index), pos);
    }

    public void PlaySFX2D(string clipName, int index = -1)
    {
        sfx2DSource.PlayOneShot(soundArchive.GetAudioClipByName(clipName, index), masterVolume * sfxVolume);
    }

    public void PlayMusic(AudioClip clip, float fadeDuration)
    {
        if (clip == null)
            return;
        currentAudioSrcIndex = 1 - currentAudioSrcIndex; // 1이면 0이 되고 0이면 1이 됨.

        audioSources[currentAudioSrcIndex].clip = clip;
        audioSources[currentAudioSrcIndex].Play();
        StartCoroutine(FadeAudio(fadeDuration));
    }

    IEnumerator FadeAudio(float duration)
    {

        float percent = 0;
        while (percent <= 1)
        {
            percent += Time.deltaTime * (1 / duration);
            audioSources[currentAudioSrcIndex].volume = Mathf.Lerp(0, masterVolume * musicVolume, percent);
            audioSources[1 - currentAudioSrcIndex].volume = Mathf.Lerp(masterVolume * musicVolume, 0, percent);
            yield return null;

        }
        audioSources[1 - currentAudioSrcIndex].Stop();
    }
    void Update()
    {
    }



}
