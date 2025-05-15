
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerHand : MonoBehaviour
{
    [SerializeField] private MahjongTileGameObject tilePrefap;
    [SerializeField] private GameObject handTilesHolder;
    [SerializeField] private GameObject tsumoTileHolder;

    [SerializeField] private GameObject RiichiNya;
    [SerializeField] private GameObject TsumoNya;

    public int currentIndex;
    public MahjongTileGameObject[] tilesInHand;
    public MahjongTileGameObject tileTsumo;
    public bool hasTsumoTileInHand = false;




    bool callRiichiNya = false;
    void Start()
    {
        tilesInHand = new MahjongTileGameObject[13];
        currentIndex = 6;
        InitialzeHand();
        UpdateHand();
    }

    void UpdateHand(){
        for(int i=0; i<13; i++){
            bool doSelect = false;
            if(i == currentIndex) doSelect = true;
            tilesInHand[i].SetSelected(doSelect);
        }
    }

    void InitialzeHand(){
        List<MahjongTile> list = MahjongTile.StringToTiles("1m1m1m2m3m4m0m6m7m8m9m9m9m");
        int index = 0;
        foreach(var i in list){
            // MahjongTileGameObject newTile = Instantiate(tilePrefap, tilePrefap.transform.position, tilePrefap.transform.rotation, handTilesHolder.transform);
            MahjongTileGameObject newTile = Instantiate(tilePrefap, handTilesHolder.transform);

            newTile.SetTileImage(i);
            newTile.SetDora(i.isDora || i.isAkaDora);
            newTile.enabled = false;
            tilesInHand[index] = newTile;
            index++;
        }
        tileTsumo = Instantiate(tilePrefap, tsumoTileHolder.transform);
        tileTsumo.SetTileImage(MahjongTile.StringToTile("1m"));
        tileTsumo.enabled = false;

        RiichiNya.SetActive(false);
        TsumoNya.SetActive(false);

    }

    //데이터 조작 가능성이 있을 것 같은데... 일단 패스.
    //또한, 가능하다면 계속 Instantiate를 하지 말고 풀링했으면 좋겠다. 이건 욕심이니 나중에.
    public void FillHand(List<MahjongTile> tiles){
        int index = 0;
        foreach(var i in tiles){
            tilesInHand[index].SetTileImage(i);
            tilesInHand[index].SetDora(i.isDora || i.isAkaDora);
            tilesInHand[index].enabled = false;
            index++;
        }
        foreach(var tileInstance in tilesInHand){
            tileInstance.enabled = true;
        }
    }
    // public void TsumoTile(MahjongTile tile){
    //     tileTsumo.SetTile(tile);
    //     tileTsumo.enabled = true;

    // }

    public void TsumoTile(TsumoInfo tsumoInfo){
        tileTsumo.SetTile(tsumoInfo.tsumoTile);
        tileTsumo.enabled = true;
        ShowRiichiNyaButtons(tsumoInfo);


    }

    void ShowRiichiNyaButtons(TsumoInfo tsumoInfo){
        // MyLogger.Log($"리치냐를 킬까요? {tsumoInfo.isRiichiAble}/{tsumoInfo.isTsumoAble}");
        RiichiNya.SetActive(tsumoInfo.isRiichiAble);
        TsumoNya.SetActive(tsumoInfo.isTsumoAble);
        // MyLogger.Log("업데이트했습니다!");
    }
    void HideRiichiNyaButtons(){
        RiichiNya.SetActive(false);
        TsumoNya.SetActive(false);
        // MyLogger.Log("패를 버렸으니 리치냐 버튼을 가릴게요.");
    }




    
    void Update()
    {
        // float hInput = Input.GetAxisRaw("Horizontal");
        if(Input.GetKeyDown(InputPreset.left)){
            // MoveHand(-1);
            MoveHandToLeft();
        }
        if(Input.GetKeyDown(InputPreset.right)){
            // MoveHand(1);
            MoveHandToRight();
        }
        if(Input.GetKeyDown(InputPreset.discard)){
            DiscardSelectedTile();
        }
        else if(Input.GetKeyDown(InputPreset.discardTsumoTile)){
            DiscardTsumoTile();
        }


        if(Input.GetKeyDown(InputPreset.riichi)){
            callRiichiNya = true;
        }
        else if(Input.GetKeyDown(InputPreset.tsumoAgari)){
            OnPlayerCall(PlayerCallType.Tsumo);
        }

        
    }

    /// <summary>
    /// 새로운 Index값으로 변경.(clamp 해줌 걱정 ㄴㄴ)
    /// </summary>
    /// <param name="value"></param>
    void MoveHand(int newIndex){
        int beforeValue = currentIndex;
        currentIndex =  UnityEngine.Mathf.Clamp(newIndex, 0, 12);
        if(beforeValue != currentIndex){
            UpdateHand();
        }
    }

    void MoveHandToLeft(){
        MoveHand(currentIndex - 1);
    }
    void MoveHandToRight(){
        MoveHand(currentIndex +1);

    }
    public event Action<int> OnPlayerDiscard = delegate{};
    public event Action<PlayerCallType> OnPlayerCall = delegate{};

    void DiscardSelectedTile(){
        HideRiichiNyaButtons();
        OnPlayerDiscard(currentIndex);
        
    }
    void DiscardTsumoTile(){
        HideRiichiNyaButtons();
        OnPlayerDiscard(13);
    }
    // public event Action OnPlayerCallRiichi = delegate{};
    // public event Action OnPlayerCallTsumoAgari = delegate{};

}
