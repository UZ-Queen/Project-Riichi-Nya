#define SHIGURE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEditor.Rendering;


public struct MahjongBlock : IComparable<MahjongBlock>
{
    public enum BlockType { Body, Head }
    public enum BodyType { Sequence, Triplet, Quad, NotABody }
    public enum OpenType { Concealed, Opened }




    public enum GotFrom { Ton, Nan, Sha, Pei, Self };

    public BlockType blockType;
    public BodyType bodyType;
    public OpenType openType;

    public MahjongTile.TileType tileType;


    public MahjongTile[] block { get; private set; }
    public MahjongTile this[int index]
    {
        get
        {
            //if (index < 0) index = 0;
            //if (index)
            return block[index]; // 생각해보니까 이거 깡이면 4개도 되잖아.. 
        }
    }
    public int startingNumber { get; private set; } // 몸통의 시작 수패의 숫자. 이페코나 삼색, 혼노두 등을 판단할 때 쓰인다.
    public bool Contains19 => block.Any(tile => (tile.number == 1 || tile.number == 9) && !tile.IsZapae);
    public bool ContainsZapae => block.Any(tile => tile.IsZapae);

    public bool Contains(MahjongTile targetTile)
    {
        return block.Any(tile => tile == targetTile);
    }


    public static MahjongBlock NewHead(MahjongTile iroha, MahjongTile hina)
    {
        return new MahjongBlock(new List<MahjongTile>() { iroha, hina });
    }

    public static MahjongBlock NewBody(MahjongTile yuuka, MahjongTile noa, MahjongTile koyuki)
    {
        return new MahjongBlock(new List<MahjongTile>() { yuuka, noa, koyuki });
    }

    public MahjongBlock(List<MahjongTile> tiles)
    {
        //block = new List<MahjongTile>(tiles);
        block = tiles.ToArray();

        //슌쯔 커쯔 깡쯔 판단

        if (block.Length == 2)
        {
            blockType = BlockType.Head;
            bodyType = BodyType.NotABody;
        }
        else if (block.Length == 3)
        {
            blockType = BlockType.Body;

            if (block[0] == block[1] && block[1] == block[2])
            {
                bodyType = BodyType.Triplet;
            }
            else
            {
                bodyType = BodyType.Sequence;
            }
        }
        else// if(block.Count == 4)
        {
            blockType = BlockType.Body;
            bodyType = BodyType.Quad;
        }

        //타일의 종류 판단

        tileType = block[0].tileType;
        startingNumber = block[0].number;


        // 후로 여부 판단

        openType = OpenType.Concealed;

    }
    public int CompareTo(MahjongBlock other)
    {
        if (block == null || other.block == null)
        {
            throw new ArgumentNullException("끄아앙!!");
        }
        return block[0].CompareTo(other.block[0]);
    }

    //public static bool WeakSamseakGak()

    public static bool operator ==(MahjongBlock momoi, MahjongBlock midori)
    {
        if (momoi.bodyType != midori.bodyType) return false;
        if (momoi[0] != midori[0]) return false;

        return true;
    }
    public static bool operator !=(MahjongBlock momoi, MahjongBlock midori)
    {
        return !(momoi == midori);
    }
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

}

public partial struct MahjongTile : IComparable<MahjongTile>
{
    //...왜?
    public bool IsZapae
    {
        get
        {
            return tileType >= TileType.Ton;
        }
    }


    public bool IsYoguPae
    {
        get
        {
            if (IsZapae) return true;
            if (number == 1 || number == 9) return true;
            return false;
        }
    }



