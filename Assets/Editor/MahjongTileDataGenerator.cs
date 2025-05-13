#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
public class MahjongTileDataGenerator : EditorWindow
{
    [MenuItem("Datsui_Mahjong/MahjongTileDataGenerator")]
    private static void ShowWindow()
    {
        var window = GetWindow<MahjongTileDataGenerator>();
        window.titleContent = new GUIContent("MahjongTileDataGenerator");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("AL-1S: 마작 타일 데이터베이스 자동 생성 모드", EditorStyles.boldLabel);

        if (GUILayout.Button("타일 DB를 생성할까요?"))
        {
            GenerateTileDatabaseWithConfirmation();
        }
    }
    private static void GenerateTileDatabaseWithConfirmation()
    {
        string assetPath = "Assets/ScriptableObjects/MahjongTileDatabase.asset";

        bool exists = File.Exists(assetPath);
        if (exists)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "끄앙!",
                "기존 타일 데이터베이스가 존재해요. 덮어쓰실래요?",
                "ㅇㅇ", "ㄴㄴ");

            if (!overwrite) return;
        }

        GenerateTileDB(assetPath);
    }



    // [MenuItem("Tools/Mahjong/Create Tile Database")]
    public static void GenerateTileDB(string pathToSave)
    {
        List<MahjongTileDatabase.MahjongTileData> tileData = new List<MahjongTileDatabase.MahjongTileData>();
        // var tileData = new Dictionary<string, MahjongTileDatabase.MahjongTileData>();

        List<MahjongTile> allTiles = MahjongTile.GetAllTiles();
        allTiles.AddRange(MahjongTile.GetAllAkadoras());
        string spriteRelativeDirectory = @"Sprites/MahjongTiles";
        int count = 0;

        foreach (var tile in allTiles)
        {
            string fileName = tile.ToString() + ".png";
            MahjongTileDatabase.MahjongTileData newData = new MahjongTileDatabase.MahjongTileData(tile);
            if (File.Exists(Path.Combine(Application.dataPath, spriteRelativeDirectory, fileName)))
            {
                
                string realImagePath = Path.Combine("Assets",spriteRelativeDirectory,fileName);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(realImagePath);
                // Debug.Log($"이미지 로드 시도 중.. {spriteRelativeDirectory + '/' + fileName}");
                Debug.Log($"이미지 로드 시도 중.. {realImagePath}");
                if (sprite == null)
                {
                    Debug.LogError("로드 실패..");
                }
                else
                {
                    Debug.Log("로드 성공!");
                    newData.sprite = sprite;
                    count++;
                }
                // Debug.Log(sprite == null ? "로드 실패.." : "이미지 로드 성공"); // <-- Tlqkf 이거 왜 안됨??
            }
            else
            {
                Debug.LogError($"마작 타일 이미지를 불러오지 못했습니다. 경로를 확인해주세요\n{Path.Combine(Application.dataPath, spriteRelativeDirectory, fileName)}"); ;
            }
            // tileData.Add(tile.ToString() ,newData);
            tileData.Add(newData);
        }

        // return tileData;

        // string pathToSave = "Assets/ScriptableObjects/MahjongTileDatabase.asset";
        var existing = AssetDatabase.LoadAssetAtPath<MahjongTileDatabase>(pathToSave);
        if (existing != null)
        {
            // existing.tileAssets = tileData;
            existing.SetTileAssets(tileData);
            EditorUtility.SetDirty(existing);
        }
        else
        {
            var db = ScriptableObject.CreateInstance<MahjongTileDatabase>();
            // db.tileAssets = tileData;
            db.SetTileAssets(tileData);
            AssetDatabase.CreateAsset(db, pathToSave);
        }


        AssetDatabase.SaveAssets();
        Debug.Log($"마작 타일 DB 생성 완료! 불러온 이미지: {count}/37");
    }
}
#endif