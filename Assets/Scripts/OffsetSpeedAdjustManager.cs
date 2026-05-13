using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class OffsetSpeedAdjustManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI offsetValueText;
    public TextMeshProUGUI travelTimeValueText;
    public Button offsetIncreaseButton;
    public Button offsetDecreaseButton;
    public Button travelTimeIncreaseButton;
    public Button travelTimeDecreaseButton;
    public Toggle preInputModeToggle;
    public Button okButton;

    [Header("Adjustment Steps")]
    public float offsetStep = 0.01f; // 10ms
    public float travelTimeStep = 0.1f; // 0.1s

    [Header("Scene Settings")]
    public float reloadTime = 22f;

    private PlayerController.ControlMode originalInputMode;

    // 静态变量：记住从哪个场景进入的
    private static string returnSceneName = "Title";

    /// <summary>
    /// 设置返回场景（在跳转到延迟调节场景前调用）
    /// </summary>
    public static void SetReturnScene(string sceneName)
    {
        returnSceneName = sceneName;
        Debug.Log($"[OffsetAdjust] Return scene set to: {sceneName}");
    }

    void Start()
    {
        // 保存原始输入模式
        originalInputMode = (PlayerController.ControlMode)GameSettings.InputMode;

        // 初始化时，确保场景中的 NoteManager 同步最新的全局设置
        SyncNoteManager();

        // 初始化 UI
        UpdateUI();
        SetupButtons();

        // 启动重新加载协程
        StartCoroutine(ReloadSceneAfterTime());
    }

    void Update()
    {
        // 实时更新 UI 确保显示准确
        UpdateUI();
    }

    private void SetupButtons()
    {
        // 使用 Lambda 表达式绑定按钮事件
        offsetIncreaseButton?.onClick.AddListener(() => AdjustOffset(offsetStep));
        offsetDecreaseButton?.onClick.AddListener(() => AdjustOffset(-offsetStep));
        travelTimeIncreaseButton?.onClick.AddListener(() => AdjustTravelTime(travelTimeStep));
        travelTimeDecreaseButton?.onClick.AddListener(() => AdjustTravelTime(-travelTimeStep));
        okButton?.onClick.AddListener(OnOKButtonClicked);

        // 设置复选框初始状态和监听
        if (preInputModeToggle != null)
        {
            preInputModeToggle.isOn = false; // 默认不勾选
            preInputModeToggle.onValueChanged.AddListener(OnPreInputModeToggleChanged);
        }
    }

    private void AdjustOffset(float delta)
    {
        // 直接调用 GameSettings 的封装方法（它内部处理了 PlayerPrefs.Save）
        GameSettings.AdjustOffset(delta);
        UpdateUI();
        Debug.Log($"[OffsetAdjust] Offset: {GameSettings.GlobalOffset * 1000f:F0}ms");
    }

    private void AdjustTravelTime(float delta)
    {
        // 1. 修改全局持久化设置（触发 PlayerPrefs 存储）
        GameSettings.NoteTravelTime = Mathf.Max(0.5f, GameSettings.NoteTravelTime + delta);

        // 2. 立即同步给当前场景的 NoteManager，实现”即调即看”
        SyncNoteManager();

        UpdateUI();
        Debug.Log($"[OffsetAdjust] Travel time saved: {GameSettings.NoteTravelTime:F2}s");

        // 3. 重新加载场景以应用新的 travel time
        StopCoroutine(ReloadSceneAfterTime());
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 将全局设置同步给场景中的 NoteManager 实例
    /// </summary>
    private void SyncNoteManager()
    {
        // 1. 同步 NoteManager (原本的逻辑)
        var noteManager = FindFirstObjectByType<NoteManager>();
        if (noteManager != null)
        {
            noteManager.noteTravelTime = GameSettings.NoteTravelTime;
        }

        // 2. 新增：同步 PlayerController
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.RefreshControlMode();
        }
    }

    private void UpdateUI()
    {
        // 直接从 GameSettings 读取，不再依赖场景物体
        if (offsetValueText != null)
            offsetValueText.text = $"{(GameSettings.GlobalOffset * 1000f):F0}ms";

        if (travelTimeValueText != null)
            travelTimeValueText.text = $"{GameSettings.NoteTravelTime:F2}s";
    }

    private IEnumerator ReloadSceneAfterTime()
    {
        yield return new WaitForSeconds(reloadTime);
        Debug.Log("[OffsetAdjust] Reloading scene for loop test");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnOKButtonClicked()
    {
        // 恢复原始输入模式后跳转
        GameSettings.InputMode = (int)originalInputMode;
        Debug.Log($"[OffsetAdjust] Returning to {returnSceneName}");
        SceneManager.LoadScene(returnSceneName);
    }

    private void OnPreInputModeToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // 切换到预输入模式 (Preset = 0)
            GameSettings.InputMode = (int)PlayerController.ControlMode.Preset;
        }
        else
        {
            // 恢复到原始输入模式
            GameSettings.InputMode = (int)originalInputMode;
        }

        // 同步到 NoteManager
        SyncNoteManager();
    }

    void OnDestroy()
    {
        // 场景销毁时恢复原始输入模式
        GameSettings.InputMode = (int)originalInputMode;
    }
}