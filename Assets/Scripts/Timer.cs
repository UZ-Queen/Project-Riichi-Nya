using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public event Action<int> OnTimeTick = delegate { };
    public event Action OnTimerFinished = delegate { };

    public float RemainingTime { get; private set; } = 0;
    public int RemainingSeconds => Mathf.FloorToInt(RemainingTime);
    private bool isRunning = false;

    public void StartTimer(float time)
    {
        RemainingTime = time;
        isRunning = true;
        _paused = false;
    }
    public void AddTime(float time)
    {
        if (!isRunning) return;
        CheckTimerTick(-time);
    }
    private bool _paused = false;
    public void TaimuSutopu()
    {
        _paused = true;
    }
    public void Resume()
    {
        _paused = false;
    }

    void Update()
    {
        if (!isRunning) return;
        if (_paused) return;
        CheckTimerTick(Time.deltaTime);

        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            isRunning = false;
            OnTimerFinished();
        }
    }
    /// <summary>
    /// dt만큼 타이머를 감소시키고 초가 바뀌었는지 확인한다.(증가도 가능)
    /// </summary>
    /// <param name="dt"></param>
    void CheckTimerTick(float dt)
    {
        int lastTimeSecond = RemainingSeconds;
        RemainingTime -= dt;

        if (lastTimeSecond != RemainingSeconds)
        {
            OnTimeTick(RemainingSeconds);
        }
    }
}
