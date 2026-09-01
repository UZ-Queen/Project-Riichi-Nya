using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiGameOver : MonoBehaviour
{
    [SerializeField] private Image uiYan;
    [SerializeField] private TextMeshProUGUI uiTotalScore;

    [SerializeField] private TextMeshProUGUI uiRecordScore;
    [SerializeField] private TextMeshProUGUI uiReason;


    float playerScore = 0;
    float recordScore = 0;
    GameEndReason endReason;

    /// <summary>
    /// 종료 이유와 현재 거리 및 최고 기록을 표시합니다.
    /// </summary>
    public void Initialize(float yourScore, float bestScore, GameEndReason reason)
    {
        playerScore = yourScore;
        recordScore = bestScore;
        endReason = reason;
        UpdateUI();
    }
    void UpdateUI()
    {
        uiTotalScore.text = playerScore.ToString();
        uiRecordScore.text = recordScore.ToString();
        uiReason.text = endReason == GameEndReason.Forfeit ? "포기" : "시간 종료";
    }
}
