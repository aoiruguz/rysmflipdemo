using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// NoteManager 负责管理节奏游戏中 Note 的生成、生命周期和游戏流程控制
/// 核心职责：
/// 1. 加载和管理谱面数据（ChartData）
/// 2. 按时间顺序生成 Note
/// 3. 同步音乐播放与 Note 生成
/// 4. 追踪游戏进度和判定完成情况
/// 5. 处理游戏结束和场景切换
/// </summary>
public class NoteManager : MonoBehaviour
{
    [Header("Settings")]
    /// <summary>Note预制体，用于实例化每个Note对象</summary>
    public GameObject notePrefab;

    [Header("Note 文字预设")]
    [Tooltip("Note TMP 随机文字预设文件（每行一条）。留空则不修改 Note 上的文字。")]
    public TextAsset noteTextData;
    private string[] noteMessages;
    
    /// <summary>当前加载的谱面数据，包含所有Note信息、音乐等</summary>
    public ChartData currentChart;
    
    /// <summary>音乐播放器，用于同步音乐与游戏节奏</summary>
  public AudioSource audioSource;

    [Header("Flow Control")]
    /// <summary>
    /// Note从生成点到判定区域的飞行时间（秒）
    /// 决定了Note在屏幕上的运动速度
 /// 默认值为2.0秒，会被GameSettings中的设置覆盖
    /// </summary>
  public float noteTravelTime = 2.0f;
    
    /// <summary>游戏开始前的延迟时间（秒），用于显示歌曲标题等</summary>
    public float prePlayDelay = 2.0f;
    
    [Header("UI Alignment (Canvas Sync)")]
    [Tooltip("Note 生成点的 UI 参照对象（屏幕顶部）")]
    public RectTransform spawnPointUI;
    [Tooltip("Note 判定区域的 UI 参照对象（玩家接住区域）")]
    public RectTransform catchPointUI;
    [Tooltip("Note 失误区域的 UI 参照对象（Miss线）")]
    public RectTransform missPointUI;
    [Tooltip("四条Lane的 UI 参照对象，对应4条竖直的游戏路线")]
    public RectTransform[] laneUIAnchors;

    [Header("Fallback Coordinates")]
    /// <summary>如果没有设置UI参照，使用的默认Note生成的Y坐标</summary>
    public float spawnY = 6f;
    
    /// <summary>如果没有设置UI参照，使用的默认Note判定区域的Y坐标</summary>
    public float catchY = -4f;
    
    /// <summary>如果没有设置UI参照，使用的默认Note失误区域的Y坐标</summary>
    public float missY = -6f;
    
    /// <summary>如果没有设置UI参照，使用的默认四条Lane的X坐标</summary>
    public float[] laneXPositions = new float[] { -3.75f, -1.25f, 1.25f, 3.75f };

    [Header("Difficulty Hints")]
    /// <summary>难度提示UI的父对象，包含Easy/Normal/Hard三个子对象</summary>
    public GameObject hintParent;

    /// <summary>计算出的Note速度（像素/秒），基于飞行时间和距离</summary>
    public float CurrentSpeed { get; private set; }
    
    /// <summary>游戏是否已开始</summary>
    private bool isGameStarted = false;

    /// <summary>游戏是否已停止（用于结算时停止生成Note和音乐）</summary>
    private bool isGameStopped = false;

    /// <summary>音乐开始播放的实时时间戳</summary>
    private float musicStartTime;
    
 /// <summary>当前谱面的总Note数</summary>
private int totalNotes = 0;

    /// <summary>已被判定（捕获或失误）的Note数</summary>
    private int processedNotes = 0;

