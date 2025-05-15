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

public enum PlayerCallType
{
    Riichi, Tsumo, Ron, Chii, Pon, Kan, Nukidora
}

public enum GameState{
    Initializing, PlayerTurn, GameOver, Processing, MOLLU,
}
