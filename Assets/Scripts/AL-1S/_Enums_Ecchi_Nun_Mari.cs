using System;



public enum Wind { Ton, Nan, Sha, Pei, MOLLU };

public enum UniqueName
{
    Jjuna = 0,
    Nyangan = 2000,
    Henenyan = 3000,
    Bainyan = 4000,
    Sanbainyan = 6000,
    Yakunyan = 8000,
    DoubleYakunyan = 16000,
    TripleYakunyan = 24000,
    YonbaiYakunyan = 32000,
    GobaiYakunyan = 40000,
    RyokubaiYakunyan = 48000,
}

/// <summary>
/// 플레이어가 마작 규칙에 전달할 수 있는 선언 종류입니다.
/// </summary>
public enum PlayerCallType
{
    Riichi,
    Tsumo,
    Ron,
    Chii,
    Pon,
    Kan,
    Nukidora,
}

public enum GameState{
    Initializing, PlayerTurn, GameOver, Processing, MOLLU,
}
