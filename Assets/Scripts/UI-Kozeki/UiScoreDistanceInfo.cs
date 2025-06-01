using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEditor;
public class UiScoreDistanceInfo : MonoBehaviour, IScoreDistanceConsumer
{
    IScoreDistanceService _scv;
    // 각 랭크별로 1~2, 2~3 사이의 색깔임.
    [SerializeField] private List<Color> colors;
    [SerializeField] private float gaugeInterpolateDuration = 0.25f;

    [SerializeField] private float gaugeUpdatePeriod = 0.25f;
    [SerializeField] private float textUpdatePeriod = 0.1f;

    // int currentLevel = 0;
    float nextGaugeUpdateTime = 0;
    // float nextTextUpdateTime = 0;
    Color currentLevelColor = Color.white;
    Color nextLevelColor = Color.white;

    [SerializeField] private RectTransform currentLevelGauge;
    [SerializeField] private  RectTransform nextLevelGauge;

    [SerializeField] private  TextMeshProUGUI uiDistance;
    [SerializeField] private  TextMeshProUGUI uiLevelIndicator;


    bool _isInitialized = false;
    
    void Initialize()
    {
        nextGaugeUpdateTime = 0;
        // nextTextUpdateTime = 0;
        // currentLevel = 0;
        ChangeColor(0);

        // currentLevelColor = colors[0];
        // nextLevelColor = colors[1];
        UpdateGauge();
    }

    
    public void Construct(IScoreDistanceService svc)
    {
        // _scv.GetBoostAndDistance(1);
        _scv = svc;
        if (isActiveAndEnabled)
        {
            svc.OnBoostRankAlters += OnBoostRankAlters;
            svc.OnDistanceChange += UpdateDistance;
            Initialize();
        }
    }
    void OnDisable()
    {
        _scv.OnBoostRankAlters -= OnBoostRankAlters;
        _scv.OnDistanceChange -= UpdateDistance;
    }
    void OnEnable()
    {
        if (_scv == null)
        {
            MyLogger.LogWarning("서비스가 아직 없습니다.");
            return;
        }

        _scv.OnBoostRankAlters += OnBoostRankAlters;
        _scv.OnDistanceChange += UpdateDistance;
        Initialize();
    }

    //색깔을 바꾸고 스케일 값을 초기화한다.
    void OnBoostRankAlters(int newRank)
    {
        
        currentLevelGauge.localScale = new Vector3(1, 1, 1);
        ChangeColor(newRank);
        uiLevelIndicator.text = newRank.ToString();
    }
    /// <summary>
    /// 점수가 업데이트된 경우만 업데이트한다. 성능이 딸린다면 업데이트 주기도 바꿔주자.
    /// </summary>
    void UpdateDistance(float newDistance)
    {
        // if (Time.time < nextTextUpdateTime) return;

        uiDistance.text = _scv.Distance.ToString();
    }
    Tween _currentGaugeTween;
    /// <summary>
    /// CurrentLevel Gauge의 스케일을 1->0으로 줄이면 양옆에서 다음 레벨의 게이지가 올라오는 것처럼 보이겠지.
    /// </summary>
    void UpdateGauge()
    {
        if (nextGaugeUpdateTime > Time.time) return;
        // currentLevelGauge.localScale = new Vector3(_scv.InterpolatedBoostValue, 1, 1);
        if (_currentGaugeTween != null && _currentGaugeTween.IsActive())
        {
            _currentGaugeTween.Kill();
        }
        _currentGaugeTween = currentLevelGauge.DOScaleX(1 - _scv.InterpolatedBoostValue, gaugeInterpolateDuration).SetEase(Ease.OutBack);
        nextGaugeUpdateTime = Time.time + gaugeUpdatePeriod;

    }





    void ChangeColor(int newLevel)
    {
        // if (colors.Count < newLevel + 1)
        // {
        //     MyLogger.LogWarning("다음 색깔이 없습니다!");
            newLevel = Mathf.Clamp(newLevel, 0, colors.Count - 2);
        // }
        currentLevelColor = colors[newLevel];
        nextLevelColor = colors[newLevel + 1];

        currentLevelGauge.GetComponent<Image>().color = currentLevelColor;
        nextLevelGauge.GetComponent<Image>().color = nextLevelColor;


    }



    // Update is called once per frame
    void Update()
    {
        if (_scv == null) return;
        UpdateGauge();
    }


}
