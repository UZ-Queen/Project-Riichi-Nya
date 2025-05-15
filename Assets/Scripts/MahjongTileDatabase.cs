using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

[CreateAssetMenu(fileName ="타일 DB", menuName = "마작/DB")]
public class MahjongTileDatabase : ScriptableObject
{

    [System.Serializable]
    public class TileImageSomething{
        
        public string tileName;
        // public MahjongTileData tileData;
        public Sprite tileSprite;

    }

    [System.Serializable]
    public struct MahjongTileData{
        public MahjongTile.TileType tileType;
        public int number;
        public bool isAkaDora;
        public Sprite sprite;

        public string code;

    public MahjongTileData(MahjongTile tile){
            tileType = tile.tileType;
            number = tile.number;
            isAkaDora = tile.isAkaDora;
            sprite = null;
            code = tile.ToString();
    }



    }
    [SerializeField] private  Sprite front;
    [SerializeField] private Sprite back;
    [SerializeField] private  Sprite fallBack;
    [SerializeField] private  Dictionary<string, MahjongTileData> tileAssets;
    [SerializeField] private  List<MahjongTileData> greatAssets;


    void OnEnable()
    {
        tileAssets = greatAssets.ToDictionary(target=>target.code, target=>target);
    }

    public Sprite GetImage(MahjongTile tile){
        Sprite sprite = null;
        MahjongTileData data;
        if( tileAssets.TryGetValue(tile.ToString(), out data)){
            sprite = data.sprite;
        }
        
        if(sprite == null){
            MyLogger.LogError($"타일DB: {tile.ToString()} 타일 스프라이트를 찾지 못했습니다!");
            sprite = fallBack;
        }
        return sprite;
    }

    #if UNITY_EDITOR
    public void SetTileAssets(Dictionary<string, MahjongTileData> newTileAssets){
        tileAssets = newTileAssets;
    }
        public void SetTileAssets(List<MahjongTileData> newTileAssets){
        greatAssets = newTileAssets;
    }
    #endif



    

}
