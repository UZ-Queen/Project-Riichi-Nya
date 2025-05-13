using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.XR;

public class MahjongPlayer
{
    public Wind Seat { get; private set; }
    // public int Score { get; private set; }
    public int score;


    public LinkedList<MahjongTile> River { get; private set; }
    public List<MahjongTile> Hand { get; private set; }
    public MahjongTile tsumoTile;

    public void Initialize()
    {
        River = new LinkedList<MahjongTile>();
        Hand = new List<MahjongTile>();
        tsumoTile = MahjongTile.NullTile();
    }

    public void SetPlayerHand(List<MahjongTile> newHand)
    {
        Hand = newHand;
    }

    public MahjongPlayer(Wind seat)
    {

        score = 25000;
        Initialize();
        Seat = seat;
    }
    public void ChangeSeat()
    {
        Seat = (Wind)(((int)Seat + 1)%4);
    }

    // public bool IsRiichiAble{
    //     get{


    //     }
    // }

    // public bool IsRiichiable()
    // {
    //     // List<int> removeToTenpaiTileIndex = new List<int>();
    //     Dictionary<int, List<MahjongWin>> removeToTenpaiTileIndex = new Dictionary<int, List<MahjongWin>>();
    //     bool isRiichiAble = false;
    //     List<MahjongTile> fullHand = new List<MahjongTile>(Hand);
    //     fullHand.Add(tsumoTile);

    //     for (int i = 0; i < fullHand.Count; i++)
    //     {
    //         List<MahjongTile> tenpaiCheckHand = new List<MahjongTile>(fullHand);
    //         tenpaiCheckHand.Remove(fullHand[i]);
    //         List<MahjongWin> wins = MahjongUtility.FindAgariTiles(tenpaiCheckHand);
    //         if (wins.Count > 0)
    //         {
    //             isRiichiAble = true;
    //             // removeToTenpaiTileIndex.Add(i);
    //             removeToTenpaiTileIndex.Add(i, wins);
    //         }
    //     }
    //     return isRiichiAble;
    // }

    public bool IsRiichiAble(out Dictionary<int, HashSet<MahjongWinInfo>> removeToTenpaiTileIndex)
    {
        removeToTenpaiTileIndex = new Dictionary<int, HashSet<MahjongWinInfo>>();
        if (tsumoTile == MahjongTile.NullTile())
        {
            // removeToTenpaiTileIndex = null;
            return false;
        }

        bool isRiichiAble = false;
        List<MahjongTile> fullHand = new List<MahjongTile>(Hand);
        fullHand.Add(tsumoTile);

        for (int i = 0; i < fullHand.Count; i++)
        {
            List<MahjongTile> tenpaiCheckHand = new List<MahjongTile>(fullHand);
            tenpaiCheckHand.Remove(fullHand[i]);
            HashSet<MahjongWinInfo> wins = MahjongUtility.FindAgariTiles(tenpaiCheckHand);
            if (wins.Count > 0)
            {
                isRiichiAble = true;
                // removeToTenpaiTileIndex.Add(i);
                removeToTenpaiTileIndex.Add(i, wins);
            }
        }
        return isRiichiAble;
    }
    public bool IsTsumoAble(MahjongTile targetTile, out HashSet<MahjongWinInfo> winInfos)
    {
        List<MahjongTile> handCopy = new List<MahjongTile>(Hand);
        handCopy.Add(targetTile);
        winInfos = MahjongUtility.CheckWinnableHashSet(handCopy, targetTile);
        return winInfos.Count > 0;
    }
    public bool IsTsumoAble(out HashSet<MahjongWinInfo> winInfos)
    {
        return IsTsumoAble(tsumoTile, out winInfos);
    }

    public bool IsTenpai(){
        var winInfo = MahjongUtility.FindAgariTiles(Hand);
        return winInfo.Count > 0;

    }

}

public class MahjongRound
{
    MahjongPlayer player;
    MahjongRoundInfo currentRoundInfo;
    // int seed { get; }

