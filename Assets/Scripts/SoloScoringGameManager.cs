#define IROHA
#undef IROHA
using System;
using UnityEngine;
using UnityEngine.Profiling;


/// <summary>
/// 솔로 스코어링 모드의 실행 상태, 규칙 진행, 타이머와 종료 정책을 관리합니다.
/// </summary>
public partial class SoloScoringGameManager : MonoBehaviour, IScoreDistanceConsumer
{
    public static SoloScoringGameManager Instance { get; private set; }

    public event Action<GameState> OnStateChange = delegate { };
    public event Action OnGameOver = delegate { };
    public event Action OnGameStart = delegate { };

    [Header("엑스트라 유틸리티 2")]
    [SerializeField] private ScoreManagerDistance scoreManagerDistance;
    [SerializeField] private Timer redstoneClock;
    [Header("시구레 UI")]
    [SerializeField] private PlayerHandController playerHand;
    [SerializeField] private SoloScoringUIController soloUIController;

    [Header("몰름보")]
    public GameState currentState = GameState.Initializing;
    public MahjongTileDatabase TileDB;
    System.Random prng;
    MahjongRound currentRound;
    MahjongPlayer player;
    int seed = 1557;
    bool pendingForfeit;
    bool sessionFinalized;
    GameEndReason lastEndReason;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        if (playerHand != null)
        {
            playerHand.OnPlayerDiscard -= PlayerDiscardTile;
            playerHand.OnPlayerCall -= CallHandler;
            playerHand.ForfeitRequested -= RequestForfeit;
            playerHand.OnPlayerDiscard += PlayerDiscardTile;
            playerHand.OnPlayerCall += CallHandler;
            playerHand.ForfeitRequested += RequestForfeit;
        }

