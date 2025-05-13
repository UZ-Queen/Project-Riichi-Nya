using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public partial class Utilities
{
    //Special Characters
    public static class SC{
        static public string infinity {get{return "∞";}}
    }



    static public string[] indexToEnglishNumerals = {
    "Zero", "One", "Two", "Three", "Four", "Five",
    "Six", "Seven", "Eight", "Nine", "Ten", "Eleven",
    "Twelve", "Thirteen", "Fourteen", "Fifteen",
    "Sixteen", "Seventeen", "Eighteen", "Nineteen", "Twenty"
    };
    static public string[] indexToRomanNumerals = {
        "N", "I", "II", "III", "IV", "V",
        "VI", "VII", "VIII", "IX", "X",
        "XI", "XII", "XIII", "XIV", "XV",
        "XVI", "XVII", "XVIII", "XIX", "XX"
    };

    // static public 




    public static int HashCombine(params object[] values)
        {
            unchecked
            {
                int hash = 17;
                foreach(var value in values)
                {
                    hash = hash * 31 + (value?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
    static public T[] ShuffleArray<T>(T[] array, int seed)
    {
        System.Random prng = new System.Random(seed);
        for (int i = 0; i < array.Length - 1; i++)
        {
            Swap(ref array[i], ref array[prng.Next(i, array.Length - 1)]);
        }

        return array;
    }

    static public T GetRandomItem<T>(IList<T> array, int seed = 0){
        System.Random prng = seed == 0 ? new System.Random() : new System.Random(seed);
        if(array == null || array.Count == 0) throw new ArgumentException("콜렉션이 비어 있거나 널널했습니다.");
        int randomIndex = prng.Next(0, array.Count);
        return array[randomIndex];
    }
    static public void Swap<T>(ref T a, ref T b)
    {
        (b, a) = (a, b); //이런게 되나요?
    }
    static public UnityEngine.Vector3 GetReciprocalVector(UnityEngine.Vector3 vector, bool regardInfinityAsZero = false)
    {

        float infinityOrZero = regardInfinityAsZero ? float.PositiveInfinity : 0;

        float x = vector.x != 0 ? 1 / vector.x : infinityOrZero;
        float y = vector.y != 0 ? 1 / vector.y : infinityOrZero;
        float z = vector.z != 0 ? 1 / vector.z : infinityOrZero;

        return new UnityEngine.Vector3(x, y, z);
    }
}


public struct Coord
{

    public int x;
    public int y;
    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;

    }

    public static bool operator ==(Coord a, Coord b)
    {
        if (a.x == b.x && a.y == b.y)
            return true;
        else
            return false;
    }

    public static bool operator !=(Coord a, Coord b)
    {
        return !(a == b);
    }
    public static Coord operator +(Coord a, Coord b)
    {
        return new Coord(a.x + b.x, a.y+b.y);
    }
    public static Coord operator -(Coord a)
    {
        return new Coord(-a.x,-a.y);
    }
    public static Coord operator -(Coord a, Coord b){
        return a+(-b);
    }
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    // public Coord(Vector2Int vector){
    //     Coord(vector.x, vector.y);
    // }

}