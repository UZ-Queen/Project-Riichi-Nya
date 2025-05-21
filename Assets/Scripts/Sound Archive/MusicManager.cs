using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioClip menuMusic;
    [SerializeField] AudioClip mainMusic;

    string sceneName;
    
    void OnEnable(){
        SceneManager.activeSceneChanged += OnSceneLoaded;
        // Debug.Log("뮤직매니저가 액티브씬체인지에 이벤트 등록함!" + gameObject.name);
    }
    void OnDisable(){
        SceneManager.activeSceneChanged -= OnSceneLoaded;
        // Debug.Log("뮤직매니저가 액티브씬체인지에 이벤트 등록 해제함"  + gameObject.name);
    }

    void OnSceneLoaded(Scene prev, Scene current){
        string newSceneName = current.name;

        if(newSceneName == sceneName){
            Debug.Log($"{newSceneName}, {sceneName} 이 같대요! 뿅!"  + gameObject.name);
            return;
        }
        Debug.Log($"{newSceneName}, {sceneName} 이 다르대요 뿅!"  + gameObject.name);
        sceneName = newSceneName;
        PlayMusic();
    }



    void PlayMusic(){
        Debug.Log("노래를 바꿉니다!");
        AudioClip music = null;
        // Scene currentScene = SceneManager.GetActiveScene();
        // if(currentScene.name == SceneName.mainMenu){
        //     music = menuMusic;
        // }
        // else if(currentScene.name == SceneName.level){
        //     music = mainMusic;
        // }
        
        if(music == null){
            return;
        }

        AudioManager.instance.PlayMusic(music, 2f);
        Invoke(nameof(PlayMusic), music.length);



    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.RightBracket)){
            AudioManager.instance.PlayMusic(mainMusic, 3f);
        }
    }
}
