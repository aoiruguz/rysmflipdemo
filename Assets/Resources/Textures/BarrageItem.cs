using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BarrageItem : MonoBehaviour
{
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI msgText;
    public TextMeshProUGUI nameText;

    [Header("Spatial 特殊效果")]
    public GameObject giftObj;
    public Image giftImage;
    public Image giftDigit1;
    public Image giftDigit2;
    public GameObject moneyObj;
    public Image moneyDigit1;
    public Image moneyDigit2;

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetAsNormal()
    {
        if (giftObj != null) giftObj.SetActive(false);
        if (moneyObj != null) moneyObj.SetActive(false);
    }

    public void SetAsGift(Sprite giftSprite, Sprite digit1, Sprite digit2)
    {
        if (giftObj != null) giftObj.SetActive(true);
        if (giftImage != null && giftSprite != null)
        {
            giftImage.sprite = giftSprite;
            giftImage.SetNativeSize();
        }
        if (giftDigit1 != null && digit1 != null)
        {
            giftDigit1.sprite = digit1;
            giftDigit1.SetNativeSize();
        }
        if (giftDigit2 != null && digit2 != null)
        {
            giftDigit2.sprite = digit2;
            giftDigit2.SetNativeSize();
        }
        if (moneyObj != null) moneyObj.SetActive(false);
    }

    public void SetAsMoney(Sprite digit1, Sprite digit2)
    {
        if (giftObj != null) giftObj.SetActive(false);
        if (moneyObj != null) 
        {
            moneyObj.SetActive(true);
            if (moneyDigit1 != null && digit1 != null)
            {
                moneyDigit1.sprite = digit1;
                moneyDigit1.SetNativeSize();
            }
            if (moneyDigit2 != null && digit2 != null)
            {
                moneyDigit2.sprite = digit2;
                moneyDigit2.SetNativeSize();
            }
        }
    }
}