        if (soloUIController != null)
        {
            soloUIController.ConfirmRequested -= ConfirmForfeit;
            soloUIController.CancelRequested -= CancelForfeit;
            soloUIController.ConfirmRequested += ConfirmForfeit;
            soloUIController.CancelRequested += CancelForfeit;
        }
    }

    void Start()
    {
    }

    void Update()
    {
#if IROHA
        GetScore();
        CheatHandler();
#endif
    }

    void OnDisable()
    {
        if (playerHand != null)
        {
            playerHand.OnPlayerDiscard -= PlayerDiscardTile;
            playerHand.OnPlayerCall -= CallHandler;
            playerHand.ForfeitRequested -= RequestForfeit;
        }

        if (soloUIController != null)
        {
            soloUIController.ConfirmRequested -= ConfirmForfeit;
            soloUIController.CancelRequested -= CancelForfeit;
        }

        if (redstoneClock != null)
        {
            redstoneClock.OnTimerFinished -= HandleTimerFinished;
        }

        DetachRoundEvent();
    }

    public void StartNewGame()
    {
        DetachRoundEvent();
        if (redstoneClock != null)
        {
            redstoneClock.OnTimerFinished -= HandleTimerFinished;
        }

        currentState = GameState.Initializing;
        pendingForfeit = false;
        sessionFinalized = false;
        OnGameStart();
        soloUIController?.Initialize();
        
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
        soloUIController?.BindScoreDistance(svcScoreManager);
        //타이머 생성 후..
        redstoneClock.StartTimer(180);
        redstoneClock.OnTimerFinished += HandleTimerFinished;
        soloUIController?.BindRemainingTime(redstoneClock);
        
        currentState = GameState.PlayerTurn;
        // currentRound = new MahjongRound(prng.Next(), player);
    }

    void StartNextRound(MahjongRound nextRound)
    {
        if (sessionFinalized)
        {
            return;
        }

        currentState = GameState.Processing;
        DetachRoundEvent();
        currentRound = nextRound;
        AttachRoundEvent();
        currentRound.GenerateYama();
        
        currentState = GameState.PlayerTurn;

        
    }

    
    void HandleTimerFinished()
    {
        soloUIController?.HideForfeitConfirmation();
        FinalizeGame(GameEndReason.TimeExpired);
    }

    /// <summary>
    /// 대기 중인 포기를 확정합니다.
    /// </summary>
    public void ConfirmForfeit()
    {
        if (!pendingForfeit)
        {
            return;
        }

        FinalizeGame(GameEndReason.Forfeit);
    }

    /// <summary>
    /// 대기 중인 포기를 취소하고 플레이어 입력으로 돌아갑니다.
    /// </summary>
    public void CancelForfeit()
    {
        if (!pendingForfeit || sessionFinalized)
        {
            return;
        }

        pendingForfeit = false;
        ChangeState(GameState.PlayerTurn);
        soloUIController?.SetGameplayInputEnabled(true);
        soloUIController?.HideForfeitConfirmation();
    }

    void RequestForfeit()
    {
        if (pendingForfeit && currentState == GameState.Processing)
        {
            CancelForfeit();
            return;
        }

        if (currentState != GameState.PlayerTurn || sessionFinalized)
        {
            return;
        }

        pendingForfeit = true;
        ChangeState(GameState.Processing);
        soloUIController?.SetGameplayInputEnabled(false);
        soloUIController?.ShowForfeitConfirmation();
    }

    void FinalizeGame(GameEndReason reason)
    {
        if (sessionFinalized)
        {
            return;
        }

        sessionFinalized = true;
        pendingForfeit = false;
        lastEndReason = reason;
        soloUIController?.SetGameplayInputEnabled(false);
        soloUIController?.HideForfeitConfirmation();
        redstoneClock.OnTimerFinished -= HandleTimerFinished;
        redstoneClock.TaimuSutopu();
        DetachRoundEvent();

        ChangeState(GameState.GameOver);
        svcScoreManager.OnGameOver();

        float yourScore = svcScoreManager.DistanceWithAccumulated;

        var saveData = SettingsManager.Load();
        if (reason == GameEndReason.TimeExpired && yourScore > saveData.highScore)
        {
            saveData.highScore = yourScore;
        }

        soloUIController?.ShowGameOver(yourScore, saveData.highScore, reason);
        if (reason == GameEndReason.TimeExpired)
        {
            SettingsManager.Save(saveData);
        }

        OnGameOver();

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
        if (currentRound == null)
        {
            return;
        }

        currentRound.OnHandUpdate -= UpdatePlayerHand;
        currentRound.OnTsumoTile -= LetPlayerTsumoTile;
        currentRound.OnNewRoundStart -=StartNextRound;
        currentRound.OnPlayerScoreAlters -= UpdatePlayerScore;
        currentRound.OnPlayerWin -= HandlePlayerWin;
        currentRound.OnRoundInfoUpdate -= UpdateRoundInfo;
    }


    void UpdatePlayerHand()
    {
        soloUIController?.ShowPlayerHand(player.Hand);
    }
    void LetPlayerTsumoTile(TsumoInfo tsumoInfo)
    {
        soloUIController?.ShowTsumoTile(tsumoInfo);
        if (soloUIController != null && soloUIController.UpdateCallOptions(tsumoInfo))
        { 
           soloUIController.ActivePanel(GameUIState.RiichiTsumo);
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
        currentState = GameState.PlayerTurn;
    }





    /// <summary>
    /// 플레이어의 점수에서 변경된 수치를 받습니다. 
    /// </summary>
    /// <param name="delta"></param>
    void UpdatePlayerScore(int delta)
    {
        // MyLogger.Log($"점수를 바꿀게요! {delta} + {player.Score}");
        soloUIController?.UpdatePlayerScore(player.Score);
        if (delta > 0)
        {
            svcScoreManager?.GetBoostAndDistance(delta);
        }
    }

    void UpdateRoundInfo(MahjongRoundInfo info){
        soloUIController?.UpdateRoundInfo(info);
    }

    void HandlePlayerWin(MahjongWinInfo info)
    {
        soloUIController?.ShowWinInfo(info, player.IsOya);
    }

    void CheckRiichii(TsumoInfo tsumoInfo)
    {

    }
    void CheckTsumoAgari(TsumoInfo tsumoInfo)
    {

    }







    void CallHandler(PlayerCallType callType)
    {
        if (currentState != GameState.PlayerTurn)
        {
            return;
        }

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
public partial class SoloScoringGameManager : MonoBehaviour, IScoreDistanceConsumer
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
public partial class SoloScoringGameManager : MonoBehaviour, IScoreDistanceConsumer
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
            HandleTimerFinished();
        }
    }
#endif
}
