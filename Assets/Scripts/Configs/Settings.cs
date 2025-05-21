using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SoundSettings
{
    public float masterVolume = 1f;
    public float musicVolume  = 1f;
    public float sfxVolume    = 1f;
}

[Serializable]
public class GraphicsSettings
{
    public int   resolutionIndex = 0;
    public bool fullscreen      = false;
}

[Serializable]
public class InputSettings
{
    // 예: 게임 동작(Action) → 키 코드 매핑
    public Dictionary<string, KeyCode> keyBindings
        = new Dictionary<string, KeyCode>()
        {
            [InputLists.MoveLeft] = KeyCode.A,
            [InputLists.MoveRight] = KeyCode.D,
            [InputLists.Tedashi] = KeyCode.W,
            [InputLists.Tsumogiri] = KeyCode.Q,
            [InputLists.Tsumo] = KeyCode.Space,
        };
        public float DAS = 0.133f;
        public float ARR = 0.017f;
}



[Serializable]
public class GameSettings
{
    public SoundSettings    sound    = new SoundSettings();
    // public GraphicsSettings graphics = new GraphicsSettings();
    public InputSettings    input    = new InputSettings();
}

