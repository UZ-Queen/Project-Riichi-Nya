using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도라표시패, 현재 풍 정보를 보여줍니다.
/// 예: 도라: 3삭, 동국, 플레이어는 서 자리에 있음
/// </summary>
public class UiRoundInfo : MonoBehaviour
{
    [SerializeField] private MahjongTileGameObject uiDoraTile;
    [SerializeField] private MahjongTileGameObject uiRoundWind;
    [SerializeField] private TMPro.TextMeshProUGUI uiGuk;
    [SerializeField] private MahjongTileGameObject uiPlayerWind;


    public void UpdateUIInfo(MahjongRoundInfo info){

        MyLogger.Log("라운드 정보가 업데이트되었습니다..\n" + info.ToString());
        uiDoraTile.SetTile(info.doraTiles[0]);
        uiRoundWind.SetTile(MahjongTile.WindToTile(info.RoundWind));
        uiPlayerWind.SetTile(MahjongTile.WindToTile(info.playerWind));
        uiGuk.text = (info.guk % 4 + 1).ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
