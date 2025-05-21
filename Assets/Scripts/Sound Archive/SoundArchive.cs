using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundArchive : MonoBehaviour
{

    [SerializeField] private AudioClip[] fallBackClips;
    [SerializeField] private AudioGroup[] audioGroups;
    Dictionary<string, AudioClip[]> audioArchive = new Dictionary<string, AudioClip[]>();
    // Start is called before the first frame update
    void Awake()
    {
        foreach(AudioGroup audioGroup in audioGroups){
            audioArchive.Add(audioGroup.groupName, audioGroup.clips);
        }
    }


    public AudioClip GetAudioClipByName(string name, int index = -1){
        if(!audioArchive.ContainsKey(name)){
            AudioClip fallback = fallBackClips[Random.Range(0, fallBackClips.Length)];
            Debug.LogWarning($"{name} 사운드클립을 찾지 못했습니다!");
             return fallback;
        }
  

        AudioClip randomClip; 
        int length = audioArchive[name].Length;
        if( index <0 )
            randomClip = audioArchive[name][Random.Range(0, length)];
        else{
            index = Mathf.Clamp(index, 0, length-1);
            randomClip = audioArchive[name][index];
        }
        return randomClip;
    }

    [System.Serializable]
    public class AudioGroup{
        public string groupName;
        public AudioClip[] clips;
    }
}
