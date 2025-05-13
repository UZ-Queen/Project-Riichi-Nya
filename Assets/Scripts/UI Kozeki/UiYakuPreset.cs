using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
/// <summary>
/// 리치 1판 , 탕야오 1판처럼 역 하나의 정보를 출력합니다.
/// </summary>
public class UiYakuPreset : MonoBehaviour
{
    /// <summary>
    /// 역의 이름. riichi, Tsumo 등등...
    /// </summary>
    [SerializeField] private TextMeshProUGUI uiYakuName;

    /// <summary>
    /// 1판, 2판, 3판, 6판의 숫자 부분.
    /// </summary>
    [SerializeField] private TextMeshProUGUI uiYakuHan;
    /// <summary>
    /// ~판, ~han 등...
    /// </summary>
    [SerializeField] private TextMeshProUGUI uiHanText;

    // Start is called before the first frame update
    void Start()
    {
        UpdateInfo(Yaku.Tanyao);
    }


    public void UpdateInfo(Yaku yaku){
        // YakuInfo.YakuData[yaku]
        uiYakuName.text = yaku.ToString();
        uiYakuHan.text = YakuInfo.YakuData[yaku].Han.ToString();
        uiYakuName.text = "판"; // <----- 언어 모듈 연결해서 바꿔!
            
    }
}
