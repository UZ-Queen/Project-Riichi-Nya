using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class UiScoreInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI uiScoreText;

    int currentValue = 0;
    public void UpdateScore(int newValue){
        currentValue = newValue;
        UpdateScoreUI();
    }
    public void AlterScore(int delta){

    }

    void UpdateScoreUI(){
        uiScoreText.text = currentValue.ToString();
    }

    void Start()
    {
        
    }
}