    public event Action OnHandUpdate = delegate { };
    public event Action<TsumoInfo> OnTsumoTile = delegate { };
    public event Action<MahjongRound> OnNewRoundStart = delegate{};
    public event Action<int> OnPlayerScoreAlters = delegate{};
    public event Action<MahjongWinInfo> OnPlayerWin = delegate{};
    public event Action OnPlayerDie = delegate{};



    public struct RoundEndInfo
    {
        public bool gameOver;
        public Wind nextWind;
    }

    // 여기부터 패산 관련

    LinkedList<MahjongTile> yama;
    List<MahjongTile> originalYama;
    LinkedList<MahjongTile> linshan;

    /// <summary>
    /// 숨겨진 무덤 패산. 깡친 뒤 패산에서 하나를 치울 때나, 1인 마작의 경우 136개의 패를 모두 쓰지 않기 때문에 때놓을 패를 여기다 보관하자.
    /// </summary>
    List<MahjongTile> graveyard;

    List<MahjongTile> doraTiles;
    List<MahjongTile> uradoraTiles;
    int kanCount = 0;

    System.Random prng;

    /// <summary>
    /// 패산을 생성하고, 왕패를 생성하고, 플레이어에게 손패를 나눠줍니다.
    /// 또한, 쯔모도 실행합니다.
    /// </summary>
    public void GenerateYama()
    {
        // System.Random prng = new System.Random(currentRoundInfo.seed);

        //초기화
        player.Initialize();

        linshan = new LinkedList<MahjongTile>();
        graveyard = new List<MahjongTile>();
        doraTiles = new List<MahjongTile>();
        uradoraTiles = new List<MahjongTile>();
        kanCount = 0;

        //패산 생성
        yama = new LinkedList<MahjongTile>
            (Utilities.ShuffleArray(MahjongTile.GetAllTiles().SelectMany(tile => Enumerable.Repeat(tile, 4)).ToArray(), prng.Next()));
        originalYama = new List<MahjongTile>(yama);

        //린샹패
        for (int i = 0; i < 4; i++)
        {
            linshan.AddLast(GetTileFromYamaLast());
        }
        //도라패
        for (int i = 0; i < 5; i++)
        {
            doraTiles.Add(GetTileFromYamaLast());
            uradoraTiles.Add(GetTileFromYamaLast());
        }

        //이후 플레이어에게 패 나눔. 이때 패는 fisrt부터 뽑아서 나눈다.
        for (int i = 0; i < 13; i++)
        {
            // newHand.Add(GetTileFromYamaFirst());
            player.Hand.Add(GetTileFromYamaFirst());
        }
        player.Hand.Sort();
        //손패를 고쳤다고 이벤트 실행
        OnHandUpdate();
        // 쯔모도 써먹어보거라~ 
        Tsumo();

    }

    /// <summary>
    /// 패를 쯔모합니다. 만약 패산이 비어있다면 이벤트를 발동하지 않고, 유국 처리하고 새로운 라운드를 시작합니다.
    /// </summary>
    public void Tsumo()
    {
        MahjongTile tsumoTile = GetTileFromYamaFirst();
        player.tsumoTile = tsumoTile;
        if(tsumoTile == MahjongTile.NullTile()){
            HandlePlayerYuguk();
            return;
        }
        TsumoInfo tsumoInfo = new TsumoInfo(player);
        OnTsumoTile(tsumoInfo);
    }
    /// <summary>
    /// 패를 버리고 강에 추가합니다. index 13이면 쯔모기리라는 거죠. 나중에 추적할 거면 여기서 추적하세요.
    /// </summary>
    /// <param name="index"></param>
    public void DiscardTile(int index)
    {

        index = Mathf.Clamp(index, 0, 13);
        if (index == 13)
        {
            player.River.AddLast(player.tsumoTile);

        }
        else
        {
            MahjongTile tile = MahjongUtility.SwapTiles(player.Hand, index, player.tsumoTile);
            player.River.AddLast(tile);

        }
        player.tsumoTile = MahjongTile.NullTile();
        player.Hand.Sort();
        // ??

        Tsumo();
    }

