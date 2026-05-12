using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BarrageManager : MonoBehaviour
{
    [Header("UI 引用")]
    public RectTransform container;      // 弹幕容器
    public BarrageItem itemPrefab;       // 弹幕预制体

    public enum ScrollDirection
    {
        BottomToTop,
        TopToBottom
    }

    [Header("排版参数")]
    public ScrollDirection scrollDirection = ScrollDirection.BottomToTop; // 滚动方向
    public int maxCount = 8;             // 最大容纳数量
    public float itemHeight = 60f;       // 单条弹幕的高度
    public float spacing = 10f;          // 弹幕间距
    public float animDuration = 0.3f;    // 动画时长

    [Header("数据源")]
    public TextAsset textData;           // 预设档案 (TXT)
    private string[] presetMessages;

    [Header("自动触发设置")]
    [Tooltip("普通弹幕触发按钮（把触发评论的按钮拖到这里）")]
    public Button normalBarrageButton;

    [Tooltip("是否启用普通弹幕自动触发")]
    public bool enableAutoTrigger = true;

    [Tooltip("普通弹幕触发间隔（秒）")]
    public float autoTriggerInterval = 1f;

    private float autoTriggerTimer = 0f;

    // 活跃队列与对象池
    private Queue<BarrageItem> activeItems = new Queue<BarrageItem>();
    private Queue<BarrageItem> itemPool = new Queue<BarrageItem>();

    private void Start()
    {
        // 1. 解析文本文档（按行分割）
        if (textData != null)
        {
            presetMessages = textData.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
    }

    private void Update()
    {
        if (!enableAutoTrigger || normalBarrageButton == null) return;

        autoTriggerTimer += Time.deltaTime;
        if (autoTriggerTimer >= autoTriggerInterval)
        {
            autoTriggerTimer = 0f;
            // 自动点击按钮
            normalBarrageButton.onClick.Invoke();
        }
    }

    // 提供给外部的钩子函数，用于触发新弹幕
    public void TriggerRandomBarrage()
    {
        if (presetMessages == null || presetMessages.Length == 0) return;
        
        string randomMsg = presetMessages[Random.Range(0, presetMessages.Length)];
        ShowNewBarrage(randomMsg);
    }

    private void ShowNewBarrage(string msg)
    {
        float stepOffset = itemHeight + spacing;
        float dirSign = scrollDirection == ScrollDirection.BottomToTop ? 1f : -1f;

        // 1. 如果超出容量，处理最老（最远端）的弹幕
        if (activeItems.Count >= maxCount)
        {
            BarrageItem oldestItem = activeItems.Dequeue();
            
            // 向外滑出并渐隐，完成后压入对象池回收
            oldestItem.rectTransform.DOAnchorPosY(oldestItem.rectTransform.anchoredPosition.y + stepOffset * dirSign, animDuration);
            oldestItem.canvasGroup.DOFade(0, animDuration).OnComplete(() =>
            {
                oldestItem.gameObject.SetActive(false);
                itemPool.Enqueue(oldestItem);
            });
        }

        // 2. 遍历当前队列中的所有弹幕，全部向指定方向移动一格
        foreach (var item in activeItems)
        {
            float targetY = item.rectTransform.anchoredPosition.y + stepOffset * dirSign;
            item.rectTransform.DOAnchorPosY(targetY, animDuration).SetEase(Ease.OutQuad);
        }

        // 3. 从对象池获取新弹幕，插入起始端
        BarrageItem newItem = GetItemFromPool();
        newItem.msgText.text = msg;

        // 动态根据方向调整锚点和中心点，这样就不用手动去改 prefab 了
        float anchorY = scrollDirection == ScrollDirection.BottomToTop ? 0f : 1f;
        float h = newItem.rectTransform.rect.height;
        
        newItem.rectTransform.anchorMin = new Vector2(newItem.rectTransform.anchorMin.x, anchorY);
        newItem.rectTransform.anchorMax = new Vector2(newItem.rectTransform.anchorMax.x, anchorY);
        newItem.rectTransform.pivot = new Vector2(newItem.rectTransform.pivot.x, anchorY);
        newItem.rectTransform.sizeDelta = new Vector2(newItem.rectTransform.sizeDelta.x, h);
        
        // 初始状态：位置在起始端 (Y=0)，全透明 (Alpha=0)
        newItem.rectTransform.anchoredPosition = new Vector2(0, 0);
        newItem.canvasGroup.alpha = 0;
        newItem.gameObject.SetActive(true);

        // 渐显动画
        newItem.canvasGroup.DOFade(1, animDuration).SetEase(Ease.OutQuad);

        // 加入活跃队列
        activeItems.Enqueue(newItem);
    }

    // 对象池获取逻辑
    private BarrageItem GetItemFromPool()
    {
        if (itemPool.Count > 0)
        {
            return itemPool.Dequeue();
        }
        else
        {
            // 池子空了则实例化新的，挂载到容器下
            BarrageItem newItem = Instantiate(itemPrefab, container);
            return newItem;
        }
    }
}