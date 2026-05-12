using UnityEngine;
using TMPro;
using DG.Tweening;

public class BarrageItem : MonoBehaviour
{
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI msgText;

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }
}