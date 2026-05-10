using UnityEngine;

public class Note : MonoBehaviour
{
    private SpriteRenderer mainRenderer;

    void Awake()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
    }

    [Header("State Sprites")]
    [Tooltip("默认/正常状态的精灵图")]
    public Sprite normalSprite;
    [Tooltip("红色状态的精灵图 (对应快捷键J / ColorA)")]
    public Sprite redSprite;
    [Tooltip("黄色状态的精灵图 (对应快捷键K / ColorB)")]
    public Sprite yellowSprite;
    [Tooltip("蓝色状态的精灵图 (对应快捷键L / ColorC)")]
    public Sprite blueSprite;
    [Tooltip("左划状态的精灵图 (对应A / DirectionalLeft)")]
    public Sprite leftSwipeSprite;
    [Tooltip("右划状态的精灵图 (对应D / DirectionalRight)")]
    public Sprite rightSwipeSprite;

    public int Lane { get; private set; }
    public GameColor Color { get; private set; }
    public NoteType NoteType { get; private set; }
    public float HitTimeMs { get; private set; }

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
                case GameColor.ColorA:
                    mainRenderer.sprite = redSprite;
                    break;
                case GameColor.ColorB:
                    mainRenderer.sprite = yellowSprite;
                    break;
                case GameColor.ColorC:
                    mainRenderer.sprite = blueSprite;
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
    }

    void Update()
    {
        // Simple downward movement based on the speed calculated by GameManager
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        if (transform.position.y < missY)
        {
            onMiss?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
