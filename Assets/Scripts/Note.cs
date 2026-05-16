using UnityEngine;
using DG.Tweening;
using TMPro;

public class Note : MonoBehaviour
{
    private SpriteRenderer mainRenderer;
    private TextMeshPro tmpText;
    private bool isDead = false;

    void Awake()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        tmpText = GetComponentInChildren<TextMeshPro>();
    }

    [Header("State Sprites")]
    [Tooltip("默认/正常状态的精灵图")]
    public Sprite normalSprite;
    [Tooltip("【J（color 0）】红色状态的精灵图")]
    public Sprite sprite0_J_Red;
    [Tooltip("【L（color 1）】黄色状态的精灵图")]
    public Sprite sprite1_L_Yellow;
    [Tooltip("【K（color 2）】蓝色状态的精灵图")]
    public Sprite sprite2_K_Blue;
    [Tooltip("左划状态的精灵图 (对应A / DirectionalLeft)")]
    public Sprite leftSwipeSprite;
    [Tooltip("右划状态的精灵图 (对应D / DirectionalRight)")]
    public Sprite rightSwipeSprite;

    public int Lane { get; private set; }
    public GameColor Color { get; private set; }
    public NoteType NoteType { get; private set; }
    public float HitTimeMs { get; private set; }

    [Header("Miss Animation Settings")]
    [Tooltip("Miss 动画持续时间")]
    public float missAnimDuration = 0.35f;
    [Tooltip("Miss 时的目标颜色（通常是红色且透明度为0）")]
    public Color missTargetColor = new Color(1f, 0.4f, 0.4f, 0f);
    [Tooltip("颤抖强度")]
    public float missShakeStrength = 0.15f;
    [Tooltip("颤抖频率 (Vibrato)")]
    public int missShakeVibrato = 30;
    [Tooltip("颤抖随机度")]
    public float missShakeRandomness = 90f;

    [Header("Spawn Animation Settings")]
    [Tooltip("出现时的渐现时间")]
    public float spawnAnimDuration = 0.2f;

    private float speed;
    private float missY;
    private System.Action<Note> onMiss;

    // For color notes
    public void Initialize(int lane, GameColor color, float hitTimeMs, float speed, float missY, System.Action<Note> onMiss)
    {
        this.Lane = lane;
        this.Color = color;
        this.NoteType = NoteType.Color;
        this.HitTimeMs = hitTimeMs;
        this.speed = speed;
        this.missY = missY;
        this.onMiss = onMiss;

        UpdateVisual();
        PlaySpawnAnimation();
    }

    // For directional notes
    public void InitializeDirectional(int lane, NoteType noteType, float hitTimeMs, float speed, float missY, System.Action<Note> onMiss)
    {
        this.Lane = lane;
        this.NoteType = noteType;
        this.HitTimeMs = hitTimeMs;
        this.speed = speed;
        this.missY = missY;
        this.onMiss = onMiss;

        UpdateVisual();
        PlaySpawnAnimation();
    }

    private void UpdateVisual()
    {
        if (mainRenderer == null) return;

        // 恢复默认颜色，防止被之前的代码逻辑污染成纯色
        mainRenderer.color = UnityEngine.Color.white;

        if (NoteType == NoteType.Color)
        {
            switch (Color)
            {
                case GameColor.J_Color0_Red:
                    mainRenderer.sprite = sprite0_J_Red;
                    break;
                case GameColor.L_Color1_Yellow:
                    mainRenderer.sprite = sprite1_L_Yellow;
                    break;
                case GameColor.K_Color2_Blue:
                    mainRenderer.sprite = sprite2_K_Blue;
                    break;
                default:
                    mainRenderer.sprite = normalSprite;
                    break;
            }
        }
        else if (NoteType == NoteType.DirectionalLeft)
        {
            mainRenderer.sprite = leftSwipeSprite;
        }
        else if (NoteType == NoteType.DirectionalRight)
        {
            mainRenderer.sprite = rightSwipeSprite;
        }

        // --- 新增：对于 A/D 键方向 Note，清空子物体的 TMP 文字 ---
        if (tmpText != null)
        {
            if (NoteType == NoteType.DirectionalLeft || NoteType == NoteType.DirectionalRight)
            {
                tmpText.text = "";
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        // Simple downward movement based on the speed calculated by GameManager
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        if (transform.position.y < missY)
        {
            isDead = true;
            onMiss?.Invoke(this);
            PlayMissAnimation();
        }
    }

    private void PlaySpawnAnimation()
    {
        // 1. SpriteRenderer 渐现
        if (mainRenderer != null)
        {
            // 初始设为透明
            Color c = mainRenderer.color;
            c.a = 0f;
            mainRenderer.color = c;
            // 渐变到不透明
            mainRenderer.DOFade(1f, spawnAnimDuration).SetEase(Ease.OutQuad);
        }

        // 2. TextMeshPro 渐现
        if (tmpText != null)
        {
            // 初始设为透明
            Color c = tmpText.color;
            c.a = 0f;
            tmpText.color = c;
            // 渐变到不透明
            tmpText.DOFade(1f, spawnAnimDuration).SetEase(Ease.OutQuad);
        }
    }

    private void PlayMissAnimation()
    {
        // 1. 颜色变红并快速降低透明度
        if (mainRenderer != null)
        {
            mainRenderer.DOColor(missTargetColor, missAnimDuration).SetEase(Ease.OutQuad);
        }

        if (tmpText != null)
        {
            tmpText.DOColor(missTargetColor, missAnimDuration).SetEase(Ease.OutQuad);
        }

        // 2. 颤抖效果
        transform.DOShakePosition(missAnimDuration, strength: missShakeStrength, vibrato: missShakeVibrato, randomness: missShakeRandomness)
            .OnComplete(() => {
                if (this != null && gameObject != null)
                {
                    Destroy(gameObject);
                }
            });
    }
}
