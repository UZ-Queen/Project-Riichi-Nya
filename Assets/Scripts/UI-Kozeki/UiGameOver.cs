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


    float playerScore = 0;
    float recordScore = 0;

    public void Initialize(float yourScore, float bestScore)
    {
        playerScore = yourScore;
        recordScore = bestScore;
        UpdateUI();
        // recordScore;
    }
    void UpdateUI()
    {
        uiTotalScore.text = playerScore.ToString();
        uiRecordScore.text = recordScore.ToString();
    }
}
