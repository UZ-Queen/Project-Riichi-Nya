
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using System.Threading;
using Unity.VisualScripting;

public enum GameUIState { RoundInfo, Score, PlayerHand, WinInfo, RiichiTsumo }
public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }
    [Header("패널 매핑")]
    [SerializeField] List<GamePanelEntry> panels;
    [SerializeField] private GameObject gameCanvas;


    [Header("트랜지션 설정")]
    // public Vector2 direction = Vector2.left;
    public float distance = 500f;
    public float duration = 0.4f;
    public Ease ease = Ease.InOutCubic;

    [Serializable]
    class UIInfo
    {
        public RectTransform rect;
        public CanvasGroup group;
        public Wind appearFromWhere;
        public Vector2 originalPosition;

        public UIInfo(RectTransform _rect, CanvasGroup _group, Wind _appearFromWhere, Vector2 _originalPos)
        {
            rect = _rect;
            group = _group;
            appearFromWhere = _appearFromWhere;
            originalPosition = _originalPos;
        }
    }
    Dictionary<GameUIState, GamePanelEntry> panelMap;


    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        panelMap = new Dictionary<GameUIState, GamePanelEntry>();
        foreach (var i in panels)
        {
            i.originalPosition = i.rect.anchoredPosition;
            i.rect.gameObject.SetActive(false);
            // panelMap[i.state] = (i.rect, i.group, i.appearFromWhere, i.originalPosition);
            panelMap[i.state] = i;
        }

        // TogglePanel();
    }

    public void Initialize()
    {
        gameCanvas.SetActive(true);
        TogglePanel(GameUIState.RoundInfo);
        TogglePanel(GameUIState.PlayerHand);
        TogglePanel(GameUIState.Score);
    }

    private Tween currentVolatileTween = null;
    public void VolatileTurnOn(GameUIState state, float volatileTime)
    {
        if (currentVolatileTween != null && currentVolatileTween.IsActive())
            currentVolatileTween.Kill();


        GamePanelEntry panel;
        if (!panelMap.TryGetValue(state, out panel))
            return;

        panel.rect.gameObject.SetActive(true);
        panel.rect
        .SlideInAndFade(panel.group, panel.appearFromWhere.ToVector2(), distance, duration, ease);
        currentVolatileTween = DOVirtual.DelayedCall(volatileTime, () =>
        {
            currentVolatileTween = null;
            DeactivePanel(state);
        });


    }
    public void TogglePanel(GameUIState state)
    {
        GamePanelEntry panel;
        if (!panelMap.TryGetValue(state, out panel))
            return;

        if (panel.rect.gameObject.activeSelf)
        {
            panel.rect.SlideOutAndFade(panel.group, panel.appearFromWhere.ToVector2(), distance, duration, ease)
            .OnComplete(() =>
                    {
                        panel.rect.anchoredPosition = panel.originalPosition;
                        panel.rect.gameObject.SetActive(false);
                    });
        }
        else
        {
            panel.rect.gameObject.SetActive(true);
            panel.rect
                .SlideInAndFade(panel.group, panel.appearFromWhere.ToVector2(), distance, duration, ease);
        }

    }

    public void ActivePanel(GameUIState state)
    {
        GamePanelEntry panel;
        if (!panelMap.TryGetValue(state, out panel))
            return;
        panel.rect.anchoredPosition = panel.originalPosition;
                        // +panel.appearFromWhere.ToVector2() * distance;
        panel.group.alpha = 0f;
        panel.rect.gameObject.SetActive(true);
        panel.rect
            .SlideInAndFade(panel.group, panel.appearFromWhere.ToVector2(), distance, duration, ease);
    }
    public void DeactivePanel(GameUIState state)
    {
        GamePanelEntry panel;
        if (!panelMap.TryGetValue(state, out panel))
            return;
        panel.rect.SlideOutAndFade(panel.group, panel.appearFromWhere.ToVector2(), distance, duration, ease)
        .OnComplete(() =>
                {
                    panel.rect.anchoredPosition = panel.originalPosition;
                    panel.rect.gameObject.SetActive(false);
                });
    }
    public void OnBackButton()
    {
        FindObjectOfType<UiManager>().ShowPanel(UIState.MainMenu);
    }
}
