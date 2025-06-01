#define IROHA
#undef IROHA
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;


public partial class MahjongGameManager : MonoBehaviour, IScoreDistanceConsumer
{
    public static MahjongGameManager Instance { get; private set; }

    public event Action<GameState> OnStateChange = delegate { };
    public event Action OnGameOver = delegate { };
    public event Action OnGameStart = delegate { };

    [Header("엑스트라 유틸리티 2")]
    [SerializeField] private ScoreManagerDistance scoreManagerDistance;
    [SerializeField] private Timer redstoneClock;
    [Header("시구레 UI")]
    [SerializeField] private PlayerHand playerHand;
    [SerializeField] private UiScoreDistanceInfo uiScoreDistanceInfo;
    [SerializeField] private UiScoreInfo uiScoreInfo;
    [SerializeField] private UiRoundInfo uiRoundInfo;
    [SerializeField] private UiCallInfo uiCallHolder;

    [SerializeField] private UiWinInfo uiWininfo;
    [SerializeField] private UiRemainingTimeIndicator uiRemainingTime;
    [SerializeField] private UiGameOver uiGameOver;

    [Header("몰름보")]
    public GameState currentState = GameState.Initializing;
    public MahjongTileDatabase TileDB;
    System.Random prng;
    MahjongRound currentRound;
    MahjongPlayer player;
    int seed = 1557;
    // bool _isGameOver = false;

    public void StartNewGame()
    {
        OnGameStart();
        currentState = GameState.Initializing;
        GameUIManager.Instance.Initialize();
        
        prng = new System.Random();
#if IROHA
        prng = new System.Random(seed);
#endif
        Profiler.BeginSample("마작라운드 생성");
        currentRound = MahjongRound.NewRound(prng.Next(), out player);
        //라운드 생성 후 꼭 패산을 수동으로 생성해야 라운드가 시작한다.
        AttachRoundEvent();
        currentRound.GenerateYama();
        UpdatePlayerScore(0);

        #if IROHA
        player.ManipulateHand("1z1z1z2z2z2z3z3z3z4z4z4z2p");
        #endif
        UpdatePlayerHand();
        Profiler.EndSample();

        //스코어매니저 생성
        Construct(scoreManagerDistance);
        svcScoreManager.Initialize();
        //UI에 뿌려줌
        uiScoreDistanceInfo.Construct(svcScoreManager);
        //타이머 생성 후..
        redstoneClock.StartTimer(180);
        redstoneClock.OnTimerFinished += HandleGameOver;
        uiRemainingTime?.Construct(redstoneClock);
        
        currentState = GameState.PlayerTurn;
        // currentRound = new MahjongRound(prng.Next(), player);
    }

    void StartNextRound(MahjongRound nextRound){
        currentState = GameState.Processing;
        DetachRoundEvent();
        currentRound = nextRound;
        AttachRoundEvent();
        currentRound.GenerateYama();
        
        currentState = GameState.PlayerTurn;

        
    }

    
    void HandleGameOver()
    {
        redstoneClock.OnTimerFinished -= HandleGameOver;
        redstoneClock.TaimuSutopu();
        DetachRoundEvent();
        
        currentState = GameState.GameOver;
        svcScoreManager.OnGameOver();

        float yourScore = svcScoreManager.DistanceWithAccumulated;

        var saveData = SettingsManager.Load();
        uiGameOver.Initialize(yourScore, saveData.highScore);
        // uiGameOver.gameObject.SetActive(true);
        if (yourScore > saveData.highScore)
        {
            saveData.highScore = yourScore;
        }
        SettingsManager.Save(saveData);
        

        OnGameOver(); // UI 요소가 켜질 거임

    }


    void ChangeState(GameState state)
    {
        currentState = state;
        OnStateChange(state);
    }

    void AttachRoundEvent()
    {
        currentRound.OnHandUpdate += UpdatePlayerHand;
        currentRound.OnTsumoTile += LetPlayerTsumoTile;
        currentRound.OnNewRoundStart +=StartNextRound;
        currentRound.OnPlayerScoreAlters += UpdatePlayerScore;
        currentRound.OnPlayerWin += HandlePlayerWin;
        currentRound.OnRoundInfoUpdate += UpdateRoundInfo;

    }
    void DetachRoundEvent()
    {
        currentRound.OnHandUpdate -= UpdatePlayerHand;
        currentRound.OnTsumoTile -= LetPlayerTsumoTile;
        currentRound.OnNewRoundStart -=StartNextRound;
        currentRound.OnPlayerScoreAlters -= UpdatePlayerScore;
        currentRound.OnPlayerWin -= HandlePlayerWin;
        currentRound.OnRoundInfoUpdate -= UpdateRoundInfo;
    }


