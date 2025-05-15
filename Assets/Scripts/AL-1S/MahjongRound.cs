using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class MahjongPlayer
{
    public Wind Seat { get; private set; }
    public bool IsOya{get{return Seat == Wind.Ton;}}
    // public int Score { get; private set; }
    public int Score{get;private set;}


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

        Score = 25000;
        Initialize();
        Seat = seat;
    }
    public void ChangeSeat()
    {
        Seat = (Wind)(((int)Seat + 1)%4);
    }

    public void AlterScore(int delta){
        Score += delta;
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
    
    /// <summary>
    /// 도라패가 바뀌었거나, 늘어났거나 할 때 실행하는 이벤트인데, 필요할려나..? 
    /// </summary>
    public event Action<MahjongRoundInfo> OnRoundInfoUpdate = delegate{};
    public event Action<int> OnPlayerScoreAlters = delegate{};
    public event Action<MahjongWinInfo> OnPlayerWin = delegate{};
    public event Action OnPlayerDie = delegate{};

/// <summary>
/// 패산이 많이 남아 있어도 이 이상 넘어가면 유국 처리한다.
/// </summary>
    public int remainingTsumoCount;

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
    int doraIndex = 0;

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

        currentRoundInfo.AddNewDoraTiles(doraTiles[0]);

        //이후 플레이어에게 패 나눔. 이때 패는 fisrt부터 뽑아서 나눈다.
        for (int i = 0; i < 13; i++)
        {
            // newHand.Add(GetTileFromYamaFirst());
            player.Hand.Add(GetTileFromYamaFirst());
        }
        player.Hand.Sort();

        UpdateDora(0);
        OnRoundInfoUpdate(currentRoundInfo);
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

    void UpdateDora(int doraIndex = 0){
        
        UpdateDoraFromList(yama, doraTiles[doraIndex]);
        UpdateDoraFromList(linshan, doraTiles[doraIndex]);
        UpdateDoraFromList(player.River, doraTiles[doraIndex]);
        for(int i=0; i<player.Hand.Count; i++){
            if(doraTiles[doraIndex].DoraTileID == player.Hand[i].TileID){
                var tmpTile = player.Hand[i];
                tmpTile.isDora = true;
                player.Hand[i] = tmpTile;
            }
        }

    }

    //도라표시패가 3만이라면, 도라를 켜야 하는 타일은 4만. 즉 
    void UpdateDoraFromList(LinkedList<MahjongTile> ll,  MahjongTile target){
        var node = ll.First;
        while(node != null){
            if(target.DoraTileID == node.Value.TileID){
                var tmpTile = node.Value;
                tmpTile.isDora = true;
                node.Value = tmpTile;
            }
            node = node.Next;
        }
    }
    


    MahjongTile GetTileFromYama(bool getFromFirst)
    {
        if (yama.Count == 0)
        {
            MyLogger.LogError("패산이 비어있습니다! 유국 처리할게요.");
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
            MyLogger.LogWarning("랜덤한 풍패를 가져오려 했는데.. 라운드의 prng가 널이에요! 뭔가 문제가 생긴 것 같습니다...");
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
    //     MyLogger.LogWarning("아직 오야인덱스가 구분이 안 되어있어요!");
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
        MyLogger.LogWarning("텐파이 시 점수를 주지 않습니다. 바꿀 꺼면 여기서 바꾸세요!");
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
        player.AlterScore( amount);
        OnPlayerScoreAlters(amount);
    }


}