    /// <summary>
    /// Start 方法：游戏初始化
    /// 执行流程：
    /// 1. 从全局设置同步Note飞行时间
    /// 2. 加载谱面数据（从SongSelectionManager或Inspector）
    /// 3. 初始化HealthSystem（生命值系统）
    /// 4. 配置音频播放器
    /// 5. 计算Note速度
    /// 6. 激活难度提示UI
    /// 7. 启动游戏协程
    /// </summary>
    void Start()
    {
        // --- 新增：从全局设置中同步飞行时间 ---
        // 这样可以确保你在校准界面调整的速度在这里也能生效
   this.noteTravelTime = GameSettings.NoteTravelTime;
        Debug.Log($"[NoteManager] Applied Travel Time from Settings: {this.noteTravelTime:F2}s");

        // 尝试从SongSelectionManager或LevelUIManager获取谱面
        if (currentChart == null)
        {
            currentChart = SongSelectionManager.GetSelectedChart();
            if (currentChart != null)
            {
                Debug.Log("[NoteManager] Loaded chart from SongSelectionManager");
            }
            else
            {
                currentChart = LevelUIManager.GetSelectedChart();
                if (currentChart != null)
                {
                    Debug.Log("[NoteManager] Loaded chart from LevelUIManager");
                }
            }
        }
        else
        {
            // 如果Inspector中已指定谱面，则使用Inspector中的值
            Debug.Log("[NoteManager] Using chart assigned in Inspector (ignoring selection managers)");
        }

     // 如果仍然无法获取谱面，则报错并退出
        if (currentChart == null)
      {
            Debug.LogError("No ChartData assigned to NoteManager!");
        return;
        }

        Debug.Log($"[NoteManager] Current chart: {currentChart.songName}, Difficulty: {currentChart.difficulty}");

        // 初始化进度计数
        totalNotes = currentChart.notes.Count;
        processedNotes = 0;

        // 初始化生命值系统，根据难度设置对应的生命值规则
        HealthSystem.Instance?.Initialize(currentChart.difficulty);

        // 确保AudioSource组件存在
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = GameSettings.MusicVolume;

        // --- 适配 UI Canvas 坐标系统 ---
        // 如果配置了 UI 参照点，则使用 UI 元素的世界坐标，保证动态 Note 和静态 UI 一一对应
        if (spawnPointUI != null) spawnY = spawnPointUI.position.y;
        if (catchPointUI != null) catchY = catchPointUI.position.y;
        if (missPointUI != null) missY = missPointUI.position.y;
        if (laneUIAnchors != null && laneUIAnchors.Length > 0)
        {
            for (int i = 0; i < laneUIAnchors.Length && i < laneXPositions.Length; i++)
            {
                if (laneUIAnchors[i] != null)
                {
                    laneXPositions[i] = laneUIAnchors[i].position.x;
                }
            }
        }

        // --- 核心：基于同步后的 noteTravelTime 计算速度 ---
        // 公式：速度 = 距离 / 时间
        // 这样Note恰好在noteTravelTime秒后到达catchY位置
        float distance = Mathf.Abs(spawnY - catchY);
        CurrentSpeed = distance / noteTravelTime;

        // 设置音乐片段
     audioSource.clip = currentChart.audioClip;

        // 激活对应难度的提示UI
        ActivateDifficultyHints();

    // 启动游戏主流程协程
        StartCoroutine(PlayRoutine());
    }

    /// <summary>
    /// 激活对应难度的提示UI
    /// 根据当前谱面难度，显示对应的难度提示标签（Easy/Normal/Hard）
    /// </summary>
    private void ActivateDifficultyHints()
    {
        // 检查提示UI父对象是否存在
        if (hintParent == null)
        {
      Debug.LogWarning("Hint parent not assigned in NoteManager!");
  return;
        }

        // 查找三个难度对应的UI子对象
     Transform easyHint = hintParent.transform.Find("Easy");
        Transform normalHint = hintParent.transform.Find("Normal");
 Transform hardHint = hintParent.transform.Find("Hard");

      // 先全部关闭
      if (easyHint != null) easyHint.gameObject.SetActive(false);
        if (normalHint != null) normalHint.gameObject.SetActive(false);
    if (hardHint != null) hardHint.gameObject.SetActive(false);

  // 根据难度激活对应的提示UI
        switch (currentChart.difficulty)
        {
            case ChartDifficulty.Easy:
       if (easyHint != null) easyHint.gameObject.SetActive(true);
     break;
    case ChartDifficulty.Normal:
 if (normalHint != null) normalHint.gameObject.SetActive(true);
     break;
        case ChartDifficulty.Hard:
 if (hardHint != null) hardHint.gameObject.SetActive(true);
                break;
      }
    }

    /// <summary>
    /// PlayRoutine 协程：游戏主循环
    /// 负责：
    /// 1. 显示歌曲标题
    /// 2. 等待预延迟时间
    /// 3. 启动音乐播放
    /// 4. 按时间顺序生成Note
    /// 
    /// 工作流程：
    /// - 每帧检查当前时间与下一个Note的生成时间
 /// - 当currentTime >= targetSpawnTime时，生成Note
    /// - 一直循环直到所有Note都生成且音乐播放完毕
    /// </summary>
    private IEnumerator PlayRoutine()
    {
      // 显示歌曲标题，持续时间为 prePlayDelay + 1 秒
        UIManager.Instance?.ShowSongTitle(currentChart.songName, prePlayDelay + 1f);

        // 计算音乐开始播放的实时时间
        musicStartTime = Time.time + prePlayDelay;
        isGameStarted = true;

        int noteIndex = 0;  // 当前要生成的Note下标
     List<NoteData> notes = currentChart.notes;
        bool musicPlayed = false;

        // 主循环：持续生成Note直到所有Note都处理完毕或游戏停止
        while ((noteIndex < notes.Count || !musicPlayed) && !isGameStopped)
{
          // 获取当前歌曲时间（相对于音乐开始）
        float currentTimeMs = GetCurrentSongTimeMs();
      
// 更新UI时间显示
       UIManager.Instance?.UpdateTimeDisplay(currentTimeMs);

            // 当达到时间0时，启动音乐播放
     if (!musicPlayed && currentTimeMs >= 0)
   {
    audioSource.Play();
                musicPlayed = true;
         }

            // 检查是否需要生成新的Note
         if (noteIndex < notes.Count)
      {
   // 获取运行时全局偏移（毫秒）
                float runtimeGlobalOffsetMs = GameSettings.GlobalOffset * 1000f;
     
          // 计算Note的目标生成时间
    // 公式：hitTime - 飞行时间 + 全局偏移
           // 这样Note恰好在hitTime时到达判定区域
         float targetSpawnTimeMs = notes[noteIndex].hitTime - (noteTravelTime * 1000f) + runtimeGlobalOffsetMs;

       // 如果当前时间已到达生成时间，则生成Note
      if (currentTimeMs >= targetSpawnTimeMs)
                {
         SpawnNoteFromData(notes[noteIndex]);
    noteIndex++;
    }
            }

            // 等待下一帧
      yield return null;
     }
  }