    MahjongTile GetTileFromYama(bool getFromFirst)
    {
        if (yama.Count == 0)
        {
            Debug.LogError("패산이 비어있습니다! 유국 처리할게요.");
            // HandlePlayerYuguk();
            return MahjongTile.NullTile();
        }
        MahjongTile newTile;
        if (getFromFirst)
        {
            newTile = yama.Last();
            yama.RemoveLast();
        }
        else
        {
            newTile = yama.First();
            yama.RemoveFirst();
        }

        if (yama.Count == 0)
        {
            // do Something;
        }
        return newTile;

    }
    /// <summary>
    /// 패산의 맨 앞에서 패를 가져옵니다. 비어있다면 NULL타일을 반환합니다.
    /// 패산의 맨 앞은 보통 쯔모할 때, 플레이어에게 패를 나눠줄 때 가져옵니다.
    /// </summary>
    /// <returns></returns>
    MahjongTile GetTileFromYamaFirst()
    {
        return GetTileFromYama(true);
    }
    MahjongTile GetTileFromYamaLast()
    {
        return GetTileFromYama(false);
    }

    Wind GetRandomWind(){
        if(prng == null){
            Debug.LogWarning("랜덤한 풍패를 가져오려 했는데.. 라운드의 prng가 널이에요! 뭔가 문제가 생긴 것 같습니다...");
            return (Wind)UnityEngine.Random.Range(0,4);
        }
        return (Wind)prng.Next(0,4);
    }

    // private MahjongRound(MahjongRoundInfo info, MahjongPlayer player)
    // {
    //     this.currentRoundInfo = info;
    //     this.player = player;
    //     // GenerateYama();
    // }

    // public static MahjongRound NewRound(int seed, MahjongPlayer player)
    // {
    //     Debug.LogWarning("아직 오야인덱스가 구분이 안 되어있어요!");
    //     MahjongRoundInfo newInfo = MahjongRoundInfo.NewRound(0, seed);
    //     return new MahjongRound(newInfo, player);
    // }
    
