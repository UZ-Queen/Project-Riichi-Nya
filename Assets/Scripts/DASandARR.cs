using System.Collections.Generic;
using UnityEngine;

public class DasArrInput
{
    // DAS(Delayed Auto Shift)–최초 반복까지의 지연 시간
    public float dasDelay = 0.133f;
    // ARR(Auto-Repeat Rate)–지연 후 반복 간격
    public float arrInterval = 0.017f;

    // 키별로 다음 반복이 가능한 시각을 저장
    private Dictionary<KeyCode, float> _nextRepeat = new Dictionary<KeyCode, float>();

    /// <summary>
    /// key에 대해 “눌렀다면 즉시 true, 누르고 있으면 DAS 이후 ARR 간격으로 true 반환
    /// </summary>
    public bool GetInput(KeyCode key)
    {
        // 처음 눌렀을 때
        if (Input.GetKeyDown(key))
        {
            // 다스딜레이 이후부터 연속 반복 가능.
            _nextRepeat[key] = Time.time + dasDelay;
            return true;
        }

        // 누르고 있는 동안
        if (Input.GetKey(key))
        {
            // 딕셔너리에 항목이 없으면 방금 눌렀을 때 빠졌을 수도 있으니까..
            if (!_nextRepeat.ContainsKey(key))
                _nextRepeat[key] = Time.time + dasDelay;

            // 현재 시각이 다스 딜레이 이후 시간이라면
            if (Time.time >= _nextRepeat[key])
            {
                // 다음 반복 시각을 ARR 간격만큼 미뤄줌
                _nextRepeat[key] = Time.time + arrInterval;
                return true;
            }
        }

        // 키를 뗐을 때는 초기화
        if (Input.GetKeyUp(key))
        {
            _nextRepeat.Remove(key);
        }

        return false;
    }
}
