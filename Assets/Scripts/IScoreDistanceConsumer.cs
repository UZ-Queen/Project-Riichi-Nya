using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public interface IScoreDistanceConsumer
{
    void Construct(IScoreDistanceService svc);
    // void OnBoostRankAlters(int newRank);
    // void OnDistanceChange(float newDistance);
}