    /// <summary>
    /// 获取当前歌曲时间（毫秒）
    /// 相对于音乐开始播放的时间
    /// 返回值：游戏未开始时返回-999999，其他情况返回实际歌曲时间
    /// </summary>
    public float GetCurrentSongTimeMs()
    {
        if (!isGameStarted) return -999999;
        // 将实时秒数转换为毫秒
        return (Time.time - musicStartTime) * 1000f;
 }

    /// <summary>
    /// 根据NoteData创建并生成一个Note
    /// 负责：
    /// 1. 在正确位置创建Note对象
    /// 2. 配置Rigidbody2D用于物理运动
    /// 3. 初始化Note脚本
    /// 4. 为不同类型的Note设置不同的初始化方式
    /// </summary>
    private void SpawnNoteFromData(NoteData data)
    {
        if (notePrefab == null) return;

        // 根据Lane计算生成位置（laneXPositions已在Start中与UI同步）
        float spawnX = laneXPositions.Length > data.lane ? laneXPositions[data.lane] : 0;

        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
        
        // 实例化Note预制体
        GameObject noteObj = Instantiate(notePrefab, spawnPos, Quaternion.identity);

        // 获取或添加Note脚本组件
        Note note = noteObj.GetComponent<Note>();
        if (note == null) note = noteObj.AddComponent<Note>();

        // 配置Rigidbody2D用于直线运动
        Rigidbody2D rb = noteObj.GetComponent<Rigidbody2D>() ?? noteObj.AddComponent<Rigidbody2D>();
   rb.bodyType = RigidbodyType2D.Kinematic;  // 使用Kinematic模式（由代码控制而非物理）
        rb.useFullKinematicContacts = true;

        // 根据Note类型进行初始化
        if (data.noteType == NoteType.Color)
   {
            // 彩色Note：有指定的颜色
        note.Initialize(data.lane, data.color, data.hitTime, CurrentSpeed, missY, (n) => {
 OnNoteProcessed();  // Note被处理时调用
      ScoreManager.Instance?.OnMiss();  // 如果Note到达失误线则扣分
            });
   }
        else
     {
   // 方向Note：需要特定方向的输入（左/右）
            note.InitializeDirectional(data.lane, data.noteType, data.hitTime, CurrentSpeed, missY, (n) => {
          OnNoteProcessed();  // Note被处理时调用
                ScoreManager.Instance?.OnMiss();  // 失误时扣分
         });
      }

        // --- Note 文字随机替换 ---
        // 仿照 PlayerController.SpawnCatchEffect 的懒加载写法
        // Note 是独立 Sprite 元素，不在 Canvas 内，直接用 GetComponentInChildren 查找 TMP 即可
        if (noteMessages == null && noteTextData != null)
        {
            noteMessages = noteTextData.text.Split(
                new[] { '\r', '\n' },
                System.StringSplitOptions.RemoveEmptyEntries
            );
        }

        if (noteMessages != null && noteMessages.Length > 0)
        {
            TextMeshPro tmp = noteObj.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = noteMessages[Random.Range(0, noteMessages.Length)];
            }
        }
    }

    /// <summary>
    /// 当一个Note被处理时调用（无论是捕获还是失误）
    /// 负责：
    /// 1. 增加已处理Note的计数
    /// 2. 通知 SongCompletionDetector（如果存在）
    /// </summary>
    public void OnNoteProcessed()
    {
        processedNotes++;
        Debug.Log($"[NoteManager] Note processed: {processedNotes}/{totalNotes}");

        // 通知 SongCompletionDetector（如果场景中有的话）
        SongCompletionDetector detector = FindFirstObjectByType<SongCompletionDetector>();
        if (detector != null)
        {
            detector.OnNoteProcessed();
        }
    }

    /// <summary>
    /// 停止游戏（停止音乐和Note生成）
    /// 在显示结算界面时调用
    /// </summary>
    public void StopGame()
    {
        isGameStopped = true;

        // 停止音乐播放
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("[NoteManager] Music stopped");
        }

        // 销毁所有还在场景中的Note
        Note[] remainingNotes = FindObjectsByType<Note>(FindObjectsSortMode.None);
        foreach (Note note in remainingNotes)
        {
            Destroy(note.gameObject);
        }

        Debug.Log("[NoteManager] Game stopped, all remaining notes destroyed");
    }
}