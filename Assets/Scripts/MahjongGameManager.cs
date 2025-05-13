using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PlayerCallType
{
    Riichi, Tsumo, Ron, Chii, Pon, Kan, Nukidora
}

public enum GameState{
    Initializing, PlayerTurn, GameOver, Processing, MOLLU,
}

public class MahjongGameManager : MonoBehaviour
{
    public static MahjongGameManager Instance { get; private set; }

    [SerializeField] private PlayerHand playerHand;
    [SerializeField] private UiScoreInfo uiScoreInfo;

    public GameState currentState = GameState.Initializing;
    public MahjongTileDatabase TileDB;
    System.Random prng;
    MahjongRound currentRound;
    MahjongPlayer player;
    int seed = 1557;


    void StartNewGame()
    {
        
        currentState = GameState.Initializing;

        // prng = new System.Random(seed);
        prng = new System.Random();
        // Wind playerSeat = (Wind)prng.Next(0, 4);
        // player = new MahjongPlayer(playerSeat);
        //원래 4명의 자리를 뚝딱 해줘야하지만.. 1인용이다!

        
        currentRound = MahjongRound.NewRound(prng.Next(), out player);

        //라운드 생성 후 꼭 패산을 수동으로 생성해야 라운드가 시작한다.
        AttachRoundEvent();
        currentRound.GenerateYama();

        currentState = GameState.PlayerTurn;
        // currentRound = new MahjongRound(prng.Next(), player);
    }

    void AttachRoundEvent()
    {
        currentRound.OnHandUpdate += UpdatePlayerHand;
        currentRound.OnTsumoTile += LetPlayerTsumoTile;
        currentRound.OnNewRoundStart +=StartNextRound;
        currentRound.OnPlayerScoreAlters += UpdatePlayerScore;
        currentRound.OnPlayerWin += HandlePlayerWin;

    }
    void DetachRoundEvent()
    {
        currentRound.OnHandUpdate -= UpdatePlayerHand;
        currentRound.OnTsumoTile -= LetPlayerTsumoTile;
        currentRound.OnNewRoundStart -=StartNextRound;
        currentRound.OnPlayerScoreAlters -= UpdatePlayerScore;
        currentRound.OnPlayerWin -= HandlePlayerWin;
    }


    void UpdatePlayerHand()
    {
        playerHand.FillHand(player.Hand);
    }
    void LetPlayerTsumoTile(TsumoInfo tsumoInfo)
    {
        playerHand.TsumoTile(tsumoInfo);
        currentState = GameState.PlayerTurn;
    }

    void StartNextRound(MahjongRound nextRound){
        currentState = GameState.Processing;
        DetachRoundEvent();
        currentRound = nextRound;
        AttachRoundEvent();
        currentRound.GenerateYama();
        
        currentState = GameState.PlayerTurn;

        
    }

    void UpdatePlayerScore(int delta){
        uiScoreInfo?.UpdateScore(delta + player.score);
    }

    void HandlePlayerWin(MahjongWinInfo info){

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
        StartNewGame();

    }

    // Update is called once per frame
    void Update()
    {

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

    void CallHandler(PlayerCallType callType)
    {
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
