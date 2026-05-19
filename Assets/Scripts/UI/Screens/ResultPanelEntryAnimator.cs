using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// 结算界面动画总控制器。
///
/// 功能一（进场）：
///   在 Awake 时记录 targetPanels 的最终 anchoredPosition 并将其推至屏幕外；
///   外部调用 PlayEntryAnimation() 后，按索引顺序依次滑入回原位。
///
/// 功能二（撤离 + 进场联动）：
///   外部调用 PlayExitThenEntryAnimation() 后，先让 exitPanels 按索引顺序
///   依次滑出屏幕外，全部撤离完毕后禁用 exitObjectToDisable（如 Gameplay 根节点），
///   再自动触发 targetPanels 的进场动画。
/// </summary>
public class ResultPanelEntryAnimator : MonoBehaviour
{
    /// <summary>
    /// 滑入 / 滑出方向枚举（进场从此方向飞入；退场则向此方向飞出）
    /// </summary>
    public enum SlideDirection
    {
        Top,
        Bottom,
        Left,
        Right
    }

    // ──────────────────────────────────────────────
    // 可序列化配置项
    // ──────────────────────────────────────────────

    [System.Serializable]
    public class PanelAnimationConfig
    {
        [Tooltip("需要进行动画的子UI物体")]
        public RectTransform panel;
        [Tooltip("屏幕侧方位：进场时从此侧飞入，退场时从此侧滑出")]
        public SlideDirection slideSide = SlideDirection.Left;
    }

    // ──────────────────────────────────────────────
    // Inspector 字段
    // ──────────────────────────────────────────────

    [Header("Result Panel Entry（进场面板）")]
    [Tooltip("结算界面子UI物体列表及其滑入方向，按索引顺序依次进场")]
    public List<PanelAnimationConfig> targetPanels = new List<PanelAnimationConfig>();

    [Header("Gameplay Panel Exit（撤离面板）")]
    [Tooltip("Gameplay 中需要依次撤离屏幕的子物体列表及其撤离方向，按索引顺序依次退场")]
    public List<PanelAnimationConfig> exitPanels = new List<PanelAnimationConfig>();

    [Tooltip("所有 exitPanels 撤离完毕后需要整体禁用的对象（如 Gameplay 根节点或 Canvas）。留空则跳过禁用步骤。")]
    public GameObject exitObjectToDisable;

    [Header("Animation Settings")]
    [Tooltip("单个子物体滑入/滑出到位的动画持续时间（秒）")]
    public float slideDuration = 0.4f;

    [Tooltip("相邻两个子物体依次进场/退场的时间差（秒）")]
    public float sequenceDelay = 0.2f;

    [Tooltip("淡入动画持续时间（秒），若物体挂载了 CanvasGroup 则生效")]
    public float fadeDuration = 0.3f;

    [Tooltip("动画缓动曲线（进场）")]
    public Ease entryEase = Ease.OutCubic;

    [Tooltip("动画缓动曲线（退场）")]
    public Ease exitEase = Ease.InCubic;

    // ──────────────────────────────────────────────
    // 内部状态
    // ──────────────────────────────────────────────

    /// <summary>targetPanels 的最终（在屏）anchoredPosition</summary>
    private Vector2[] finalPositions;

    /// <summary>targetPanels 在屏幕外的起点 anchoredPosition</summary>
    private Vector2[] offScreenPositions;

    /// <summary>exitPanels 在屏幕外的终点 anchoredPosition（退场目标位）</summary>
    private Vector2[] exitOffScreenPositions;

    /// <summary>当前控制器持有的 Sequence 引用，用于防抖 Kill</summary>
    private Sequence currentSequence;

    /// <summary>所属 Canvas 的 RectTransform，用于计算屏幕边界</summary>
    private RectTransform canvasRect;

    // ──────────────────────────────────────────────
    // 生命周期
    // ──────────────────────────────────────────────

    private void Awake()
    {
        // 获取父级 Canvas RectTransform
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("[ResultPanelEntryAnimator] 未找到父级 Canvas，屏幕外推算将使用 Screen 尺寸作为回退。");
        }