    /// <summary>
    /// 초기화용 생성자
    /// </summary>
    /// <param name="seed"></param>
    /// <param name="player"></param>
    private MahjongRound(int seed, out MahjongPlayer player){
        prng = new System.Random(seed);
        Wind playerWind = GetRandomWind();
        MahjongRoundInfo newInfo = MahjongRoundInfo.NewRound(playerWind);
        this.currentRoundInfo = newInfo;

        player = new MahjongPlayer(playerWind);
        this.player = player;
    }



    
    /// <summary>
    /// 새 라운드 생성용(맨 처음만 호출하세요!)
    /// 모든 라운드를 시작하려면 패산을 생성해야 합니다!!!
    /// </summary>
    /// <param name="seed"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public static MahjongRound NewRound(int seed, out MahjongPlayer player){
        return new MahjongRound(seed, out player);
    }

    /// <summary>
    /// 다음 라운드 생성용. 이전 라운드의 정보를 기반으로 다음 라운드를 생성해요!
    /// 위의 생성자와 다른 점은, 플레이어의 정보를 꼐속 유지한다는 점이죠!
    /// </summary>
    /// <param name="nextRoundInfo">이전 라운드에서 변경된 새로운 정보입니다.</param>
    /// <param name="player"></param>
    private MahjongRound(MahjongRoundInfo nextRoundInfo, MahjongPlayer player, int seed){
        prng = new System.Random(seed);
        this.currentRoundInfo = nextRoundInfo;
        this.player = player;
    }

    
    /// <summary>
    /// 라운드를 종료하고, 다음 라운드를 생성해 반환합니다. 
    /// </summary>
    /// <param name="hasWinner"></param>
    /// <param name="oyaTenpaiOrWon"></param>
    /// <returns></returns>
    MahjongRound NextRound(bool hasWinner, bool oyaTenpaiOrWon)
    {
        MahjongRoundInfo info;
        if (hasWinner)
        {
            info = currentRoundInfo.NextRoundOnWin(oyaTenpaiOrWon);
        }
        else
        {
            info = currentRoundInfo.NextRoundOnYuguk(oyaTenpaiOrWon);
        }

        if(hasWinner && !oyaTenpaiOrWon){
            player.ChangeSeat(); //<-- 사실 본장 따라서 자동으로 바뀌게 하고 싶은데.. 그만두자.
        }

        return new MahjongRound(info, player, prng.Next());
        // ...
    }



    /// <summary>
    /// 현재 손패와 쯔모패를 보고, 화료 가능한지 아닌지 판단합니다. 라운드를 종료합니다.
    /// 
    /// </summary>
    public void CheckTsumoWin()
    {
        HashSet<MahjongWinInfo> wininfo;
        if(!player.IsTsumoAble(out wininfo)) HandleDanbean();
        else HandlePlayerWin(MahjongUtility.GetHighestWinInfo(wininfo));
    }


    /// <summary>
    /// 새 라운드를 시작합니다.
    /// </summary>
    /// <param name="playerWon"></param>
    /// <param name="playerTenpai"></param>
    /// <returns></returns>
    void OnRoundEnds(bool playerWon, bool playerTenpai)
    {
        if(playerWon){
            ModifyPlayerToScore(currentRoundInfo.riichiBong * 1000);
            ModifyPlayerToScore(currentRoundInfo.bonzang * 300);
        }
        Debug.LogWarning("텐파이 시 점수를 주지 않습니다. 바꿀 꺼면 여기서 바꾸세요!");
        MahjongRound newRound = NextRound(playerWon, playerTenpai && player.Seat == Wind.Ton);

        OnNewRoundStart(newRound);

        // return newRound;

    }

    void HandlePlayerWin(MahjongWinInfo info)
    {
        int scoreToAdd;
        if(player.Seat == Wind.Ton){
            scoreToAdd =  info.scoreTable.oyaRon;
        }
        else{
            scoreToAdd = info.scoreTable.zaRon;
        }
        OnPlayerWin(info);
        ModifyPlayerToScore(scoreToAdd);
        OnRoundEnds(true, true);
    }
    void HandlePlayerYuguk(){
        bool isTenpai = player.IsTenpai(); 
        OnRoundEnds(playerWon:false, playerTenpai:isTenpai);
    }
    void HandleDanbean(){
        ModifyPlayerToScore(-8000);
        OnRoundEnds(false, false);
    }

    void ModifyPlayerToScore(int amount){
        player.score += amount;
        OnPlayerScoreAlters(amount);
    }


}


/// <summary>
/// 예아 예아(Showtime)
/// </summary>
public partial class MahjongUtility
{
    /// <summary>
    /// 13개의 손패가 들어 있는 리스트를 받아 HashSet<MahjongWinInfo>를 생성합니다.
    /// HashSet<MahjongWinInfo>: 가능한 화료패에 따른 역 종류/판수/부수의 정보를 가지고 있습니다.   
    /// </summary>
    /// <param name="hand"> 리스트는 13개의 손패여야 해요! </param>
    static public HashSet<MahjongWinInfo> FindAgariTiles(List<MahjongTile> hand)
    {
        HashSet<MahjongWinInfo> winInfos = new HashSet<MahjongWinInfo>();

        //화료패를 찾기 위해 모든 패를 넣어서 확인해본다.
        List<MahjongWin> availableWins = new List<MahjongWin>();
        if (hand.Count != 13) return winInfos;
        foreach (var agariTile in MahjongTile.GetAllTiles())
        {
            List<MahjongTile> copyHand = new List<MahjongTile>(hand);
            copyHand.Add(agariTile);
            availableWins.AddRange(CheckWinnable(copyHand, agariTile));
        }

        foreach (var i in availableWins)
        {
            winInfos.Add(new MahjongWinInfo(i));
        }
        return winInfos;

    }

