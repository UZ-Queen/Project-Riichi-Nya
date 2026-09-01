using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 솔로 손패 타일과 선택 및 선언 가능 표시를 렌더링합니다.
/// </summary>
public class PlayerHandView : MonoBehaviour
{
    [SerializeField] private MahjongTileGameObject tilePrefap;
    [SerializeField] private GameObject handTilesHolder;
    [SerializeField] private GameObject tsumoTileHolder;
    [SerializeField] private GameObject RiichiNya;
    [SerializeField] private GameObject TsumoNya;

    private MahjongTileGameObject[] tilesInHand;
    private MahjongTileGameObject tileTsumo;

    void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// 기존 손패 계층에 열세 장과 쯔모패 표시 객체를 한 번 생성합니다.
    /// </summary>
    public void Initialize()
    {
        if (tilesInHand != null)
        {
            return;
        }

        tilesInHand = new MahjongTileGameObject[13];
        List<MahjongTile> initialTiles = MahjongTile.StringToTiles("1m1m1m2m3m4m0m6m7m8m9m9m9m");
        for (int i = 0; i < initialTiles.Count; i++)
        {
            MahjongTileGameObject newTile = Instantiate(tilePrefap, handTilesHolder.transform);
            MahjongTile tile = initialTiles[i];
            newTile.SetTileImage(tile);
            newTile.SetDora(tile.isDora || tile.isAkaDora);
            newTile.enabled = false;
            tilesInHand[i] = newTile;
        }

        tileTsumo = Instantiate(tilePrefap, tsumoTileHolder.transform);
        tileTsumo.SetTileImage(MahjongTile.StringToTile("1m"));
        tileTsumo.enabled = false;
        HideActionButtons();
    }

    /// <summary>
    /// 열세 장의 손패를 기존 타일 객체에 표시합니다.
    /// </summary>
    /// <param name="tiles">표시할 손패입니다.</param>
    public void FillHand(List<MahjongTile> tiles)
    {
        int index = 0;
        foreach (MahjongTile tile in tiles)
        {
            if (index >= tilesInHand.Length)
            {
                StringBuilder message = new StringBuilder();
                foreach (MahjongTile handTile in tiles)
                {
                    message.Append($"[{handTile.ToChoboFriendlyString()}]");
                }

                MyLogger.LogError($"손패 길이: {tiles.Count}해당 손패예요. 13개가 아닌지 확인해보세요.\n{message}");
            }

            tilesInHand[index].SetTileImage(tile);
            tilesInHand[index].SetDora(tile.isDora || tile.isAkaDora);
            tilesInHand[index].enabled = false;
            index++;
        }

        foreach (MahjongTileGameObject tileInstance in tilesInHand)
        {
            tileInstance.enabled = true;
        }
    }

    /// <summary>
    /// 쯔모패와 리치 및 쯔모 가능 표시를 갱신합니다.
    /// </summary>
    /// <param name="tsumoInfo">쯔모패와 선언 가능 정보입니다.</param>
    public void TsumoTile(TsumoInfo tsumoInfo)
    {
        tileTsumo.SetTile(tsumoInfo.tsumoTile);
        tileTsumo.enabled = true;
        RiichiNya.SetActive(tsumoInfo.isRiichiAble);
        TsumoNya.SetActive(tsumoInfo.isTsumoAble);
    }

    /// <summary>
    /// 현재 선택한 손패만 강조합니다.
    /// </summary>
    /// <param name="selectedIndex">강조할 손패 인덱스입니다.</param>
    public void UpdateSelectedIndex(int selectedIndex)
    {
        for (int i = 0; i < tilesInHand.Length; i++)
        {
            tilesInHand[i].SetSelected(i == selectedIndex);
        }
    }

    /// <summary>
    /// 리치 및 쯔모 가능 표시를 숨깁니다.
    /// </summary>
    public void HideActionButtons()
    {
        RiichiNya.SetActive(false);
        TsumoNya.SetActive(false);
    }
}
