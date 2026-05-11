using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public TextMeshProUGUI comboText;
    public Text offsetText;
    public Text globalOffsetText;
    public TextMeshProUGUI songTitleText;
    public Text timeDisplayText;
    public TextMeshProUGUI songClearText; // 歌曲完成文本

    [Header("Judgment Image Display")]
    [Tooltip("用于显示判定结果图片的 Image 组件")]
    public Image judgmentImage;
    public Sprite perfectSprite;
    public Sprite greatSprite;
    public Sprite goodSprite;
    public Sprite missSprite;

    private Coroutine hideRoutine;
private Coroutine titleRoutine;

    void Awake()
    {
        Instance = this;
        if (comboText) 
        {
            comboText.text = "";
            comboText.gameObject.SetActive(false);
        }
        if (judgmentImage) judgmentImage.gameObject.SetActive(false);
        if (offsetText) offsetText.text = "";
        if (songTitleText) songTitleText.color = new Color(1, 1, 1, 0);
        if (songClearText) songClearText.gameObject.SetActive(false);
        UpdateGlobalOffsetDisplay();
    }

    public void ShowSongTitle(string title)
    {
        if (songTitleText == null) return;
        if (titleRoutine != null) StopCoroutine(titleRoutine);
        titleRoutine = StartCoroutine(FadeTitleRoutine(title));
    }

    private IEnumerator FadeTitleRoutine(string title)
    {
        songTitleText.text = title;
        float fadeTime = 1.0f; // 稍微调慢一点，让它“缓慢”出现

        // Fade In
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            songTitleText.color = new Color(1, 1, 1, t / fadeTime);
            yield return null;
        }
        songTitleText.color = Color.white;
    }

    public void UpdateGlobalOffsetDisplay()
    {
        if (globalOffsetText)
        {
            float offsetMs = GameSettings.GlobalOffset * 1000f;
            string sign = offsetMs >= 0 ? "+" : "";
            globalOffsetText.text = $"Global Offset: {sign}{offsetMs:F0}ms";
        }
    }

    public void UpdateTimeDisplay(float timeMs)
    {
        if (timeDisplayText)
        {
            timeDisplayText.text = $"{timeMs:F0}ms";
        }
    }

    public void ShowJudgment(string judgment, float offsetMs, int combo, bool showOffset = true)
    {
        // 处理图片显示逻辑
        if (judgmentImage != null)
        {
            Sprite targetSprite = null;
            switch (judgment.ToUpper())
            {
                case "PERFECT": targetSprite = perfectSprite; break;
                case "GREAT":   targetSprite = greatSprite; break;
                case "GOOD":    targetSprite = goodSprite; break;
                case "MISS":    targetSprite = missSprite; break;
            }

            if (targetSprite != null)
            {
                judgmentImage.sprite = targetSprite;
                judgmentImage.gameObject.SetActive(true);
                
                // 初始化动画状态：透明度 0，缩放 1.1
                Color col = judgmentImage.color;
                col.a = 0;
                judgmentImage.color = col;
                judgmentImage.transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                judgmentImage.gameObject.SetActive(false);
            }
        }

        if (offsetText)
        {
            if (showOffset && judgment != "MISS")
            {
                string sign = offsetMs >= 0 ? "+" : "";
                offsetText.text = $"{sign}{offsetMs:F0}ms";
                offsetText.color = GetJudgmentColor(judgment);
            }
            else
            {
                offsetText.text = "";
            }
        }

        if (comboText)
        {
            bool hasCombo = combo > 0;
            comboText.gameObject.SetActive(hasCombo);
            if (hasCombo) comboText.text = combo.ToString();
        }

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideJudgmentAfterDelay(0.5f)); // 缩短显示时间，提升反馈节奏
    }

    private Color GetJudgmentColor(string judgment)
    {
        switch (judgment)
        {
            case "PERFECT": return Color.yellow;
            case "GOOD": return Color.cyan;
            default: return Color.red;
        }
    }

    private IEnumerator HideJudgmentAfterDelay(float delay)
    {
        if (judgmentImage == null) yield break;

        Color col = judgmentImage.color;
        float elapsed = 0;
        
        // 阶段 1：快速淡入 + 缩放回弹 (约 0.05s)
        float fadeInDuration = 0.05f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            col.a = t;
            judgmentImage.color = col;
            judgmentImage.transform.localScale = Vector3.Lerp(Vector3.one * 1.1f, Vector3.one, t);
            yield return null;
        }
        col.a = 1f;
        judgmentImage.color = col;
        judgmentImage.transform.localScale = Vector3.one;

        // 阶段 2：展示停留
        float fadeOutDuration = 0.15f;
        float stayDuration = delay - fadeInDuration - fadeOutDuration;
        yield return new WaitForSeconds(Mathf.Max(0, stayDuration));

        // 阶段 3：平滑淡出
        elapsed = 0;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            col.a = 1f - t;
            judgmentImage.color = col;
            yield return null;
        }

        judgmentImage.gameObject.SetActive(false);
        if (offsetText) offsetText.text = "";
    }

    /// <summary>
    /// 显示歌曲完成文本（Song Clear / Full Combo / All Perfect）
    /// </summary>
    public void ShowSongClearText(string text)
    {
        if (songClearText == null)
        {
            Debug.LogWarning("[UIManager] songClearText is not assigned!");
            return;
        }

        songClearText.text = text;
        songClearText.gameObject.SetActive(true);

        // 设置颜色
        if (text.Contains("All Perfect"))
        {
            songClearText.color = new Color(1f, 0.5f, 1f); // 粉紫色
        }
        else if (text.Contains("Full Combo"))
        {
            songClearText.color = Color.yellow;
        }
        else
        {
            songClearText.color = Color.white;
        }

        Debug.Log($"[UIManager] Showing song clear text: {text}");
    }
}