    /// <summary>
    /// copyHand는 agariTile을 포함하고 있어야 해요! 즉 14개의 손패, 그리고 agariTile은 그중 뭐가 화료패인지를 명시해 주는 것 뿐이에요.
    /// </summary>
    /// <param name="copyHand">14개의 완전한 손패</param>
    /// <param name="agariTile">14개의 손패 중 화료패</param>
    /// <returns></returns>
    static public List<MahjongWin> CheckWinnable(List<MahjongTile> copyHand, MahjongTile agariTile)
    {
        List<MahjongWin> wins = new List<MahjongWin>();
        // 혹시라도 쯔모한 타일이 이상하다면..
        if (agariTile == MahjongTile.NullTile()) return wins;
        //5번째 패의 대기는 인정하지 않는다.
        if (copyHand.FindAll(x => x == agariTile).Count > 4) return wins;
            
        copyHand.Sort();
        
        //특수 승리 관리
        MahjongWin tempWin;
        //치또이일 경우
        if (IsChittoi(copyHand, agariTile, out tempWin))
        {
            wins.Add(tempWin);
        }
        //국싸무쌍의 경우
        else if (IsKokushiMusou(copyHand, agariTile, out tempWin))
        {
            wins.Add(tempWin);
            return wins;
        }

        //각 손패에 대해서 머리를 고정시키고 몸통을 찾는다.
        for (int k = 0; k < copyHand.Count - 1; k++)
        {
            if (copyHand[k] == copyHand[k + 1]) // 머리 발견 시 몸통이 있는지 검사한다.
            {
                List<MahjongWin> tempWins;
                if (Has4Body(copyHand, copyHand[k], agariTile, out tempWins))
                {
                    wins.AddRange(tempWins);
                    break;
                }
                k++;
            }
        }
        return wins;
    }

    static public HashSet<MahjongWinInfo> CheckWinnableHashSet(List<MahjongTile> copyHand, MahjongTile agariTile)
    {
        HashSet<MahjongWinInfo> info = new HashSet<MahjongWinInfo>();
        foreach (var i in CheckWinnable(copyHand, agariTile))
        {
            info.Add(new MahjongWinInfo(i));
        }
        return info;
    }

    /// <summary>
    /// 같은 화료패가 있다면 가장 높은 부수와 판수를 가진 승리 정보만 남깁니다.
    /// </summary>
    /// <param name="info"></param>
    static public void RemoveDoubleInfoPerTile(HashSet<MahjongWinInfo> info)
    {
        HashSet<MahjongWinInfo> infoToRemove = new HashSet<MahjongWinInfo>();
        foreach (var i in info)
        {
            if (info.Any(win => win.winTile == i.winTile && i < win))
            {
                infoToRemove.Add(i);
            }
        }
        foreach (var i in infoToRemove)
        {
            info.Remove(i);
        }
    }

    /// <summary>
    /// 가장 높은 가치를 가진 승리 정보 하나를 가져옵니다.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    static public MahjongWinInfo GetHighestWinInfo(HashSet<MahjongWinInfo> info)
    {
        return info.Max();
    }
    static bool HasStraight(List<MahjongTile> hand, out List<MahjongTile> body)
    {
        if (hand.Count < 3)
        {
            body = null;
            return false;
        }
        List<MahjongTile> newBody = new List<MahjongTile>();
        newBody.Add(hand[0]);
        int index;


        for (int i = 0; i < 2; i++)
        {
            index = hand.FindIndex(x => x.TileID == newBody[i].NextConnectedTileID);
            if (index == -1)
            {
                body = null;
                return false;
            }
            newBody.Add(hand[index]);
        }
        body = newBody;
        return true;

    }
    static bool HasTriple(List<MahjongTile> hand, out List<MahjongTile> body)
    {

        if (hand.Count < 3)
        {
            body = null;
            return false;
        }

        List<MahjongTile> newBody = new List<MahjongTile>();
        newBody.Add(hand[0]);

        if (hand[0] == hand[1] && hand[1] == hand[2])
        {
            newBody.Add(hand[1]);
            newBody.Add(hand[2]);
            body = newBody;
            return true;
        }
        else
        {
            body = null;
            return false;
        }

    }

