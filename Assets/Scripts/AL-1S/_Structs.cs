using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

    [Serializable]
    class PanelEntry
    {
        public UIState state;
        public RectTransform rect;
        public CanvasGroup group;
        public Wind appearFromWhere;

        [HideInInspector]public Vector2 originalPosition; // 원래 위치를 기억하기 위해..
    }

[Serializable]
class GamePanelEntry
{
    public GameUIState state;
    public RectTransform rect;
    public CanvasGroup group;
    public Wind appearFromWhere;

    [HideInInspector] public Vector2 originalPosition; // 원래 위치를 기억하기 위해..
    public GamePanelEntry(
        GameUIState   state,
        RectTransform rect,
        CanvasGroup   group,
        Wind          appearFromWhere,
        Vector2       originalPosition
    )
    {
        this.state             = state;
        this.rect              = rect;
        this.group             = group;
        this.appearFromWhere   = appearFromWhere;
        this.originalPosition  = originalPosition;
    }
    }


public class MahjongRoundInfo
{
    public int guk;
    public int bonzang;
    public int riichiBong;
    // public int oyaIndex;
    public Wind playerWind; // <-- 필요없는데...
    /// <summary>
    /// 이 도라 타일은 UI 공유용입니다. 내부 계산에 쓰지 마세요.
    /// </summary>
    public List<MahjongTile> doraTiles;


    private MahjongRoundInfo(int guk, int bonzang, int riichiBong, Wind playerWind)
    {
        this.guk = guk;
        this.bonzang = bonzang;
        this.riichiBong = riichiBong;
        this.playerWind = playerWind;
        // doraTile = MahjongTile.NullTile();
        doraTiles = new List<MahjongTile>();
    }

    public void AddRiichiBong()
    {
        riichiBong++;
    }
    /// <summary>
    /// 깡을 쳤다거나 할 경우, 추가된 도라룰 여기 넣어주세요!
    /// </summary>
    /// <param name="tiles"></param>
    public void AddNewDoraTiles(MahjongTile tile)
    {
        doraTiles.Add(tile);
    }




    /// <summary>
    /// 이전 라운드와 관계없는 클린한 새 라운드를 생성합니다.
    /// oyaIndex가 0일경우 
    /// </summary>
    /// <param name="oyaIndex"></param>
    /// <returns></returns>
    public static MahjongRoundInfo NewRound(Wind playerWind)
    {
        // Wind newPlayerWind = 
        return new MahjongRoundInfo(1, 0, 0, playerWind);
    }
    /// <summary>
    /// 승리 여부에 따라 본장과 리치봉을 초기화하고 다음 라운드 정보를 반환할 거예요. 리치봉 점수는 여기서 추가하지 않아요.
    /// </summary>
    /// <param name="oyaWin"></param>
    /// <returns></returns>
    public MahjongRoundInfo NextRoundOnWin(bool oyaWin)
    {
        MahjongRoundInfo newinfo = this;
        doraTiles = new List<MahjongTile>();
        if (oyaWin)
        {
            newinfo.bonzang++;
        }
        else
        {
            newinfo.guk++;
            newinfo.bonzang = 0;
            newinfo.ChangeOya();
        }
        newinfo.riichiBong = 0;
        return newinfo;

    }
    /// <summary>
    /// 유국했어요! 다음 라운드 정보를 반환할 거에요. 오야는 무조건 넘어가고, 오야가 텐파이한 경우에만 본장이 늘어나요. 
    /// 텐파이 여부에 따라 점수를.. 줄 필요는 없겠네요 1인 마작이니까.
    /// </summary>
    /// <param name="oyaTenpai"></param>
    /// <returns></returns>
    public MahjongRoundInfo NextRoundOnYuguk(bool oyaTenpai)
    {
        MahjongRoundInfo newinfo = this;
        doraTiles = new List<MahjongTile>();
        if (oyaTenpai)
        {
            // do nothing;
            newinfo.bonzang++;
        }
        else
        {
            newinfo.guk++;
            newinfo.ChangeOya();
        }
        return newinfo;
    }


    void ChangeOya()
    {
        playerWind = (Wind)((int)(playerWind + 1) % 4);
    }
    public Wind RoundWind
    {
        get
        {
            return (Wind)(((guk - 1) / 4) % 4);
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"{RoundWind}{guk}국 {bonzang}본장, 플레이어:{playerWind}\n");
        sb.Append($"도라:");
        foreach (var i in doraTiles)
        {
            sb.Append(i.ToString());
        }
        sb.Append("\n");
        return sb.ToString();
    }
}

