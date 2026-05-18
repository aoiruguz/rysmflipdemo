using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 控制按钮选中/未选中时的透明度和图片切换
/// 配合 Sprite Swap 使用，实现选中态切换图片 + 透明度变化
/// </summary>
[RequireComponent(typeof(Button))]
public class SelectableButtonAlpha : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("透明度设置")]
    [Tooltip("未选中时的透明度 (0-1)")]
    [Range(0f, 1f)]
    public float unselectedAlpha = 0.5f;

    [Tooltip("选中时的透明度 (0-1)")]
    [Range(0f, 1f)]
    public float selectedAlpha = 1f;

    [Header("Sprite 设置")]
    [Tooltip("未选中时的图片（无对勾）")]
    public Sprite normalSprite;

    [Tooltip("选中时的图片（有对勾）")]
    public Sprite selectedSprite;

    [Header("引用")]
    [Tooltip("要控制透明度和图片的 Image 组件，留空则自动获取")]
    public Image targetImage;

    private Button button;
    private bool isSelected = false;

    private void Awake()
    {
        button = GetComponent<Button>();

        // 如果没有指定 targetImage，自动获取
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetImage == null)
        {
            Debug.LogError($"[SelectableButtonAlpha] No Image component found on {gameObject.name}!");
            enabled = false;
            return;
        }

        // 如果没有手动设置 Sprite，尝试从 Button 的 SpriteState 获取
        if (normalSprite == null)
        {
            normalSprite = targetImage.sprite;
        }

        if (selectedSprite == null)
        {
            SpriteState spriteState = button.spriteState;
            selectedSprite = spriteState.selectedSprite;
        }
    }

    private void Start()
    {
        // 初始化为未选中状态
        UpdateVisuals(false);
    }

    /// <summary>
    /// 当按钮被选中时调用
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        // 不再自动更新视觉，由外部控制
        // isSelected = true;
        // UpdateVisuals(true);
    }

    /// <summary>
    /// 当按钮失去选中时调用
    /// </summary>
    public void OnDeselect(BaseEventData eventData)
    {
        // 不再自动更新视觉，由外部控制
        // isSelected = false;
        // UpdateVisuals(false);
    }

    /// <summary>
    /// 更新视觉效果（透明度 + 图片）
    /// </summary>
    private void UpdateVisuals(bool selected)
    {
        if (targetImage == null) return;

        // 设置透明度
        Color color = targetImage.color;
        color.a = selected ? selectedAlpha : unselectedAlpha;
        targetImage.color = color;

        // 切换图片
        if (selected && selectedSprite != null)
        {
            targetImage.sprite = selectedSprite;
        }
        else if (!selected && normalSprite != null)
        {
            targetImage.sprite = normalSprite;
        }
    }

    /// <summary>
    /// 手动设置选中状态（供外部调用）
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals(selected);

        // 同时更新 Unity 的选中状态
        if (selected)
        {
            button.Select();
        }
        else
        {
            // 取消选中：让 EventSystem 选中 null
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    /// <summary>
    /// 仅更新视觉效果，不触碰 EventSystem（供外部批量更新使用）
    /// </summary>
    public void SetSelectedVisualOnly(bool selected)
    {
        isSelected = selected;
        UpdateVisuals(selected);
    }

    /// <summary>
    /// 获取当前是否选中
    /// </summary>
    public bool IsSelected()
    {
        return isSelected;
    }
}