    void UpdatePlayerHand()
    {
        playerHand.FillHand(player.Hand);
    }
    void LetPlayerTsumoTile(TsumoInfo tsumoInfo)
    {
        
        playerHand.TsumoTile(tsumoInfo);
        if (uiCallHolder.UpdateInfo(tsumoInfo.isRiichiAble, tsumoInfo.isTsumoAble))
        { 
           GameUIManager.Instance.ActivePanel(GameUIState.RiichiTsumo);
        }
        currentState = GameState.PlayerTurn;
    }

    void PlayerDiscardTile(int index)
    {
        if(currentState != GameState.PlayerTurn) return;
        //대충 조건 검사
        currentRound.DiscardTile(index);
        currentState = GameState.Processing;
        if (index != 13)
        {
            UpdatePlayerHand();
        }
        // GameUIManager.Instance.DeactivePanel(GameUIState.RiichiTsumo);
        currentState = GameState.PlayerTurn;
    }





    /// <summary>
    /// 플레이어의 점수에서 변경된 수치를 받습니다. 
    /// </summary>
    /// <param name="delta"></param>
    void UpdatePlayerScore(int delta)
    {
        // MyLogger.Log($"점수를 바꿀게요! {delta} + {player.Score}");
        uiScoreInfo?.UpdateScore(player.Score);
        if (delta > 0)
        {
            svcScoreManager?.GetBoostAndDistance(delta);
        }
    }

    void UpdateRoundInfo(MahjongRoundInfo info){
        uiRoundInfo?.UpdateUIInfo(info);
    }

    void HandlePlayerWin(MahjongWinInfo info)
    {
        uiWininfo.UpdateInfo(info, player.IsOya);
        GameUIManager.Instance.VolatileTurnOn(GameUIState.WinInfo, 5);
    }

    void CheckRiichii(TsumoInfo tsumoInfo)
    {

    }
    void CheckTsumoAgari(TsumoInfo tsumoInfo)
    {

    }







    void OnEnable()
    {
        if (playerHand != null)
        {
            playerHand.OnPlayerDiscard += PlayerDiscardTile;
            playerHand.OnPlayerCall += CallHandler;
        }
    }

    void OnDisable()
    {
        if (playerHand != null)
        {
            playerHand.OnPlayerDiscard -= PlayerDiscardTile;
            playerHand.OnPlayerCall -= CallHandler;
        }
    }
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }
    // Start is called before the first frame update
    void Start()
    {
        // currentRound = new MahjongRound(prng.Next(), player); 
        // StartNewGame();

    }

    void Update()
    {
#if IROHA
        GetScore();
        CheatHandler();
#endif
    }

    void CallHandler(PlayerCallType callType)
    {
        if (currentState != GameState.PlayerTurn) return;
        switch (callType)
        {
            case PlayerCallType.Riichi:
                RiichiHandler();
                break;
            case PlayerCallType.Tsumo:
                TsumoHandler();
                break;
            case PlayerCallType.Ron:
                break;
            case PlayerCallType.Chii:
                break;
            case PlayerCallType.Pon:
                break;
            case PlayerCallType.Kan:
                break;
            case PlayerCallType.Nukidora:
                break;
            case PlayerCallType.Forfeit:
                HandleGameOver();
                break;
            default:
                break;
        }
    }

    void RiichiHandler(){

    }
    void TsumoHandler(){
        // if(player.tsumoTile == MahjongTile.NullTile()){
        //     return;
        // }
        if(currentState != GameState.PlayerTurn) return;

        currentState = GameState.Processing;
        currentRound.CheckTsumoWin();
    }


}

/// 점수 관련 GI 테스트 영역
public partial class MahjongGameManager : MonoBehaviour, IScoreDistanceConsumer
{
    IScoreDistanceService svcScoreManager;
    public void Construct(IScoreDistanceService newService)
    {
        svcScoreManager = newService;
        // scoreManager.OnBoostRankAlters += OnBoostRankAlters;
        // scoreManager.OnDistanceChange
        // ((IScoreDistanceComsumer)Instance).Construct(jjuna);
    }
    // scoreManager.OnBoostRankAlters -= OnBoostRankAlters;


}

/// <summary>
/// 치트!
/// </summary>
public partial class MahjongGameManager : MonoBehaviour, IScoreDistanceConsumer
{
    public void GetScore()
    {
#if IROHA
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // UpdatePlayerScore(8000);
            svcScoreManager.GetBoostAndDistance(8000);
        }
#endif
    }

#if IROHA
    public void CheatHandler()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            HandleGameOver();
        }
    }
#endif
}