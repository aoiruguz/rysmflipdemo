using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Text comboText;
    public Text judgmentText;
    public Text offsetText;
    public Text globalOffsetText;
    public Text songTitleText;
    public Text timeDisplayText;
    public TextMeshProUGUI songClearText; // 歌曲完成文本

    private Coroutine hideRoutine;
private Coroutine titleRoutine;

    void Awake()
    {
        Instance = this;
        if (comboText) comboText.text = "";
        if (judgmentText) judgmentText.text = "";
        if (offsetText) offsetText.text = "";
        if (songTitleText) songTitleText.color = new Color(1, 1, 1, 0);
        if (songClearText) songClearText.gameObject.SetActive(false);
        UpdateGlobalOffsetDisplay();
    }

    public void ShowSongTitle(string title, float duration)
    {
        if (songTitleText == null) return;
        if (titleRoutine != null) StopCoroutine(titleRoutine);
        titleRoutine = StartCoroutine(FadeTitleRoutine(title, duration));
    }

    private IEnumerator FadeTitleRoutine(string title, float totalDuration)
    {
        songTitleText.text = title;
        float fadeTime = 0.5f;
        float stayTime = totalDuration - (fadeTime * 2f);

        // Fade In
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            songTitleText.color = new Color(1, 1, 1, t / fadeTime);
            yield return null;
        }
        songTitleText.color = Color.white;

        yield return new WaitForSeconds(Mathf.Max(0, stayTime));

        // Fade Out
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            songTitleText.color = new Color(1, 1, 1, 1f - (t / fadeTime));
            yield return null;
        }
        songTitleText.color = new Color(1, 1, 1, 0);
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
        if (judgmentText)
        {
            judgmentText.text = judgment;
            judgmentText.color = GetJudgmentColor(judgment);
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
            comboText.text = combo > 0 ? $"{combo} COMBO" : "";
        }

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideJudgmentAfterDelay(1.0f));
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
        yield return new WaitForSeconds(delay);
        if (judgmentText) judgmentText.text = "";
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
