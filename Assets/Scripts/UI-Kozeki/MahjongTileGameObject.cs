using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Drawing;

public class MahjongTileGameObject : MonoBehaviour
{
    public int index;
    [SerializeField] private bool isDora = false;
    [SerializeField] private bool isSelected = false;
    [SerializeField] private Image tileImage;

    [SerializeField] private RectTransform tileTrasform;
    [SerializeField] private GameObject tileDehighligher;
    [SerializeField] private TextMeshProUGUI uiDoraIndicator;

    [SerializeField] private bool deHighlightTile = false;

    public float liftAmount = 35f;

    // [SerializeField] float moveValue = 

    public void SetDora(bool isDora)
    {
        this.isDora = isDora;
        uiDoraIndicator.gameObject.SetActive(isDora);
    }
    public void SetTileImage(Sprite sprite)
    {
        tileImage.sprite = sprite;
    }
    public void SetTileImage(MahjongTile tile)
    {
        //DB에서 이미지를 가져온다.
        //tileImage.sprite에 가져온 이미지를 할당한다.
        tileImage.sprite = MahjongGameManager.Instance?.TileDB.GetImage(tile);

    }
    public void SetTile(MahjongTile tile)
    {
        SetTileImage(tile);
        SetDora(tile.isDora || tile.isAkaDora);
    }

    public void ToggleSelected()
    {
        isSelected ^= true;
    }
    public void SetSelected(bool value)
    {
        isSelected = value;
    }
    public void SetDeHighlight(bool value)
    {
        tileDehighligher.SetActive(value);
    }

    void Update()
    {
        if (isSelected)
        {
            tileTrasform.localPosition = Vector2.up * liftAmount;

        }
        else
        {
            tileTrasform.localPosition = Vector2.zero;
        }

    }

    void OnEnable()
    {
        uiDoraIndicator.gameObject.SetActive(isDora);
    }

}
