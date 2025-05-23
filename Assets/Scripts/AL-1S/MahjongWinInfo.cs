using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;

public class MahjongHand
{
    List<MahjongTile> hand;
    List<MahjongBlock> furoBlocks;
}

public class MahjongWin : MahjongHand
{
    public enum WinType { Normal, Chittoi, Kokushimusou };
    public WinType winType = WinType.Normal;


    public List<MahjongBlock> bodies = new List<MahjongBlock>();
    public List<MahjongBlock> heads = new List<MahjongBlock>();
    public List<MahjongTile> tiles = new List<MahjongTile>();
    //public 

    public MahjongTile waitingTile = MahjongTile.NullTile();
    public List<MahjongBlock> availableWaitingBlocks = new List<MahjongBlock>();
    public bool isWaitingTileTsumo;

    // public Wind roundWind = Wind.MOLLU;
    // public Wind playerWind = Wind.MOLLU;
    public WindInfo windInfo = WindInfo.NullInfo();
    public bool IsHandConcealed
    {
        get
        {
            return bodies.All(body => body.openType == MahjongBlock.OpenType.Concealed);
        }
    }

    //뭐여 이게
    void GenerateAvailableWaitingBlocks()
    {
        availableWaitingBlocks = new List<MahjongBlock>(bodies.FindAll(block => block.Contains(waitingTile)));
    }

    public List<MahjongBlock> GetAllBlocks()
    {
        List<MahjongBlock> hands = new List<MahjongBlock>(bodies);
        hands.AddRange(heads);
        return hands;
    }
    public List<MahjongTile> GetAllTiles()
    {
        List<MahjongTile> allTiles = new List<MahjongTile>();
        foreach (var block in GetAllBlocks())
        {
            foreach (var tile in block.block)
            {
                allTiles.Add(tile);
            }
        }
        return allTiles;
    }
    /// <summary>
    /// 꼭 Wininfo를 생성하기 전에 업데이트 해 주세요.
    /// </summary>
    /// <param name="roundWind"></param>
    /// <param name="playerWind"></param>
    // public void UpdateRoundWindInfo(Wind roundWind, Wind playerWind)
    // {
    //     this.roundWind = roundWind;
    //     this.playerWind = playerWind;
    // }
    public void UpdateRoundWindInfo(WindInfo info)
    {
        // this.roundWind = info.roundWind;
        // this.playerWind = info.playerWind;
        MyLogger.LogWarning($"MahjongWIN -> 플레이어 {info.playerWind} / 라운드 {info.roundWind}");
        windInfo = info;
        MyLogger.LogWarning($"MahjongWIN -> 플레이어 {windInfo.playerWind} / 라운드 {windInfo.roundWind}");
    }
    //일반 화료
    public MahjongWin(MahjongBlock head, List<MahjongBlock> bodies, MahjongTile agariTile, bool isTsumo)
    {
        this.heads = new List<MahjongBlock>();
        heads.Add(head);

        this.bodies = new List<MahjongBlock>(bodies);
        this.waitingTile = agariTile;

        // this.roundWind = roundWind;
        // this.playerWind = playerWind;

        GenerateAvailableWaitingBlocks();
        isWaitingTileTsumo = isTsumo;
        //UpdateInfo();
    }

    //치또이 전용
    public MahjongWin(List<MahjongBlock> heads, MahjongTile agariTile, bool isTsumo)
    {
        this.heads = new List<MahjongBlock>(heads);

        this.bodies = new List<MahjongBlock>();
        this.waitingTile = agariTile;
        GenerateAvailableWaitingBlocks();
        isWaitingTileTsumo = isTsumo;
        winType = WinType.Chittoi;
        //UpdateInfo();
    }

    //isKokushimusou
    public MahjongWin(List<MahjongTile> tiles, MahjongTile agariTile, bool isTsumo)
    {

        tiles = new List<MahjongTile>(tiles);
        this.waitingTile = agariTile;
        GenerateAvailableWaitingBlocks();
        isWaitingTileTsumo = isTsumo;
        winType = WinType.Kokushimusou;
    }

}