    static bool Has4Body(List<MahjongTile> hand, MahjongTile head, MahjongTile agariTile, out List<MahjongWin> wins)
    {
        wins = new List<MahjongWin>();

        if (hand.Count != 14)
        {
            Logger.LogWarning("Has4Body: 손 개수가 14개가 아닙니다!");
            wins = null;
            return false;
        }
        bool hasBodyStraightFisrt = true;
        bool hasBodyTripleFisrt = true;
        List<MahjongTile> listHand = new List<MahjongTile>(hand);

        //머리 블록 생성

        List<MahjongTile> headList = new List<MahjongTile>();
        int headIndex = listHand.FindIndex(x => x == head);
        headList.Add(listHand[headIndex]);
        listHand.Remove(head);
        headList.Add(listHand[headIndex]);
        listHand.Remove(head);
        MahjongBlock headBlock = new MahjongBlock(headList); // 좀 더러운데


        List<MahjongBlock> bodyBlocks = new List<MahjongBlock>();
        //MahjongBlock blockContainingWinningTile;



        // 슌쯔를 찾는다. 만약 슌쯔가 없다면 커쯔를 찾는다. 
        // 몸통을 찾았다면 
        while (listHand.Count > 0)
        {
            List<MahjongTile> body;
            if (HasStraight(listHand, out body))
            {
                foreach (var i in body)
                {
                    listHand.Remove(i);
                }
                bodyBlocks.Add(new MahjongBlock(body));

            }
            else if (HasTriple(listHand, out body))
            {
                foreach (var i in body)
                {
                    listHand.Remove(i);
                }
                bodyBlocks.Add(new MahjongBlock(body));
            }
            else
            {
                hasBodyStraightFisrt = false;
                bodyBlocks = null;
                break;
            }
        }

        if (bodyBlocks != null)
        {
            wins.Add(new MahjongWin(headBlock, bodyBlocks, agariTile, true));
        }


        listHand = new List<MahjongTile>(hand);
        listHand.Remove(head);
        listHand.Remove(head);
        bodyBlocks = new List<MahjongBlock>();


        while (listHand.Count > 0)
        {
            List<MahjongTile> body;
            if (HasTriple(listHand, out body))
            {
                foreach (var i in body)
                {
                    listHand.Remove(i);
                }
                bodyBlocks.Add(new MahjongBlock(body));
            }
            else if (HasStraight(listHand, out body))
            {
                foreach (var i in body)
                {
                    listHand.Remove(i);
                }
                bodyBlocks.Add(new MahjongBlock(body));
            }
            else
            {
                hasBodyTripleFisrt = false;
                bodyBlocks = null;
                break;
            }
        }
        if (bodyBlocks != null)
        {
            wins.Add(new MahjongWin(headBlock, bodyBlocks, agariTile, true));
        }


        return hasBodyStraightFisrt || hasBodyTripleFisrt;
    }
    static bool IsChittoi(List<MahjongTile> hand, MahjongTile agariTile, out MahjongWin win)
    {
        Stack<MahjongTile> stack = new Stack<MahjongTile>(hand);
        List<MahjongBlock> kyaruberos = new List<MahjongBlock>();

        MahjongTile lastTile = MahjongTile.NullTile();
        bool isChittoi = true;


        while (stack.Count > 0)
        {
            MahjongTile currentTile = stack.Pop();

            if (stack.Peek() != currentTile)
            {
                isChittoi = false;
                break;
            }
            if (lastTile == currentTile) // 1 1 2 2 3 3 3 3 의 경우 치또이가 아니다.
            {
                isChittoi = false;
                break;
            }

            kyaruberos.Add(MahjongBlock.NewHead(currentTile, stack.Pop()));
            lastTile = currentTile;
        }

        if (isChittoi)
        {
            win = new MahjongWin(kyaruberos, agariTile, true);

        }
        else
        {
            win = null;
        }
        return isChittoi;

    }
    static bool IsKokushiMusou(List<MahjongTile> hand, MahjongTile agariTile, out MahjongWin win)
    {
        if (!(agariTile.IsYoguPae))
        {
            win = null;
            return false;
        }




        List<MahjongTile> handCopy = new List<MahjongTile>(hand);
        handCopy.Sort();
        foreach (var tile in handCopy)
        {
            if (handCopy.Count(i => i == tile) == 2)
            {
                handCopy.Remove(tile);
                break;
            }
        }
        Logger.Log("***** 니 손패 *****");
        foreach (var i in handCopy) Logger.Log(i.ToString());

        List<MahjongTile> orphans = MahjongTile.StringToTiles("1m9m1p9p1s9s1z2z3z4z5z6z7z");


        Logger.Log("***** 국싸무쌍 손패 *****");
        foreach (var i in orphans) Logger.Log(i.ToString());
        if (!handCopy.SequenceEqual(orphans))
        {
            win = null;
            return false;
        }

        win = new MahjongWin(hand, agariTile, true);
        return true;

    }

}
public partial class MahjongUtility
{
    public static Wind NextWind(Wind wind)
    {
        if (wind != Wind.Pei)
        {
            return wind + 1;
        }
        else
        {
            return Wind.Ton;
        }
    }
    public static Wind NextWind(int wind)
    {
        return NextWind((Wind)(wind % 4));
    }

