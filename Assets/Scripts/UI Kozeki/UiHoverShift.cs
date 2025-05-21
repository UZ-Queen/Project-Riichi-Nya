
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class UiHoverShift : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float offsetX = -20f;
    [SerializeField] private float offsetY = 0f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Ease ease = Ease.InOutCubic;

    RectTransform rt;
    Vector2 originalPos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        originalPos = rt.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MoveTo(originalPos + Vector2.right * offsetX + Vector2.up * offsetY);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MoveTo(originalPos);
    }

    void MoveTo(Vector2 target)
    {
        rt.DOKill();
        rt.DOAnchorPos(target, duration).SetEase(ease);
    }
}
