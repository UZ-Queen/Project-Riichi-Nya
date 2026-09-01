using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 솔로 게임의 상호 배타적인 일반 패널 상태입니다.
/// </summary>
public enum GameUIState
{
    RoundInfo,
    Score,
    PlayerHand,
    WinInfo,
    RiichiTsumo,
    Distance,
    Time,
    GameOver,
    BBaggu
}

/// <summary>
/// 솔로 스코어링 모드의 화면 출력과 포기 확인 오버레이를 담당합니다.
/// </summary>
public class SoloScoringUIController : MonoBehaviour
{
    public static SoloScoringUIController Instance { get; private set; }

    /// <summary>포기 확인 버튼이 선택되었음을 전달합니다.</summary>
    public event Action ConfirmRequested = delegate { };
    /// <summary>포기 취소 버튼이 선택되었음을 전달합니다.</summary>
    public event Action CancelRequested = delegate { };

    [Header("패널 매핑")]
    [SerializeField] private List<GamePanelEntry> panels;
    [SerializeField] private GameObject gameCanvas;

    [Header("솔로 출력")]
    [SerializeField] private PlayerHandController playerHandController;
    [SerializeField] private PlayerHandView playerHandView;
    [SerializeField] private UiScoreDistanceInfo uiScoreDistanceInfo;
    [SerializeField] private UiScoreInfo uiScoreInfo;
    [SerializeField] private UiRoundInfo uiRoundInfo;
    [SerializeField] private UiCallInfo uiCallHolder;
    [SerializeField] private UiWinInfo uiWininfo;
    [SerializeField] private UiRemainingTimeIndicator uiRemainingTime;
    [SerializeField] private UiGameOver uiGameOver;