        InitializeEntryPanels();
        InitializeExitPanels();
    }

    private void OnDestroy()
    {
        KillCurrentSequence();

        KillAllPanelTweens(targetPanels);
        KillAllPanelTweens(exitPanels);
    }

    // ──────────────────────────────────────────────
    // 初始化
    // ──────────────────────────────────────────────

    /// <summary>
    /// 记录 targetPanels 的最终位置，并立即推至屏幕外待命。
    /// </summary>
    private void InitializeEntryPanels()
    {
        if (targetPanels == null || targetPanels.Count == 0) return;

        int count = targetPanels.Count;
        finalPositions = new Vector2[count];
        offScreenPositions = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            if (targetPanels[i] == null || targetPanels[i].panel == null)
            {
                Debug.LogWarning($"[ResultPanelEntryAnimator] targetPanels[{i}].panel 为 null，已跳过。");
                continue;
            }

            RectTransform panel = targetPanels[i].panel;
            finalPositions[i] = panel.anchoredPosition;
            offScreenPositions[i] = CalculateOffScreenPosition(panel, finalPositions[i], targetPanels[i].slideSide);
            panel.anchoredPosition = offScreenPositions[i];

            // 初始设置为透明（如果有 CanvasGroup）
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0;
        }
    }

    /// <summary>
    /// 预计算 exitPanels 的屏幕外目标位置（退场终点）。
    /// exitPanels 在 Awake 时不需要被移动，它们已经在屏幕上处于正常位置。
    /// </summary>
    private void InitializeExitPanels()
    {
        if (exitPanels == null || exitPanels.Count == 0) return;

        int count = exitPanels.Count;
        exitOffScreenPositions = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            if (exitPanels[i] == null || exitPanels[i].panel == null)
            {
                Debug.LogWarning($"[ResultPanelEntryAnimator] exitPanels[{i}].panel 为 null，已跳过。");
                continue;
            }

            RectTransform panel = exitPanels[i].panel;
            // 退场：从当前位置飞向屏幕外（与进场方向相同，同一套屏幕外坐标）
            exitOffScreenPositions[i] = CalculateOffScreenPosition(panel, panel.anchoredPosition, exitPanels[i].slideSide);
        }
    }

    // ──────────────────────────────────────────────
    // 公开方法
    // ──────────────────────────────────────────────

    /// <summary>
    /// 播放 Result Panel 进场动画。
    /// 按 targetPanels 索引顺序依次将子物体从屏幕外滑入到最终位置。
    /// </summary>
    public void PlayEntryAnimation()
    {
        if (targetPanels == null || targetPanels.Count == 0 || finalPositions == null)
        {
            Debug.LogWarning("[ResultPanelEntryAnimator] targetPanels 未配置或未初始化，无法播放进场动画。");
            return;
        }

        KillCurrentSequence();
        currentSequence = BuildEntrySequence();
        currentSequence.SetUpdate(true).Play();
    }

    /// <summary>
    /// 先播放 exitPanels 的退场动画（按索引顺序依次滑出），
    /// 所有元素退场完毕后禁用 exitObjectToDisable，
    /// 然后自动触发 targetPanels 的进场动画。
    /// </summary>
    public void PlayExitThenEntryAnimation()
    {
        KillCurrentSequence();

        // 若无退场面板，直接进入进场流程
        if (exitPanels == null || exitPanels.Count == 0 || exitOffScreenPositions == null)
        {
            Debug.Log("[ResultPanelEntryAnimator] exitPanels 为空，跳过退场，直接播放进场动画。");
            DisableExitObject();
            PlayEntryAnimation();
            return;
        }

        currentSequence = DOTween.Sequence();

        // ── 阶段一：退场 ──
        for (int i = 0; i < exitPanels.Count; i++)
        {
            if (exitPanels[i] == null || exitPanels[i].panel == null) continue;

            RectTransform target = exitPanels[i].panel;
            float insertTime = i * sequenceDelay;

            currentSequence.Insert(insertTime,
                target.DOAnchorPos(exitOffScreenPositions[i], slideDuration)
                      .SetEase(exitEase));
        }

        // 退场总时长 = 最后一个元素开始的时间 + 单次动画时长
        float exitTotalDuration = (exitPanels.Count - 1) * sequenceDelay + slideDuration;

        // ── 阶段二：退场结束后禁用对象，再叠加进场 Sequence ──
        currentSequence.InsertCallback(exitTotalDuration, () =>
        {
            DisableExitObject();

            // 进场序列插入在退场总时长之后
            Sequence entrySeq = BuildEntrySequence();
            entrySeq.SetUpdate(true).Play();
        });

        currentSequence.SetUpdate(true).Play();
    }

    /// <summary>
    /// 将所有 targetPanels 一键打回屏幕外待命，用于游戏重新开始时重置。
    /// </summary>
    public void ResetToOffScreen()
    {
        KillCurrentSequence();

        if (targetPanels == null || offScreenPositions == null) return;

        for (int i = 0; i < targetPanels.Count; i++)
        {
            if (targetPanels[i] == null || targetPanels[i].panel == null) continue;

            RectTransform panel = targetPanels[i].panel;
            panel.DOKill();
            panel.anchoredPosition = offScreenPositions[i];
        }
    }

    /// <summary>
    /// 将所有 exitPanels 一键恢复到屏幕内的初始位置（如需重新开始游戏）。
    /// 注意：exitPanels 的"在屏位置"是它们在 Awake 时的 anchoredPosition，
    /// 如需精确恢复请在外部管理，或在此处自行记录。
    /// </summary>
    public void ResetExitPanelsToOnScreen()
    {
        if (exitPanels == null) return;

        foreach (var config in exitPanels)
        {
            if (config == null || config.panel == null) continue;
            // 重新初始化退场面板，使它们回到"当前 anchoredPosition"对应的屏幕外坐标准备下一次退场
            // 实际上屏位置的恢复需要由外部或 Gameplay 自行管理
            config.panel.DOKill();
        }

        // 重新预计算退场目标坐标（因为 panel 可能已被外部移动）
        InitializeExitPanels();
    }

    // ──────────────────────────────────────────────
    // 内部工具方法
    // ──────────────────────────────────────────────

    /// <summary>
    /// 构建 targetPanels 进场 Sequence（不自动播放，由调用方控制）。
    /// </summary>
    private Sequence BuildEntrySequence()
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < targetPanels.Count; i++)
        {
            if (targetPanels[i] == null || targetPanels[i].panel == null) continue;

            RectTransform target = targetPanels[i].panel;

            // 确保子物体激活，触发其挂载脚本的 Awake/Start
            if (!target.gameObject.activeSelf)
                target.gameObject.SetActive(true);

            // 确保从屏幕外起点开始（防止重复调用时位置混乱）
            target.anchoredPosition = offScreenPositions[i];

            float insertTime = i * sequenceDelay;

            seq.Insert(insertTime,
                target.DOAnchorPos(finalPositions[i], slideDuration)
                      .SetEase(entryEase));

            // 同时执行淡入动画
            CanvasGroup cg = target.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                seq.Insert(insertTime, cg.DOFade(1, fadeDuration));
            }
        }

        return seq;
    }

    /// <summary>
    /// 禁用 exitObjectToDisable（若已配置）。
    /// </summary>
    private void DisableExitObject()
    {
        if (exitObjectToDisable != null && exitObjectToDisable.activeSelf)
        {
            exitObjectToDisable.SetActive(false);
        }
    }

    /// <summary>
    /// 根据滑入方向和子物体尺寸，计算刚好将其隐藏在屏幕外的坐标。
    /// 进场时此坐标是起点；退场时此坐标是终点。
    /// </summary>
    private Vector2 CalculateOffScreenPosition(RectTransform target, Vector2 referencePos, SlideDirection direction)
    {
        float canvasWidth, canvasHeight;
        if (canvasRect != null)
        {
            canvasWidth = canvasRect.rect.width;
            canvasHeight = canvasRect.rect.height;
        }
        else
        {
            canvasWidth = Screen.width;
            canvasHeight = Screen.height;
        }

        float selfWidth = target.rect.width;
        float selfHeight = target.rect.height;

        float anchorCenterX = (target.anchorMin.x + target.anchorMax.x) * 0.5f;
        float anchorCenterY = (target.anchorMin.y + target.anchorMax.y) * 0.5f;

        float worldCenterX = anchorCenterX * canvasWidth + referencePos.x;
        float worldCenterY = anchorCenterY * canvasHeight + referencePos.y;

        // pivot 偏移修正
        float pivotOffsetX = (0.5f - target.pivot.x) * selfWidth;
        float pivotOffsetY = (0.5f - target.pivot.y) * selfHeight;
        worldCenterX += pivotOffsetX;
        worldCenterY += pivotOffsetY;

        float rightEdge  = worldCenterX + selfWidth  * 0.5f;
        float leftEdge   = worldCenterX - selfWidth  * 0.5f;
        float topEdge    = worldCenterY + selfHeight * 0.5f;
        float bottomEdge = worldCenterY - selfHeight * 0.5f;

        Vector2 offScreenPos = referencePos;

        switch (direction)
        {
            case SlideDirection.Left:
                // 右边缘推到 Canvas 左边界之外
                offScreenPos.x = referencePos.x - rightEdge;
                break;

            case SlideDirection.Right:
                // 左边缘推到 Canvas 右边界之外
                offScreenPos.x = referencePos.x + (canvasWidth - leftEdge);
                break;

            case SlideDirection.Top:
                // 下边缘推到 Canvas 上边界之外
                offScreenPos.y = referencePos.y + (canvasHeight - bottomEdge);
                break;

            case SlideDirection.Bottom:
                // 上边缘推到 Canvas 下边界之外
                offScreenPos.y = referencePos.y - topEdge;
                break;
        }

        return offScreenPos;
    }

    /// <summary>
    /// 杀掉当前控制器的 Sequence，防止动画互相冲突。
    /// </summary>
    private void KillCurrentSequence()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
            currentSequence = null;
        }
    }

    private void KillAllPanelTweens(List<PanelAnimationConfig> list)
    {
        if (list == null) return;
        foreach (var config in list)
        {
            if (config != null && config.panel != null)
                config.panel.DOKill();
        }
    }
}