public struct WindInfo
{
    public Wind roundWind;
    public Wind playerWind;
    public bool IsValid
    {
        get { return roundWind != Wind.MOLLU && playerWind != Wind.MOLLU; }
    }

    public WindInfo(Wind roundWind, Wind playerWind)
    {
        this.roundWind = roundWind;
        this.playerWind = playerWind;
    }
    public static WindInfo NullInfo()
    {
        return new WindInfo(Wind.MOLLU, Wind.MOLLU);
    }

    public void Debug()
    {
        MyLogger.LogWarning($"플레이어 {playerWind} / 라운드 {roundWind}");
    }
}



public struct TsumoInfo
{
    public bool isRiichiAble;
    public bool isTsumoAble;
    public MahjongTile tsumoTile;

    public Dictionary<int, HashSet<MahjongWinInfo>> removeToTenpaiTile;

    public TsumoInfo(MahjongPlayer player)
    {
        isRiichiAble = player.IsRiichiAble(out removeToTenpaiTile);
        HashSet<MahjongWinInfo> wins;
        isTsumoAble = player.IsTsumoAble(out wins);
        tsumoTile = player.tsumoTile;

    }
}


public struct DoraInfo
{
    public int doraCount;
    public int akadoraCount;
    public int uradoraCount;
    public DoraInfo(int doraCount, int akadoraCount, int uradoraCount)
    {
        this.doraCount = doraCount;
        this.akadoraCount = akadoraCount;
        this.uradoraCount = uradoraCount;
    }

    public int DoranekoMatsuri
    {
        get
        {
            return doraCount + akadoraCount + uradoraCount;
        }
    }
}

// public struct MahjongRoundInfo
// {
//     public int guk;
//     public int bonzang;
//     public int riichiBong;
//     public int oyaIndex;
//     public int seed;

//     private MahjongRoundInfo(int guk, int bonzang, int riichiBong, int oyaIndex, int seed)
//     {
//         this.guk = guk;
//         this.bonzang = bonzang;
//         this.riichiBong = riichiBong;
//         this.oyaIndex = oyaIndex;
//         this.seed = seed;
//     }

//     /// <summary>
//     /// 이전 라운드와 관계없는 클린한 새 라운드를 생성합니다.
//     /// oyaIndex가 0일경우 
//     /// </summary>
//     /// <param name="oyaIndex"></param>
//     /// <returns></returns>
//     public static MahjongRoundInfo NewRound(int oyaIndex, int seed)
//     {
//         oyaIndex = Mathf.Clamp(oyaIndex % 4, 0, 3);
//         return new MahjongRoundInfo(1, 0, 0, oyaIndex, seed);
//     }
//     public MahjongRoundInfo NextRoundOnWin(bool oyaWin)
//     {
//         MahjongRoundInfo newinfo = this;
//         if (oyaWin)
//         {
//             newinfo.bonzang++;
//         }
//         else
//         {
//             newinfo.guk++;
//             newinfo.bonzang = 0;
//             newinfo.ChangeOya();
//         }
//         newinfo.riichiBong = 0;

//         newinfo.seed = GetNextSeed();
//         return newinfo;

//     }
//     public MahjongRoundInfo NextRoundOnYuguk(bool oyaTenpai)
//     {
//         MahjongRoundInfo newinfo = this;
//         if (oyaTenpai)
//         {
//             // do nothing;
//         }
//         else
//         {
//             newinfo.guk++;
//             newinfo.ChangeOya();
//         }
//         newinfo.bonzang++;
//         newinfo.seed = GetNextSeed();
//         return newinfo;
//     }
//     public int GetNextSeed()
//     {
//         System.Random prng = new System.Random(seed);
//         return prng.Next();
//     }

//     void ChangeOya()
//     {
//         oyaIndex = (oyaIndex + 1) % 4;
//     }
//     public Wind currentJang
//     {
//         get
//         {
//             return (Wind)((guk - 1 / 4) % 4);
//         }
//     }

// }


/// <summary>
/// 틱 택 토
/// </summary>
public static class VExtension3
{
    /// <summary>
    /// Wind 방향을 2D 벡터로 변환합니다.
    /// Ton(동)=+X, Nan(남)=-Y, Sha(서)=-X, Pei(북)=+Y
    /// </summary>
    public static Vector2 ToVector2(this Wind wind)
    {
        switch (wind)
        {
            case Wind.Ton: return Vector2.right;
            case Wind.Nan: return Vector2.down;
            case Wind.Sha: return Vector2.left;
            case Wind.Pei: return Vector2.up;
            default:       return Vector2.zero;
        }
    }
}