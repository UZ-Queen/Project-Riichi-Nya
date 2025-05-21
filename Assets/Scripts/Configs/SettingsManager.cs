using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    private const string FILENAME = "game_settings.json";
    private string _path;

    public GameSettings Settings { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _path = Path.Combine(Application.persistentDataPath, FILENAME);
        Load();
    }

    public void Save()
    {
        var json = JsonUtility.ToJson(Settings, true);
        File.WriteAllText(_path, json);
    }

    public void Load()
    {
        if (File.Exists(_path))
        {
            var json = File.ReadAllText(_path);
            Settings = JsonUtility.FromJson<GameSettings>(json);
        }
        else
        {
            Settings = new GameSettings();
            Save();
        }

        ApplyAll();
    }

    // 로드 직후 한 번만, 혹은 설정 변경 직후 호출
    private void ApplyAll()
    {
        // 사운드
        var sm = AudioManager.instance;
        sm.SetVolume(Settings.sound.masterVolume, AudioManager.AudioChannel.Master);
        sm.SetVolume(Settings.sound.musicVolume,  AudioManager.AudioChannel.Music);
        sm.SetVolume(Settings.sound.sfxVolume,    AudioManager.AudioChannel.Sfx);

        // 입력(키 바인딩)
        // InputManager.Instance.SetBindings(Settings.input.keyBindings);
    }
}
