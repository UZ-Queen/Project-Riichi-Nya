using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MahjongRoundInfo
{
    public int guk;
    public int bonzang;
    public int riichiBong;
    // public int oyaIndex;
    public Wind playerWind; // <-- 필요없는데...

    private MahjongRoundInfo(int guk, int bonzang, int riichiBong, Wind playerWind)
    {
        this.guk = guk;
        this.bonzang = bonzang;
        this.riichiBong = riichiBong;
        this.playerWind = playerWind;
    }

    public void AddRiichiBong(){
        riichiBong++;
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
        if (oyaTenpai)
        {
            // do nothing;
        }
        else
        {
            newinfo.guk++;
            newinfo.ChangeOya();
        }
        newinfo.bonzang++;
        return newinfo;
    }


    void ChangeOya()
    {
        playerWind = (Wind)(playerWind + 1 % 4);
    }
    public Wind currentJang
    {
        get
        {
            return (Wind)((guk - 1 / 4) % 4);
        }
    }

}

public struct MahjongRoundSimpleInfo{
    List<MahjongTile> doraTiles;
    public Wind roundWind;
    public int guk;
    public Wind playerWind;


    
}

public struct AdvMahjongRoundInfo{

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
