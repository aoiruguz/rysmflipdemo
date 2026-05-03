using UnityEngine;

public class Note : MonoBehaviour
{
    [Header("Child Objects")]
    public GameObject colorNoteVisual;      // 普通三色note的子物体
    public GameObject leftDirectionalVisual;  // 左方向note的子物体
    public GameObject rightDirectionalVisual; // 右方向note的子物体

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
        // Activate/deactivate child objects based on note type
        if (colorNoteVisual != null)
            colorNoteVisual.SetActive(NoteType == NoteType.Color);

        if (leftDirectionalVisual != null)
            leftDirectionalVisual.SetActive(NoteType == NoteType.DirectionalLeft);

        if (rightDirectionalVisual != null)
            rightDirectionalVisual.SetActive(NoteType == NoteType.DirectionalRight);

        // Update color for color-type notes
        if (NoteType == NoteType.Color && colorNoteVisual != null)
        {
            var spriteRenderer = colorNoteVisual.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color c = UnityEngine.Color.white;
                switch (Color)
                {
                    case GameColor.ColorA: c = UnityEngine.Color.red; break;
                    case GameColor.ColorB: c = UnityEngine.Color.green; break;
                    case GameColor.ColorC: c = UnityEngine.Color.blue; break;
                }
                spriteRenderer.color = c;
            }
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
