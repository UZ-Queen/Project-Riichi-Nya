using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManagerDistance : MonoBehaviour, IScoreDistanceService
{
    public event Action<int> OnBoostRankAlters = delegate { };
    public event Action<float> OnDistanceChange = delegate { };

    //기본 속도
    [SerializeField] private float baseSpeed = 1f;

    // 부스트 레벨당 오를 등반 속도 =  boostLv * speedBoostUnit
    [SerializeField] private float speedBoostUnit = 0.5f;
    // 부스트 감소 계수
    [SerializeField] private float boostDecayCoef = 0.0333f;
    [SerializeField] private float minDecayCoef = 0.01f;   // 레벨 0일 때 감쇠 계수
    [SerializeField] private float maxDecayCoef = 0.05f;   // 레벨 max일 때 감쇠 계수

    // 이 단위당 부스트 레벨 1
    [SerializeField] private float boostLevelStep = 1.0f;
    // [SerializeField] private float boostDecayPerLevel = 0.007f;

    [SerializeField] private int boostLevelMax = 10;

    // 이 수치를 매 프레임 누적된 거리에서 실제 거리에 더해줄 거임.
    [SerializeField] private float distanceMinUnit = 0.1f;
    public float DistanceWithAccumulated => Distance + accumulatedScore;
    public int BoostLevel => Mathf.Clamp((int)(boost / boostLevelStep), 0, boostLevelMax);
    public float FinalDistance => baseSpeed + speedBoostUnit * BoostLevel;


    public float Distance { get; private set; }
    static float ScoreToBoost(int score)
    {
        return score / 1900f;
    }
    static float ScoreToDistance(int score)
    {
        return score / 150f;
    }

    public float InterpolatedBoostValue
    {
        get
        {
            return Mathf.Clamp((boost - boostLevelStep * BoostLevel) / boostLevelStep, 0, 1);

        }
    }



    float accumulatedScore = 0;
    float boost = 0f;



    public void Initialize()
    {
        Distance = 0;
        accumulatedScore = 0;
        boost = 0;
        _isGameOver = false;
    }


    void Update()
    {
        if (_isGameOver) return;
        float dt = Time.deltaTime;

        DecayBoost(dt);
        GetDistance(dt);
    }

    void GetDistance(float deltaTime)
    {
        if (_isGameOver) return;
        accumulatedScore += (FinalDistance * deltaTime);


        if (accumulatedScore > distanceMinUnit)
        {

            Distance += distanceMinUnit;
            accumulatedScore -= distanceMinUnit;
            OnDistanceChange(Distance);
        }
    }

    void DecayBoost(float dt)
    {
        if (_isGameOver) return;
        float t = BoostLevel / (float)boostLevelMax;
        int beforeBoostRank = BoostLevel;

        // boost -= dt * BoostLevel * boostDecayCoef;
        // boost = boost * Mathf.Exp(-boostDecayCoef * dt);
        float decayCoef = Mathf.Lerp(minDecayCoef, maxDecayCoef, t);
        // 지수적 감쇠
        boost = Mathf.Max(0f, boost - decayCoef * dt);
        if (BoostLevel != beforeBoostRank)
        {
            OnBoostRankAlters(BoostLevel);
        }
    }

    public void GetBoostAndDistance(int score)
    {
        if (_isGameOver) return;
        // float amount = ScoreToBoost(score);
        GetBoost(ScoreToBoost(score));
        GetInstantDistance(ScoreToDistance(score));

    }



    public void GetBoost(float amount)
    {
        if (_isGameOver) return;

        int beforeBoostRank = BoostLevel;
        boost += amount;
        if (BoostLevel != beforeBoostRank)
        {
            OnBoostRankAlters(BoostLevel);
        }
    }
    public void GetInstantDistance(float amount)
    {
        if (_isGameOver) return;
        accumulatedScore += amount;
    }

    public void OnGameOver()
    {
        _isGameOver = true;
        // throw new NotImplementedException();
    }
    bool _isGameOver = false;
}