    [Header("포기 확인")]
    [SerializeField] private GameObject forfeitConfirmation;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("트랜지션 설정")]
    public float distance = 500f;
    public float duration = 0.4f;
    public Ease ease = Ease.InOutCubic;

    private Dictionary<GameUIState, GamePanelEntry> panelMap;
    private Tween currentVolatileTween;
    private Vector2 forfeitOriginalPosition;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        panelMap = new Dictionary<GameUIState, GamePanelEntry>();
        foreach (GamePanelEntry panel in panels)
        {
            panel.originalPosition = panel.rect.anchoredPosition;
            panel.rect.gameObject.SetActive(false);
            panelMap[panel.state] = panel;
        }

        if (forfeitConfirmation != null)
        {
            forfeitOriginalPosition = forfeitConfirmation.GetComponent<RectTransform>().anchoredPosition;
            forfeitConfirmation.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(RaiseConfirmRequested);
            confirmButton.onClick.AddListener(RaiseConfirmRequested);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(RaiseCancelRequested);
            cancelButton.onClick.AddListener(RaiseCancelRequested);
        }
    }

    void OnDisable()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(RaiseConfirmRequested);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(RaiseCancelRequested);
        }
    }

    /// <summary>
    /// 솔로 화면을 한 실행의 초기 상태로 되돌립니다.
    /// </summary>
    public void Initialize()
    {
        gameCanvas.SetActive(true);
        HideForfeitConfirmation();
        SetGameplayInputEnabled(true);

        foreach (GamePanelEntry panel in panels)
        {
            panel.rect.DOKill();
            panel.rect.anchoredPosition = panel.originalPosition;
            panel.rect.gameObject.SetActive(false);
        }

        ActivePanel(GameUIState.RoundInfo);
        ActivePanel(GameUIState.PlayerHand);
        ActivePanel(GameUIState.Distance);
        ActivePanel(GameUIState.Time);
    }

    /// <summary>
    /// 현재 솔로 화면의 모든 일반 패널을 닫습니다.
    /// </summary>
    public void HideAllPanels()
    {
        HideForfeitConfirmation();
        foreach (GamePanelEntry panel in panels)
        {
            DeactivePanel(panel.state);
        }
    }

    /// <summary>
    /// 손패 조작 입력의 허용 여부를 즉시 변경합니다.
    /// </summary>
    public void SetGameplayInputEnabled(bool enabled)
    {
        playerHandController?.SetGameplayInputEnabled(enabled);
    }

    /// <summary>
    /// 포기 확인 오버레이를 열고 취소 버튼을 기본 선택합니다.
    /// </summary>
    public void ShowForfeitConfirmation()
    {
        if (forfeitConfirmation == null)
        {
            return;
        }

        RectTransform rect = forfeitConfirmation.GetComponent<RectTransform>();
        CanvasGroup group = forfeitConfirmation.GetComponent<CanvasGroup>();
        rect.DOKill();
        rect.anchoredPosition = forfeitOriginalPosition;
        group.alpha = 0f;
        forfeitConfirmation.SetActive(true);
        rect.SlideInAndFade(group, Vector2.right, distance, duration, ease);

        if (EventSystem.current != null && cancelButton != null)
        {
            EventSystem.current.SetSelectedGameObject(cancelButton.gameObject);
        }
    }

    /// <summary>
    /// 포기 확인 오버레이를 즉시 닫습니다.
    /// </summary>
    public void HideForfeitConfirmation()
    {
        if (forfeitConfirmation == null)
        {
            return;
        }

        forfeitConfirmation.GetComponent<RectTransform>().DOKill();
        forfeitConfirmation.SetActive(false);
    }

    /// <summary>거리 표시를 현재 점수 서비스에 연결합니다.</summary>
    public void BindScoreDistance(IScoreDistanceService service)
    {
        uiScoreDistanceInfo?.Construct(service);
    }

    /// <summary>남은 시간 표시를 현재 타이머에 연결합니다.</summary>
    public void BindRemainingTime(Timer timer)
    {
        uiRemainingTime?.Construct(timer);
    }

    /// <summary>현재 손패를 표시합니다.</summary>
    public void ShowPlayerHand(List<MahjongTile> tiles)
    {
        playerHandView?.FillHand(tiles);
    }

    /// <summary>쯔모패를 표시합니다.</summary>
    public void ShowTsumoTile(TsumoInfo info)
    {
        playerHandView?.TsumoTile(info);
    }

    /// <summary>현재 선언 가능 상태를 표시합니다.</summary>
    public bool UpdateCallOptions(TsumoInfo info)
    {
        return uiCallHolder != null && uiCallHolder.UpdateInfo(info.isRiichiAble, info.isTsumoAble);
    }

    /// <summary>현재 점수를 표시합니다.</summary>
    public void UpdatePlayerScore(int score)
    {
        uiScoreInfo?.UpdateScore(score);
    }

    /// <summary>현재 국 정보를 표시합니다.</summary>
    public void UpdateRoundInfo(MahjongRoundInfo info)
    {
        uiRoundInfo?.UpdateUIInfo(info);
    }

    /// <summary>화료 정보를 잠시 표시합니다.</summary>
    public void ShowWinInfo(MahjongWinInfo info, bool isOya)
    {
        uiWininfo?.UpdateInfo(info, isOya);
        VolatileTurnOn(GameUIState.WinInfo, 5f);
    }

    /// <summary>솔로 게임의 종료 결과를 표시합니다.</summary>
    public void ShowGameOver(float score, float highScore, GameEndReason reason)
    {
        HideForfeitConfirmation();
        uiGameOver?.Initialize(score, highScore, reason);
        ActivePanel(GameUIState.GameOver);
        ActivePanel(GameUIState.BBaggu);
    }

    /// <summary>일반 패널을 지정한 시간 동안 표시합니다.</summary>
    public void VolatileTurnOn(GameUIState state, float volatileTime)
    {
        if (currentVolatileTween != null && currentVolatileTween.IsActive())
        {
            currentVolatileTween.Kill();
        }

        if (!panelMap.TryGetValue(state, out GamePanelEntry panel))
        {
            return;
        }

        panel.rect.gameObject.SetActive(true);
        panel.rect.SlideInAndFade(panel.group, panel.appearFromWhere.ToVector2(), distance, duration, ease);
        currentVolatileTween = DOVirtual.DelayedCall(volatileTime, () =>
        {
            currentVolatileTween = null;
            DeactivePanel(state);
        });
    }

    /// <summary>일반 패널의 표시 상태를 전환합니다.</summary>
    public void TogglePanel(GameUIState state)
    {
        if (!panelMap.TryGetValue(state, out GamePanelEntry panel))
        {
            return;
        }

        if (!panel.rect.gameObject.activeSelf)
        {
            ActivePanel(state);
            return;
        }

        DeactivePanel(state);
    }

    /// <summary>일반 패널 하나를 표시합니다.</summary>
    public void ActivePanel(GameUIState state)
    {
        if (!panelMap.TryGetValue(state, out GamePanelEntry panel))
        {
            return;
        }

        panel.rect.anchoredPosition = panel.originalPosition;
        panel.group.alpha = 0f;
        panel.rect.gameObject.SetActive(true);
        panel.rect.SlideInAndFade(panel.group, panel.appearFromWhere.ToVector2(), distance, duration, ease);
    }

    /// <summary>일반 패널 하나를 닫습니다.</summary>
    public void DeactivePanel(GameUIState state)
    {
        if (!panelMap.TryGetValue(state, out GamePanelEntry panel))
        {
            return;
        }

        panel.rect.SlideOutAndFade(panel.group, panel.appearFromWhere.ToVector2(), distance, duration, ease)
            .OnComplete(() =>
            {
                panel.rect.anchoredPosition = panel.originalPosition;
                panel.rect.gameObject.SetActive(false);
            });
    }

    /// <summary>메인 메뉴 패널로 돌아갑니다.</summary>
    public void OnBackButton()
    {
        FindObjectOfType<UiManager>().ShowPanel(UIState.MainMenu);
    }

    private void RaiseConfirmRequested()
    {
        ConfirmRequested();
    }

    private void RaiseCancelRequested()
    {
        CancelRequested();
    }
}
