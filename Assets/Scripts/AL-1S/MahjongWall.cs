using System;
using System.Collections.Generic;

/// <summary>
/// 네 장씩 34종과 적5 세 장으로 구성된 공유 패산을 생성합니다.
/// </summary>
public static class MahjongWall
{
    /// <summary>
    /// 전달받은 난수 생성기로 섞은 새 136장 패산을 반환합니다.
    /// </summary>
    public static MahjongTile[] CreateShuffled(Random random)
    {
        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var tiles = new List<MahjongTile>(136);
        foreach (MahjongTile tile in MahjongTile.GetAllTiles())
        {
            for (int copyIndex = 0; copyIndex < 4; copyIndex++)
            {
                bool isRedFive = !tile.IsZapae && tile.number == 5 && copyIndex == 0;
                tiles.Add(new MahjongTile(tile.tileType, tile.number, isRedFive));
            }
        }

        return Utilities.ShuffleArray(tiles.ToArray(), random);
    }
}
