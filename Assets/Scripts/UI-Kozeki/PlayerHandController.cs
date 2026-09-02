using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 솔로 손패 입력과 선택 상태를 관리하고 플레이 의도를 발행합니다.
/// </summary>
public class PlayerHandController : MonoBehaviour
{
    [SerializeField] private PlayerHandView playerHandView;

    public int currentIndex;

    private DasArrInput dasArrInput;
    private bool callRiichiNya;
    private bool isGameOver;
    private bool gameplayInputEnabled = true;

    void Awake()
    {
        currentIndex = 6;
        playerHandView.Initialize();
        UpdateHand();
        dasArrInput = new DasArrInput();
        MyLogger.Log("손 초기화 완료!");
    }

    void OnEnable()
    {
        AttachManagerEvents();
    }

    void Start()
    {
        AttachManagerEvents();
    }

    void Update()
    {
        if (HandleEscape(Input.GetKeyDown(KeyCode.Escape)))
        {
            return;
        }

        if (SoloScoringGameManager.Instance == null)
        {
            return;
        }

        if (!gameplayInputEnabled)
        {
            return;
        }

        if (SoloScoringGameManager.Instance.currentState != GameState.PlayerTurn)
        {
            return;
        }

        if (dasArrInput.GetInput(InputPreset.left))
        {
            MoveHandToLeft();
        }

        if (dasArrInput.GetInput(InputPreset.right))
        {
            MoveHandToRight();
        }

        if (Input.GetKeyDown(InputPreset.discard))
        {
            DiscardSelectedTile();
        }
        else if (Input.GetKeyDown(InputPreset.discardTsumoTile))
        {
            DiscardTsumoTile();
        }

        if (Input.GetKeyDown(InputPreset.riichi))
        {
            callRiichiNya = true;
        }
        else if (Input.GetKeyDown(InputPreset.tsumoAgari))
        {
            OnPlayerCall(PlayerCallType.Tsumo);
        }

    }

    private bool HandleEscape(bool escapePressed)
    {
        if (!escapePressed)
        {
            return false;
        }

        ForfeitRequested();
        return true;
    }

    void OnDisable()
    {
        if (SoloScoringGameManager.Instance == null)
        {
            return;
        }

        SoloScoringGameManager.Instance.OnGameOver -= HandleGameOver;
        SoloScoringGameManager.Instance.OnGameStart -= HandleGameStart;
    }

    /// <summary>
    /// 플레이어가 선택한 손패 인덱스를 전달합니다.
    /// </summary>
    public event Action<int> OnPlayerDiscard = delegate { };

    /// <summary>
    /// 플레이어가 선택한 마작 행동을 전달합니다.
    /// </summary>
    public event Action<PlayerCallType> OnPlayerCall = delegate { };

    /// <summary>
    /// 플레이어가 현재 솔로 세션의 포기 확인을 요청했음을 전달합니다.
    /// </summary>
    public event Action ForfeitRequested = delegate { };

    /// <summary>
    /// 현재 손패를 View에 표시합니다.
    /// </summary>
    /// <param name="tiles">표시할 열세 장의 손패입니다.</param>
    public void FillHand(List<MahjongTile> tiles)
    {
        playerHandView.FillHand(tiles);
    }

    /// <summary>
    /// 쯔모패와 가능한 행동을 View에 표시합니다.
    /// </summary>
    /// <param name="tsumoInfo">쯔모패와 선언 가능 정보입니다.</param>
    public void TsumoTile(TsumoInfo tsumoInfo)
    {
        playerHandView.TsumoTile(tsumoInfo);
    }

    /// <summary>
    /// 포기 확인 중에도 Escape 취소는 허용하면서 일반 손패 입력만 차단합니다.
    /// </summary>
    public void SetGameplayInputEnabled(bool enabled)
    {
        gameplayInputEnabled = enabled;
    }

    private void HandleGameOver()
    {
        isGameOver = true;
        gameplayInputEnabled = false;
    }

    private void HandleGameStart()
    {
        isGameOver = false;
        gameplayInputEnabled = true;
        currentIndex = 6;
        UpdateHand();
    }

    private void AttachManagerEvents()
    {
        if (SoloScoringGameManager.Instance == null)
        {
            return;
        }

        SoloScoringGameManager.Instance.OnGameOver -= HandleGameOver;
        SoloScoringGameManager.Instance.OnGameStart -= HandleGameStart;
        SoloScoringGameManager.Instance.OnGameOver += HandleGameOver;
        SoloScoringGameManager.Instance.OnGameStart += HandleGameStart;
    }

    private void UpdateHand()
    {
        playerHandView.UpdateSelectedIndex(currentIndex);
    }

    /// <summary>
    /// 새 인덱스를 손패 범위로 제한하여 선택합니다.
    /// </summary>
    /// <param name="newIndex">새로 선택할 손패 인덱스입니다.</param>
    private void MoveHand(int newIndex)
    {
        int beforeValue = currentIndex;
        currentIndex = Mathf.Clamp(newIndex, 0, 12);
        if (beforeValue != currentIndex)
        {
            UpdateHand();
        }
    }

    private void MoveHandToLeft()
    {
        MoveHand(currentIndex - 1);
    }

    private void MoveHandToRight()
    {
        MoveHand(currentIndex + 1);
    }

    private void DiscardSelectedTile()
    {
        playerHandView.HideActionButtons();
        OnPlayerDiscard(currentIndex);
        currentIndex = 6;
        UpdateHand();
    }

    private void DiscardTsumoTile()
    {
        playerHandView.HideActionButtons();
        OnPlayerDiscard(13);
        currentIndex = 6;
        UpdateHand();
    }
}