    public static MahjongTile SwapTiles(List<MahjongTile> hand, int index, MahjongTile newTile)
    {
        MahjongTile tmp = hand[index];
        hand[index] = newTile;
        return tmp;
    }

    public static int GetBaseScoreByHanAndFu(int han, int fu)
    {
        bool isNyanganOrHigher = false;
        if (han == 3 && fu > 60) isNyanganOrHigher = true;
        if (han == 4 && fu > 30) isNyanganOrHigher = true;
        if (han > 4) isNyanganOrHigher = true;

        int baseScore = 0;
        if (isNyanganOrHigher)
        {
            if (han < 6) baseScore = 2000;
            else if (han < 8) baseScore = 3000;
            else if (han < 11) baseScore = 4000;
            else if (han < 13) baseScore = 6000;
            else baseScore = 8000;
        }
        else
        {
            baseScore = fu * (int)System.Math.Pow(2, han + 2);
        }

        return baseScore;

    }

    public static int GetBaseScoreByYakuman(int yakumanCount)
    {
        return 8000 * yakumanCount; // 6
    }

    public static int GetHan(SortedSet<Yaku> yakues)
    {
        int count = 0;
        count = yakues.Sum(yaku => YakuInfo.YakuData[yaku].Han);

        return count;
    }
    public static void RemoveLowerYakues(SortedSet<Yaku> yakues)
    {
        SortedSet<Yaku> yakuToRemove = new SortedSet<Yaku>();
        foreach (var i in yakues)
        {
            yakuToRemove.UnionWith((YakuInfo.YakuData[i].lowerYakues));
        }
        foreach (var i in yakuToRemove)
        {
            yakues.Remove(i);
        }
    }

    //또한 핑후도 판단해준다.
    public static int GetFu(MahjongWin win, ref bool isPinfu)
    {
        if (win.winType != MahjongWin.WinType.Normal) return 25;


        int baseFu = 20;
        int maxBonusBusu = 0;
        MahjongBlock maxBlock;
        //MahjongBlock pinfuBlock;
        //bool isPinfu = false;
        foreach (var block in win.availableWaitingBlocks)
        {
            //pinfuAvailablity = false;
            //bool isPinfuThisturn = false;
            int currentBusu = 0;
            currentBusu += MachiBusu(block, win.waitingTile);
            currentBusu += HeadBusu(win.heads[0]);
            currentBusu += BodyBusu(win, block);

            if (currentBusu == 0 && win.IsHandConcealed)
            {
                isPinfu = true;
                maxBonusBusu = WinBusu(win, isPinfu);
                //isPinfuThisturn = true;
                //pinfuBlock = block;
                break; //핑후의 부수는 마지막에 고정시킨다.
            }

            currentBusu += WinBusu(win, isPinfu);

            //어짜피 무조건 쯔모라고 생각하자. 론은 나중에 생각할 것
            ////비멘젠 론은 최소 30부 보장한다.
            //if(currentBusu == 0 && !win.isWaitingTileTsumo)
            //{
            //    currentBusu = 1;
            //}


            if (maxBonusBusu < currentBusu)
            {
                maxBlock = block;
                maxBonusBusu = currentBusu;
            }

        }


        baseFu += ((maxBonusBusu + 9) / 10) * 10; //10의 자리에서 올림


        return baseFu;

    }


