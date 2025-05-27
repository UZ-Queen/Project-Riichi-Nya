using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
public class UiRemainingTimeIndicator : MonoBehaviour
{
    private Timer _timer;
    [SerializeField] private TextMeshProUGUI uiTimeIndicator;


    /// <summary>
    /// 타이머에 남은 시간을 보여줍니다.
    /// </summary>
    /// <param name="timer"></param>
    public void Construct(Timer timer)
    {
        _timer = timer;
        _timer.OnTimerFinished += OnTimerEnds;
        _timer.OnTimeTick += UpdateTimer;
        UpdateTimer(timer.RemainingSeconds);
    }

    void OnTimerEnds()
    {
        _timer.OnTimerFinished -= OnTimerEnds;
        _timer.OnTimeTick -= UpdateTimer;
    }
        void UpdateTimer(int t)
    {
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        uiTimeIndicator.text = $"{m:00}:{s:00}";
    }
}
