using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IScoreDistanceService
{
    public event Action<int> OnBoostRankAlters;
    public event Action<float> OnDistanceChange;
    public int BoostLevel { get; }
    public float Distance { get; }
    public float InterpolatedBoostValue { get; }

    //쓰게 메서드, 차후 IScoreDistanceWriter로 분리하자!
    public void GetBoostAndDistance(int score);
    public void GetBoost(float amount);
    public void GetInstantDistance(float amount);
    public void Initialize();
}