    public static bool IsHandOpened(MahjongWin win)
    {
        return win.bodies.Any((body) => body.openType == MahjongBlock.OpenType.Opened);
    }
}

// 외부에서 호출하지 않는 메서드
public partial class MahjongUtility
{
    static int MachiBusu(MahjongBlock block, MahjongTile waitingTile)
    {
        //머리의 단기대기
        if (block.blockType == MahjongBlock.BlockType.Head)
        {
            return 2;
        }
        //여기서부터 모두 몸통의 대기

        //샤보대기
        if (block.bodyType == MahjongBlock.BodyType.Triplet)
        {
            return 0;
        }

        if (block.bodyType == MahjongBlock.BodyType.Sequence)
        {
            //간짱
            if (block[1] == waitingTile)
            {
                return 2;
            }
            // 변짱 (1,2일때 3 / 8,9일 때 7)
            if (block[2] == waitingTile && waitingTile.number == 3 || block[0] == waitingTile && waitingTile.number == 7)
            {
                return 2;
            }
            //양면(귀찮다)
            return 0;

        }

        Console.WriteLine("뭔가 잘못됨");
        return 0;

        //변짱, 간짱, 단기
    }

    static int HeadBusu(MahjongBlock head)
    {
        if (!head.ContainsZapae)
            return 0;
        else
        {
            Logger.LogWarning("현재 풍패를 알 수 없기 떄문에 모든 풍패는 2부를 추가로 얻습니다. 참고하세요");
            return 2;
        }
    }

    static int BodyBusu(MahjongWin win, MahjongBlock waitingBlock)
    {
        int busu = 0;
        foreach (MahjongBlock block in win.bodies)
        {
            if (block.bodyType == MahjongBlock.BodyType.Sequence) continue;

            int baseBusu = 2; // 기본 밍커 부수 = 2
            if (block[0].IsYoguPae) baseBusu *= 2; // 요구패면 2배 
            if (block.openType == MahjongBlock.OpenType.Concealed) baseBusu *= 2; // 안커 또는 안깡이라면 2배
            if (block.bodyType == MahjongBlock.BodyType.Quad) baseBusu *= 4; // 깡이라면 4배
                                                                             //최댓값 = 2*2*2*4 = 32부
            busu += baseBusu;
        }
        return busu;

    }

    static void WinBusu(MahjongWin win, ref int currentBusu)
    {
        //화료에 따른 부수 추가

        if (win.isWaitingTileTsumo)
        {
            //쯔모 시 2부 추가, 단 핑후라면 20부 고정
            currentBusu += 2;
        }
        else if (win.IsHandConcealed)
        {
            //멘젠 론은 10부 추가
            currentBusu += 10;
        }
        //비멘젠 론이라면 30부 보장.
    }
    static int WinBusu(MahjongWin win, bool isPinfu = false)
    {
        //화료에 따른 부수 추가
        //int busu = 0;
        if (win.isWaitingTileTsumo)
        {
            //쯔모 시 2부 추가, 단 핑후라면 20부 고정
            if (isPinfu) return 0;
            else return 2;
        }
        else if (win.IsHandConcealed)
        {
            //멘젠 론은 10부 추가
            return 10;
        }
        return 0;
        //비멘젠 론이라면 30부 보장.
    }

}