//손패의 정보. 노두패 블럭의 개수.. 리치 여부.. 
public struct MahjongHandInfo
{
    public bool isHandMenzen;
    public bool isWinTileTsumo;
    public MahjongTile winTile;

    public bool isRiichi;
    public bool isDoubleRiichi;
    public bool isItpatsu;



    //슌쯔 관련 역

    public bool isPinfu;
    public int bodyContains19;
    public int headContains19;
    public int bodyContainsZapae;
    public int headContainsZapae;

    public int ipekoCount; //2면 량페코
    public bool isSamsaek;
    public bool isIttsu; //<- 이게 아닌데 걍 하자 ㅇㅇ 나중에 바꿔



    //커쯔 관련 역
    public bool isSansokuToukou;
    public int kanCount;
    public int tripleCount;
    public int closedTripleCount;




    public bool doBodyContainsTon;
    public bool doBodyContainsNan;
    public bool doBodyContainsSha;
    public bool doBodyContainsPei;
    public bool doHeadContainsWindTile;

    // public Wind seatWind;
    // public Wind roundWind;
    public WindInfo windInfo;
    // public bool isSeatWind;
    // public bool isRoundWind;

    public bool doBodyContainsHaku;
    public bool doBodyContainsHatsu;
    public bool doBodyContainsChun;

    public bool doHeadContainsDragonTile;



    // 기타 홍대병 역
    public bool isChittoi;
    public bool isHonisou; // 한 종류의 수패만 있는가?
    public int bodyContainsOnly19;




    //부가역
    public bool isChanKan;
    public bool isRinshan;
    public bool isHaitei;
    public bool isHoutei;

    public bool isTenhou;
    public bool isChihou;

    public bool isRyuuisou;
    public bool isKokushimushou;
    public bool isChuuren;



    public int doraCount;
    public int uradoraCount;
    public int akadoraCount;
    public int nukidoraCount;