    public bool IsMan
    {
        get
        {
            return tileType == TileType.Man;
        }
    }
    public bool IsPing
    {
        get
        {
            return tileType == TileType.Ping;
        }
    }
    public bool IsSou
    {
        get
        {
            return tileType == TileType.Sou;
        }
    }
    public bool IsTon
    {
        get
        {
            return tileType == TileType.Ton;
        }
    }
    public bool IsNan
    {
        get
        {
            return tileType == TileType.Nan;
        }
    }
    public bool IsSha
    {
        get
        {
            return tileType == TileType.Sha;
        }
    }
    public bool IsPei
    {
        get
        {
            return tileType == TileType.Pei;
        }
    }
    public bool IsHaku
    {
        get
        {
            return tileType == TileType.Haku;
        }
    }
    public bool IsHatsu
    {
        get
        {
            return tileType == TileType.Hatsu;
        }
    }
    public bool IsChun
    {
        get
        {
            return tileType == TileType.Chun;
        }
    }

}
public partial struct MahjongTile : IComparable<MahjongTile>
{
    public enum TileType
    {
        Man = 0, Ping = 10, Sou = 20,
        Ton = 30, Nan, Sha, Pei, Haku, Hatsu, Chun,
        MOLLU = 1557,
    }
    public TileType tileType;
    public int number;
    //동 = 30.
    public int TileID
    {
        get
        {
            return (int)this.tileType + number;
        }
    }

    // 몸통이 될 수 있는 다음 패의 TileID를 반환한다. 없다면 -1을 반환한다.
    // 예: IN 8통 -> OUT 9통 | IN 9삭 -> OUT -1
    public int NextConnectedTileID
    {
        get
        {
            if (TileID >= (int)TileType.Ton)
            {
                return -1;
            }
            else if (this.number == 9)
            {
                return -1;
            }
            else
            {
                return TileID + 1;
            }
        }
    }

    public bool isAkaDora;
    public bool isDora;
    public int doraCount;

    public void AddDora()
    {
        isDora = true;
        doraCount++;
    }

    public int DoraTileID
    {
        get
        {
            if (number == 9) return (int)tileType + 1;
            if (tileType == TileType.Pei) return (int)TileType.Ton;
            if (tileType == TileType.Chun) return (int)TileType.Haku;
            return TileID + 1;
        }
    }

    // 여기서부터 생성자임
    public MahjongTile(TileType tileType, int number = 0, bool isAkaDora = false)
    {
        this.tileType = tileType;

        if ((int)this.tileType >= (int)TileType.Ton)
        {
            number = 0;
        }
        else
        {
            //number = System.Math.Clamp(number, 1, 9);
            number = number < 1 ? 1 : number;
            number = number > 9 ? 9 : number;
        }
        this.number = number;

        this.isAkaDora = isAkaDora;
        this.isDora = false;
        doraCount = 0;
    }



    public static bool operator ==(MahjongTile a, MahjongTile b)
    {
        return a.TileID == b.TileID;
    }
    public static bool operator !=(MahjongTile a, MahjongTile b)
    {
        return !(a == b);
    }
    // public static bool operator +(MahjongTile a, int next)

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        //return base.GetHashCode();
        int hash = Utilities.HashCombine(TileID);
        return hash;
    }
    public override string ToString()
    {
        char c;
        int num;
        if (IsZapae)
        {
            c = 'z';
            num = TileID - (int)TileType.Ton + 1;
        }
        else
        {
            c = tileType.ToString().ToLower()[0];
            num = number;
            if (isAkaDora) num = 0;
        }
        return num.ToString() + c;

    }
    public readonly string ToChoboFriendlyString()
    {
        string tileName = "";

        // Dictionary<MahjongTile.TileType, string> 

        if (number != 0)
            tileName += number.ToString() + "";

        tileName = tileName + tileType.ToString();
        return tileName;

    }

    public int CompareTo(MahjongTile other)
    {
        return TileID.CompareTo(other.TileID);
    }
}


/// <summary>
/// 정적 메소드들
/// </summary>
public partial struct MahjongTile : IComparable<MahjongTile>
{
    public static MahjongTile WindToTile(Wind wind)
    {
        // MyLogger.Log($"Wind -> {wind.ToString()}({(int)wind})");
        string tileCode;

        switch (wind)
        {
            case Wind.Ton:
                tileCode = "1z";
                break;
            case Wind.Nan:
                tileCode = "2z";
                break;
            case Wind.Sha:
                tileCode = "3z";
                break;
            case Wind.Pei:
                tileCode = "4z";
                break;
            default:
                tileCode = "";
                break;
        }
        return MahjongTile.StringToTile(tileCode);
    }

