using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;


public static class SettingsManager
{
    private static readonly string SaveFilePath =
            Path.Combine(Application.persistentDataPath, "yaml.json");

    public static void Save(PetitGameSaveData data)
    {
        try
        {
            // 예쁘게 들여쓰기 된 JSON으로 변환
            string json = JsonConvert.SerializeObject(
                data,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    // 필요시 순환 참조 방지나 커스텀 컨버터 설정 가능
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }
            );

            File.WriteAllText(SaveFilePath, json);
            MyLogger.Log($"저장 완료! {SaveFilePath}");
        }
        catch (IOException e)
        {
            MyLogger.LogError($"끄앙!! 저장 실패했습니다! {e}");
        }
    }

    public static PetitGameSaveData Load()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.LogWarning($"[Load] Save file not found at {SaveFilePath}, creating new data.");
            return new PetitGameSaveData();
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            var data = JsonConvert.DeserializeObject<PetitGameSaveData>(json);
            if (data == null)
            {
                Debug.LogWarning("[Load] Deserialized data was null, returning new instance.");
                return new PetitGameSaveData();
            }
            return data;
        }
        catch (IOException e)
        {
            Debug.LogError($"[Load] Failed to read save file: {e}\nReturning new data instance.");
            return new PetitGameSaveData();
        }
        catch (JsonException e)
        {
            Debug.LogError($"[Load] JSON parse error: {e}\nReturning new data instance.");
            return new PetitGameSaveData();
        }
    }

    /// <summary>
    /// 저장 파일을 삭제합니다. (테스트나 초기화용)
    /// </summary>
    public static void DeleteSave()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log($"[Delete] Save file deleted: {SaveFilePath}");
        }
        else
        {
            Debug.LogWarning($"[Delete] No save file to delete at {SaveFilePath}");
        }
    }
}


// public partial class SettingsManager : MonoBehaviour
// {
//     public static SettingsManager Instance { get; private set; }
//     private const string FILENAME = "game_settings.json";
//     private string _path;

//     public GameSettings Settings { get; private set; }

//     void Awake()
//     {
//         if (Instance != null && Instance != this) { Destroy(gameObject); return; }
//         Instance = this;
//         DontDestroyOnLoad(gameObject);
//         _path = Path.Combine(Application.persistentDataPath, FILENAME);
//         Load();
//     }

//     public void Save()
//     {
//         var json = JsonUtility.ToJson(Settings, true);
//         File.WriteAllText(_path, json);
//     }

//     public void Load()
//     {
//         if (File.Exists(_path))
//         {
//             var json = File.ReadAllText(_path);
//             Settings = JsonUtility.FromJson<GameSettings>(json);
//         }
//         else
//         {
//             Settings = new GameSettings();
//             Save();
//         }

//         ApplyAll();
//     }

//     // 로드 직후 한 번만, 혹은 설정 변경 직후 호출
//     private void ApplyAll()
//     {
//         // 사운드
//         var sm = AudioManager.instance;
//         sm.SetVolume(Settings.sound.masterVolume, AudioManager.AudioChannel.Master);
//         sm.SetVolume(Settings.sound.musicVolume, AudioManager.AudioChannel.Music);
//         sm.SetVolume(Settings.sound.sfxVolume, AudioManager.AudioChannel.Sfx);

//         // 입력(키 바인딩)
//         // InputManager.Instance.SetBindings(Settings.input.keyBindings);
//     }
// }