    [Obsolete("아직 미구현된 역들이 많아요! 여기서 조건을 수정해주세요.")]
    public MahjongHandInfo(MahjongWin winHand)
    {
        //throw new NotImplementedException();
        isHandMenzen = winHand.IsHandConcealed;
        isWinTileTsumo = winHand.isWaitingTileTsumo;
        winTile = winHand.waitingTile;

        isRiichi = false;
        isDoubleRiichi = false;
        isItpatsu = false;

        isPinfu = false; //기본적으로 false이되 GetFu에서 핑후를 판단해서 바꿀 것이다.
        bodyContains19 = winHand.bodies.Count(block => block.Contains19);
        headContains19 = winHand.heads.Count(block => block.Contains19);
        bodyContainsZapae = winHand.bodies.Count(block => block.ContainsZapae);
        headContainsZapae = winHand.heads.Count(block => block.ContainsZapae);


        ipekoCount = MahjongYakuSolver.IpekoCounter(winHand);
        MahjongYakuSolver.IsSamSaek(winHand, out isSamsaek, out isSansokuToukou);
        isIttsu = MahjongYakuSolver.IsIttsu(winHand);

        kanCount = winHand.bodies.Count(block => block.bodyType == MahjongBlock.BodyType.Quad);
        tripleCount = winHand.bodies.Count(block => block.bodyType == MahjongBlock.BodyType.Triplet || block.bodyType == MahjongBlock.BodyType.Quad);
        closedTripleCount = tripleCount;

        doBodyContainsTon = winHand.bodies.Count(block => block.tileType == MahjongTile.TileType.Ton
                            && block.blockType == MahjongBlock.BlockType.Body) == 1;
        doBodyContainsNan = winHand.bodies.Count(block => block.tileType == MahjongTile.TileType.Nan
                            && block.blockType == MahjongBlock.BlockType.Body) == 1;
        doBodyContainsSha = winHand.bodies.Count(block => block.tileType == MahjongTile.TileType.Sha
                            && block.blockType == MahjongBlock.BlockType.Body) == 1;
        doBodyContainsPei = winHand.bodies.Count(block => block.tileType == MahjongTile.TileType.Pei
                            && block.blockType == MahjongBlock.BlockType.Body) == 1;

        doHeadContainsWindTile = winHand.heads.Count(block =>
                            block.tileType >= MahjongTile.TileType.Ton
                            && block.tileType <= MahjongTile.TileType.Pei
                            && block.blockType == MahjongBlock.BlockType.Head) == 1;

        // seatWind = winHand.playerWind;
        // roundWind = winHand.roundWind;
        windInfo = winHand.windInfo;
        MyLogger.LogWarning($"플레이어 {windInfo.playerWind} / 라운드 {windInfo.roundWind}");
        // isSeatWind = true;
        // isRoundWind = true;

        doBodyContainsHaku = winHand.bodies.Count(block => block.tileType == MahjongTile.TileType.Haku
                            && block.blockType == MahjongBlock.BlockType.Body) == 1;
        doBodyContainsHatsu = winHand.bodies.Count(block => block.tileType == MahjongTile.TileType.Hatsu
                            && block.blockType == MahjongBlock.BlockType.Body) == 1;
        doBodyContainsChun = winHand.bodies.Count(block => block.tileType == MahjongTile.TileType.Chun
                            && block.blockType == MahjongBlock.BlockType.Body) == 1;

        doHeadContainsDragonTile = winHand.heads.Count(block =>
                block.tileType >= MahjongTile.TileType.Haku
                && block.tileType <= MahjongTile.TileType.Chun
                && block.blockType == MahjongBlock.BlockType.Head) == 1;

        isChittoi = winHand.heads.Count == 7;
        isHonisou = MahjongYakuSolver.IsHonisou(winHand);
        bodyContainsOnly19 = winHand.bodies.Count(block => block.Contains19 && block.bodyType != MahjongBlock.BodyType.Sequence);



        isChanKan = false;
        isRinshan = false;
        isHaitei = false;
        isHoutei = false;
        isTenhou = false;
        isChihou = false;

        isRyuuisou = false;
        isKokushimushou = winHand.winType == MahjongWin.WinType.Kokushimusou;
        isChuuren = MahjongYakuSolver.IsChuuren(winHand);

        doraCount = winHand.GetAllTiles().Sum(tile => tile.doraCount);
        uradoraCount = 0;

        akadoraCount = winHand.GetAllTiles().Count(tile => tile.isAkaDora);
        nukidoraCount = 0;

        // ... 
    }

}


/// <summary>
/// MahjongHandInfo는 Winhand를 받아서 역, 판수, 부수를 계산하고 저장한다.
/// </summary>
public class MahjongWinInfo : IComparable<MahjongWinInfo>

{
    public UniqueName GetUniqueName
    {
        get
        {
            int score = scoreTable.baseScore;
            // if(score < 2000)

            switch (score)
            {
                case 2000:
                    return UniqueName.Nyangan;
                case 3000:
                    return UniqueName.Henenyan;
                case 4000:
                    return UniqueName.Bainyan;
                case 6000:
                    return UniqueName.Sanbainyan;
                case 8000:
                    return UniqueName.Yakunyan;
                case 16000:
                    return UniqueName.DoubleYakunyan;
                case 24000:
                    return UniqueName.TripleYakunyan;
                case 32000:
                    return UniqueName.YonbaiYakunyan;
                case 40000:
                    return UniqueName.GobaiYakunyan;
                case 48000:
                    return UniqueName.RyokubaiYakunyan;
                default:
                    return UniqueName.Jjuna;
            }
        }
    }


    public struct ScoreTable
    {
        public int baseScore;
        public int oyaRon;
        public int oyaTsumo;
        public int zaRon;
        public int zaTsumoToOya;
        public int zaTsumoToZa;

        public ScoreTable(int baseScore)
        {
            this.baseScore = baseScore;
            oyaRon = RoundUp(baseScore * 6);
            oyaTsumo = RoundUp(baseScore * 2);
            zaRon = RoundUp(baseScore * 4);
            zaTsumoToOya = RoundUp(baseScore * 2);
            zaTsumoToZa = RoundUp(baseScore * 1);
        }

