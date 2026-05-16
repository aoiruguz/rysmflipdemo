using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 菜单键盘导航系统
/// 功能：
/// 1. 使用上下键在按钮之间切换选择
/// 2. 按Enter键确认选中的按钮
/// 3. 浮标跟随当前选中的按钮
/// </summary>
public class MenuKeyboardNavigation : MonoBehaviour
{
    [Header("Navigation Settings")]
    [Tooltip("所有可导航的按钮（按从上到下的顺序排列）")]
    public Button[] buttons;

    [Tooltip("浮标对象（会自动移动到选中按钮的左侧）")]
    public RectTransform floatingIndicator;

    [Tooltip("浮标相对按钮左边缘的水平偏移距离（负数表示在左边）")]
    public float indicatorOffsetX = -50f;

    [Tooltip("浮标相对按钮中心的垂直偏移距离")]
    public float indicatorOffsetY = 0f;

    [Tooltip("浮标定位基准：true=基于按钮左边缘（推荐），false=基于按钮中心")]
    public bool alignToLeftEdge = true;

    [Header("Text Color")]
    [Tooltip("启用文字颜色变化")]
    public bool enableTextColorChange = true;

    [Tooltip("选中按钮的文字颜色")]
    public Color selectedTextColor = Color.yellow;

    [Tooltip("未选中按钮的文字颜色")]
    public Color normalTextColor = Color.white;

    [Header("Animation")]
    [Tooltip("浮标移动的平滑速度（0=瞬间移动，值越大越平滑）")]
    public float smoothSpeed = 10f;

    [Tooltip("启用浮标缩放动画")]
    public bool enablePulseAnimation = true;

    [Tooltip("浮标缩放动画的速度")]
    public float pulseSpeed = 2f;

    [Tooltip("浮标缩放动画的幅度")]
    public float pulseScale = 0.1f;

    private int currentIndex = 0;
    private Vector3 targetPosition;
    private Vector3 initialScale;
    private TextMeshProUGUI[] buttonTexts; // 缓存每个按钮的TMP组件

    void Start()
    {
        // 验证配置
        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogError("[MenuKeyboardNavigation] No buttons assigned!");
            enabled = false;
            return;
        }

        if (floatingIndicator == null)
        {
            Debug.LogError("[MenuKeyboardNavigation] Floating indicator not assigned!");
            enabled = false;
            return;
        }

        // 记录浮标初始缩放
        initialScale = floatingIndicator.localScale;

        // 缓存所有按钮的TMP组件
        CacheButtonTexts();

        // 初始化浮标位置和文字颜色
        UpdateIndicatorPosition(true);
        UpdateTextColors();
    }

    void Update()
    {
        HandleInput();
        UpdateIndicatorMovement();

        // 浮标脉动动画
        if (enablePulseAnimation)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            floatingIndicator.localScale = initialScale * scale;
        }
    }

    /// <summary>
    /// 处理键盘输入
    /// </summary>
    private void HandleInput()
    {
        // 上键：选择上一个按钮
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = buttons.Length - 1; // 循环到最后一个
            }
            UpdateIndicatorPosition(false);
            UpdateTextColors();
        }
        // 下键：选择下一个按钮
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentIndex++;
            if (currentIndex >= buttons.Length)
            {
                currentIndex = 0; // 循环到第一个
            }
            UpdateIndicatorPosition(false);
            UpdateTextColors();
        }
        // Enter键：确认选择
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ConfirmSelection();
        }
    }

    /// <summary>
    /// 缓存所有按钮的TextMeshProUGUI组件
    /// </summary>
    private void CacheButtonTexts()
    {
        buttonTexts = new TextMeshProUGUI[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                buttonTexts[i] = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonTexts[i] == null)
                {
                    Debug.LogWarning($"[MenuKeyboardNavigation] Button '{buttons[i].name}' has no TextMeshProUGUI component in children");
                }
            }
        }
    }

    /// <summary>
    /// 更新所有按钮的文字颜色
    /// </summary>
    private void UpdateTextColors()
    {
        if (!enableTextColorChange || buttonTexts == null)
            return;

        for (int i = 0; i < buttonTexts.Length; i++)
        {
            if (buttonTexts[i] != null)
            {
                buttonTexts[i].color = (i == currentIndex) ? selectedTextColor : normalTextColor;
            }
        }
    }

    /// <summary>
    /// 更新浮标目标位置
    /// </summary>
    /// <param name="immediate">是否立即移动（不使用平滑）</param>
    private void UpdateIndicatorPosition(bool immediate)
    {
        if (currentIndex < 0 || currentIndex >= buttons.Length)
            return;

        Button selectedButton = buttons[currentIndex];
        if (selectedButton == null)
            return;

        RectTransform buttonRect = selectedButton.GetComponent<RectTransform>();
        if (buttonRect == null)
            return;

        // 计算浮标目标位置
        Vector3 buttonPos = buttonRect.position;

        // 如果启用左边缘对齐，计算按钮的左边缘位置
        if (alignToLeftEdge)
        {
            // 获取按钮的宽度（考虑缩放）
            float buttonWidth = buttonRect.rect.width * buttonRect.lossyScale.x;
            // 按钮左边缘 = 按钮中心 - 宽度的一半
            buttonPos.x -= buttonWidth * 0.5f;
        }

        targetPosition = new Vector3(
            buttonPos.x + indicatorOffsetX,
            buttonPos.y + indicatorOffsetY,
            buttonPos.z
        );

        // 如果是立即移动，直接设置位置
        if (immediate)
        {
            floatingIndicator.position = targetPosition;
        }
    }

    /// <summary>
    /// 平滑移动浮标到目标位置
    /// </summary>
    private void UpdateIndicatorMovement()
    {
        if (smoothSpeed <= 0)
        {
            floatingIndicator.position = targetPosition;
        }
        else
        {
            floatingIndicator.position = Vector3.Lerp(
                floatingIndicator.position,
                targetPosition,
                Time.deltaTime * smoothSpeed
            );
        }
    }

    /// <summary>
    /// 确认选择（触发按钮点击）
    /// </summary>
    private void ConfirmSelection()
    {
        if (currentIndex < 0 || currentIndex >= buttons.Length)
            return;

        Button selectedButton = buttons[currentIndex];
        if (selectedButton != null && selectedButton.interactable)
        {
            selectedButton.onClick.Invoke();
            Debug.Log($"[MenuKeyboardNavigation] Button confirmed: {selectedButton.name}");
        }
    }

    /// <summary>
    /// 外部调用：设置当前选中的按钮索引
    /// </summary>
    public void SetSelectedIndex(int index)
    {
        if (index >= 0 && index < buttons.Length)
        {
            currentIndex = index;
            UpdateIndicatorPosition(false);
            UpdateTextColors();
        }
    }

    /// <summary>
    /// 外部调用：获取当前选中的按钮索引
    /// </summary>
    public int GetSelectedIndex()
    {
        return currentIndex;
    }
}
