using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public static class UITransitions
{
    /// <summary>
    /// RectTransform을 (startPos)에서 시작해 dir 방향으로 슬라이드 아웃
    /// </summary>
    public static Sequence SlideOutAndFade(this RectTransform rt,
                                          CanvasGroup cg,
                                          Vector2 direction,
                                          float distance,
                                          float duration,
                                          Ease ease = Ease.InOutCubic)
    {
        Vector2 offset = direction.normalized * distance;

        var seq = DOTween.Sequence();
        seq.Join(rt.DOAnchorPos(rt.anchoredPosition + offset, duration)
                   .SetEase(ease));

        seq.Join(cg.DOFade(0f, duration)
                   .SetEase(ease));

        return seq;
    }

    /// <summary>
    /// RectTransform을 dir 반대 방향(startPos + offset)에서 시작해
    /// 원래 위치로 슬라이드 인
    /// </summary>
    public static Sequence SlideInAndFade(this RectTransform rt,
                                          CanvasGroup cg,
                                          Vector2 direction,
                                          float distance,
                                          float duration,
                                          Ease ease = Ease.InOutCubic)
    {
        Vector2 offset = direction.normalized * distance;

        Vector2 original = rt.anchoredPosition;

        // 시작 위치로 강제 이동(화면 밖)
        rt.anchoredPosition = original + offset;
        cg.alpha = 0f;

        
        var seq = DOTween.Sequence();

        seq.Join(rt.DOAnchorPos(original, duration)
                   .SetEase(ease));
        seq.Join(cg.DOFade(1f, duration)
                   .SetEase(ease));

        return seq;
    }
}
