using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

/// <summary>
/// 화료 시 
/// 여러 개의 uiYakupreset(리치 1판 등), 
/// 총합 X판 X부, 
/// 최종 점수(11600 등), 
/// 만관, 하네만 등 부가 텍스트를 출력하는 UI입니다..
/// </summary>
public class UiWinInfo : MonoBehaviour
{
    [SerializeField] private GameObject yakuPresetHolder;
    [SerializeField] private UiYakuPreset yakuPresetPrefap;

    [SerializeField] private List<UiYakuPreset> uiYakuPresets;
    // [SerializeField] private TextMeshProUGUI uiHanInt;
    [SerializeField] private TextMeshProUGUI uiHanText;
    // [SerializeField] private TextMeshProUGUI uiFuInt;
    [SerializeField] private TextMeshProUGUI uiFuText;
    [SerializeField] private TextMeshProUGUI uiTotalScore;
    [SerializeField] private TextMeshProUGUI uiNyanganText;

    public void UpdateInfo(MahjongWinInfo info, bool isOya)
    {
        if(uiYakuPresets != null){
            foreach(var i in uiYakuPresets){
                i.gameObject.SetActive(false);
                // GameObject.Destroy(i.gameObject); // 풀링을 한다면...(귀찮다)
            }
        }

        // uiYakuPresets = new List<UiYakuPreset>();
        int index = 0;
            while(info.yakues.Count > uiYakuPresets.Count){
                MyLogger.Log("UIYAKUPRESET: 자리가 부족해서 새로운 프리셋을 만듭니다!");
                // MyLogger.LogError("")
                
                UiYakuPreset newPreset = Instantiate(yakuPresetPrefap, yakuPresetHolder.transform);
                newPreset.gameObject.SetActive(false);
                uiYakuPresets.Add(newPreset);
                // break;
            }
        
            StringBuilder sb = new StringBuilder();
        foreach(var yaku in info.yakues){
            sb.Append(" " + yaku.ToString());
            // newPreset.UpdateInfo(yaku);
            // uiYakuPresets.Add(newPreset);

            // if (yaku == Yaku.Dora)
            // {
            //     uiYakuPresets[index].UpdateDoraInfo(yaku, info.doraInfo.doraCount);
            // }

            switch (yaku)
            {
                case Yaku.Dora:
                    uiYakuPresets[index].UpdateDoraInfo(yaku, info.doraInfo.doraCount);
                    break;
                case Yaku.AkaDora:
                    uiYakuPresets[index].UpdateDoraInfo(yaku, info.doraInfo.akadoraCount);
                    break;
                case Yaku.NukiDora:
                    // uiYakuPresets[index].UpdateDoraInfo(yaku, info.doraInfo.NukiDora);
                    break;
                case Yaku.UraDora:
                    uiYakuPresets[index].UpdateDoraInfo(yaku, info.doraInfo.uradoraCount);
                    break;
                default:
                    uiYakuPresets[index].UpdateInfo(yaku);
                    break;
            }
            uiYakuPresets[index].gameObject.SetActive(true);
            index++;
        }
        MyLogger.LogWarning(sb.ToString());
        // uiHanInt.text = info.Han.ToString();
        uiHanText.text = info.Han.ToString() + "판";
        // uiFuInt.text = info.Fu.ToString();
        uiFuText.text = info.Fu.ToString()+"부";
        uiTotalScore.text = isOya ? info.scoreTable.oyaRon.ToString() : info.scoreTable.zaRon.ToString();
        uiNyanganText.gameObject.SetActive(false);
        if((int)info.GetUniqueName >= 2000){
            uiNyanganText.text = info.GetUniqueName.ToString();
            uiNyanganText.gameObject.SetActive(true);

        }
        gameObject.SetActive(true);
    }

    void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