    public static MahjongTile StringToTile(string input)
    {
        input = input.Trim();
        input = input.ToLower();



        int number;
        char type;
        TileType tileType = TileType.MOLLU;
        bool isAkadora = false;
        bool isZapae = false;

        string pattern = @"^(\d)([a-z])$";
        System.Text.RegularExpressions.Match match =
            System.Text.RegularExpressions.Regex.Match(input, pattern);
        if (!match.Success)
        {
            return NullTile();
        }

        if (!int.TryParse(match.Groups[1].Value, out number)) return NullTile();
        if (!char.TryParse(match.Groups[2].Value, out type)) return NullTile();

        switch (type)
        {
            case 'm':
                tileType = TileType.Man;
                break;
            case 'p':
                tileType = TileType.Ping;
                break;
            case 's':
                tileType = TileType.Sou;
                break;
            case 'z':
                tileType = TileType.Ton;
                isZapae = true;
                // 동: 30 이고 1z면 동, 2z면 남
                tileType += number - 1;
                number = 0;
                if (!Enum.IsDefined(typeof(TileType), tileType)) return NullTile();

                break;
            default:
                MyLogger.LogWarning($"올바르지 않은 형식입니다. 입력값: {input}");
                return NullTile();
        }

        if (number == 0 && !isZapae)
        {
            number = 5;
            isAkadora = true;
        }

        MahjongTile tile = new MahjongTile(tileType, number, isAkadora);

        //Logger.Log($"타일 생성 완료. input: {number}/{tileType.ToString()} 적도라: {isAkadora}");
        return tile;

    }
    public static List<MahjongTile> StringToTiles(string input)
    {
        List<MahjongTile> tiles = new List<MahjongTile>();
        input = input.Trim();
        if (input.Length == 0) return null;

        if (input.Length % 2 != 0)
        {
            MyLogger.LogWarning("입력된 문자열이 짝수가 아닙니다..");
            input = input.Substring(0, input.Length - 1);
        }
        var result = Enumerable.Range(0, input.Length / 2).Select(i => input.Substring(i * 2, 2));
        foreach (var i in result)
        {
            tiles.Add(StringToTile(i));
        }
        return tiles;
    }
    /*
     * 0~9: 만, 10~19: 통, 20~29: 삭, 30~:동
     * 8: 8만, 12: 2삭, 28: 8삭, 31: 남
     */
    public static MahjongTile TileIDToTile(int input, bool akadora = false)
    {
        MahjongTile newTile;
        if (input >= (int)TileType.Ton)
        {
            newTile = new MahjongTile((TileType)input);
        }
        else
        {
            newTile = new MahjongTile((TileType)(((int)input / 10) * 10), input % 10, akadora);
        }
        return newTile;
    }

    public static List<MahjongTile> GetAllTiles(bool includeAkadora = false)
    {
        List<MahjongTile> tiles = new List<MahjongTile>();
        for (int i = 0; i < 10; i++)
        {
            //30부터는 통남서북백발중 7개만 나옴.
            if (i > 2)
            {
                tiles.Add(TileIDToTile((int)TileType.Ton + i - 3));
                continue;
            }

            for (int k = 1; k <= 9; k++)
            {
                if(k == 5 && includeAkadora) tiles.Add(TileIDToTile(k + i * 10, true));
                tiles.Add(TileIDToTile(k + i * 10, false));
                
            }
        }
        return tiles;
    }
    // public static List<MahjongTile>
    public static List<MahjongTile> GetAllAkadoras()
    {
        List<MahjongTile> tiles = StringToTiles("0m0p0s");
        return tiles;
    }
    public static MahjongTile NullTile()
    {
        return new MahjongTile(TileType.MOLLU);
    }




}




