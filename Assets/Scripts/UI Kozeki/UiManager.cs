using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;


public enum UIState
{
    MOLLU, Loading, MainMenu, PlayMode, PlayModeConstrains, Config, Statistics, Back,
    InGame,
}

public partial class UiManager : MonoBehaviour
{


    [Header("패널 매핑")]
    [SerializeField] List<PanelEntry> panels;

    [Header("트랜지션 설정")]
    // public Vector2 direction = Vector2.left;
    public float distance = 500f;
    public float duration = 0.4f;
    public Ease ease = Ease.InOutCubic;



    private Stack<UIState> historyStack = new Stack<UIState>();


    Dictionary<UIState, (RectTransform rect, CanvasGroup group, Wind appearFromWhere, Vector2 originalPosition)> panelMap;
    UIState currentState = UIState.MOLLU;

    void Awake()
    {
        // tq 구조체 만들걸?
        panelMap = new Dictionary<UIState, (RectTransform, CanvasGroup, Wind, Vector2)>();
        foreach (var i in panels)
        {
            i.originalPosition = i.rect.anchoredPosition;
            i.rect.gameObject.SetActive(false);
            panelMap[i.state] = (i.rect, i.group, i.appearFromWhere, i.originalPosition);
        }

        historyStack = new Stack<UIState>();
        ShowPanel(UIState.MainMenu);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    /// <param name="direction"> 새로운 패널이 들어올 방향. ->이면 오른쪽에서 들어오고, 왼쪽으로 나간다.</param>
    public void ShowPanel(UIState next, bool recordHistory = true)
    {
        if (next == currentState) return;
        if (currentState != UIState.MOLLU && recordHistory)
        {
            historyStack.Push(currentState);
        }
        // 이전 패널
        if (currentState != UIState.MOLLU && panelMap.TryGetValue(currentState, out var prev))
        {
            // 슬라이드 아웃 & 페이드 아웃
            prev.rect
                .SlideOutAndFade(prev.group, prev.appearFromWhere.ToVector2(), distance, duration, ease)
                .OnComplete(() =>
                {
                    prev.rect.anchoredPosition = prev.originalPosition;
                    prev.rect.gameObject.SetActive(false);
                });
        }

        // 새 패널 활성화
        if (panelMap.TryGetValue(next, out var now))
        {
            now.rect.gameObject.SetActive(true);
            now.rect
                .SlideInAndFade(now.group, now.appearFromWhere.ToVector2(), distance, duration, ease);
        }


        CheckBbagu();
        currentState = next;
    }

    public void HidePanel(bool recordHistory = true)
    {
        if (currentState == UIState.MOLLU) return;
        if (recordHistory)
        {
            historyStack.Push(currentState);
        }
        if (panelMap.TryGetValue(currentState, out var current))
        {
            current.rect
                .SlideOutAndFade(current.group, current.appearFromWhere.ToVector2(), distance, duration, ease)
                .OnComplete(() =>
                {
                    current.rect.anchoredPosition = current.originalPosition;
                    current.rect.gameObject.SetActive(false);
                });
        }
        if (panelMap.TryGetValue(UIState.Back, out var back))
        {
            back.rect
                .SlideOutAndFade(back.group, back.appearFromWhere.ToVector2(), distance, duration, ease)
                .OnComplete(() =>
                {
                    back.rect.anchoredPosition = back.originalPosition;
                    back.rect.gameObject.SetActive(false);
                });
        }


    }
    void ToInGameState()
    {
        // historyStack.Push(currentState);
        HidePanel();
        currentState = UIState.InGame;
        MahjongGameManager.Instance.StartNewGame();
    }

    void CheckBbagu()
    {
        if (historyStack.Count > 0)
        {
            if (panelMap.TryGetValue(UIState.Back, out var backPanel))
            {
                backPanel.rect.gameObject.SetActive(true);
                backPanel.rect
                    .SlideInAndFade(backPanel.group, backPanel.appearFromWhere.ToVector2(), distance, duration, ease);
            }
        }
        else
        {
            if (panelMap.TryGetValue(UIState.Back, out var backPanel))
            {
                
                backPanel.rect
                    .SlideOutAndFade(backPanel.group, backPanel.appearFromWhere.ToVector2(), distance, duration, ease)
                    .OnComplete(()=>
                    {
                        backPanel.rect.anchoredPosition = backPanel.originalPosition;
                        backPanel.rect.gameObject.SetActive(false);
                     }
                     );
            }
        }
    }




}


public partial class UiManager : MonoBehaviour
{
    public void OnPlayButton()
    {
        ShowPanel(UIState.PlayMode);
    }
    public void OnConfigButton()
    {

    }
    public void OnBBagguButton()
    {
        if (historyStack.Count == 0) return;

        ShowPanel(historyStack.Pop(), false);
    }

    public void OnGameStartButton()
    {

        ToInGameState();
    }

}













// public class UiManager : MonoBehaviour
// {
//     [Header("패널")]
//     [SerializeField] private GameObject loadingPanel;
//     [SerializeField] private GameObject mainMenuPanel;
//     [SerializeField] private GameObject playModePanel;
//     [SerializeField] private GameObject ccPanel;
//     [SerializeField] private GameObject configPanel;
//     [SerializeField] private GameObject Statistics;







//     void Start()
//     {

//     }

//     public void ShowPanel(UIState state)
//     {
//         switch (state)
//         {
//             case UIState.Loading:
//                 break;
//             case UIState.MainMenu:
//                 break;
//             case UIState.PlayMode:
//                 break;
//             case UIState.PlayModeConstrains:
//                 break;
//             case UIState.Config:
//                 break;
//             case UIState.Statistics:
//                 break;
//             default:
//                 break;
//         }
//     }
// }