        static int RoundUp(int score)
        {
            return (score + 99) / 100 * 100;
        }
    }

    public WindInfo windInfo = WindInfo.NullInfo();

    public ScoreTable scoreTable { get; private set; }



    public SortedSet<Yaku> yakues;
    public int Han
    {
        get;
    }
    public int Fu
    {
        get;
    }


    public DoraInfo doraInfo;

    // public int doraCount;
    // public int akadoraCount;
    // public int uradoraCount;

    public MahjongTile winTile;
    public MahjongWinInfo(MahjongWin winHand)
    {
        MahjongHandInfo info = new MahjongHandInfo(winHand);
        this.windInfo = winHand.windInfo;
        winTile = info.winTile;

        yakues = new SortedSet<Yaku>();

        Fu = MahjongUtility.GetFu(winHand, windInfo, ref info.isPinfu);
        MahjongYakuSolver.Get1HanYakues(info, yakues);
        MahjongYakuSolver.Get2HanYakues(info, yakues);
        MahjongYakuSolver.Get3orHigherHanYakues(info, yakues);
        MahjongYakuSolver.GetYakumanYakues(info, yakues);
        MahjongYakuSolver.GetDora(info, yakues);
        MahjongUtility.RemoveLowerYakues(yakues);
        MyLogger.LogWarning("아직 우라도라는 포함 안되었습니다. 여기서 추가하세요");
        doraInfo = new DoraInfo(info.doraCount, info.akadoraCount, 0);
        Han = MahjongUtility.GetHan(yakues, doraInfo);
        
        int baseScore = MahjongUtility.GetBaseScoreByHanAndFu(Han, Fu);
        scoreTable = new ScoreTable(baseScore);

    }

    public void UpdateDora()
    {
        
    }

    void UpdateYakuInfo()
    {
        


    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"{winTile.ToString()} 쯔모 시: \n");
        sb.Append("***역 리스트***\n");
        foreach (var i in yakues)
        {
            int count = YakuInfo.YakuData[i].Han;
            if (count < 12) sb.Append($"[{count}판] ");
            else sb.Append($"[역만] ");

            sb.Append($"{i.ToString()}\n");
        }
        sb.Append($"{Han}판 {Fu}부 {scoreTable.zaTsumoToOya}, {scoreTable.zaTsumoToZa}점\n");

        return sb.ToString();
    }

    public static bool operator ==(MahjongWinInfo himari, MahjongWinInfo rio)
    {
        if (ReferenceEquals(himari, rio)) return true;
        if (himari is null || rio is null) return false;
        if (himari.Han != rio.Han) return false;
        if (himari.Fu != rio.Fu) return false;
        if (!himari.yakues.SetEquals(rio.yakues)) return false;
        return true;
    }
    public static bool operator !=(MahjongWinInfo himari, MahjongWinInfo rio)
    {
        return !(himari == rio);
    }

    public static bool operator <(MahjongWinInfo himari, MahjongWinInfo rio)
    {

        if (himari.Han < rio.Han) return true;
        if (himari.Han > rio.Han) return false;

        if (himari.Fu < rio.Fu) return true;
        // if(himari.Fu >= rio.Fu) return false;

        return false;

    }
    public static bool operator >(MahjongWinInfo himari, MahjongWinInfo rio)
    {
        if (himari == rio) return false;
        return !(himari < rio);
    }

    public override int GetHashCode()
    {
        int hash = Utilities.HashCombine(Han, Fu, winTile);
        foreach (var yaku in yakues)
        {
            hash = Utilities.HashCombine(hash, yaku);
        }
        return hash;
    }
    public override bool Equals(object obj)
    {
        if (obj is MahjongWinInfo other)
        {
            return other == this;
        }
        return false;
    }

    public int CompareTo(MahjongWinInfo other)
    {
        if (this == other) return 0;
        return this > other ? 1 : -1;
    }
}