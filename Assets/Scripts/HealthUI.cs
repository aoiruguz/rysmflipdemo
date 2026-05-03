using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance { get; private set; }

    [Header("Heart UI References")]
    [Tooltip("Array of 3 heart Image components (left to right)")]
    public Image[] heartImages;

    [Header("Heart Sprites")]
    [Tooltip("Sprite for full heart (5/5)")]
    public Sprite heartFull;

    [Tooltip("Sprite for 4/5 heart")]
    public Sprite heart4;

    [Tooltip("Sprite for 3/5 heart")]
    public Sprite heart3;

    [Tooltip("Sprite for 2/5 heart")]
    public Sprite heart2;

    [Tooltip("Sprite for 1/5 heart")]
    public Sprite heart1;

    [Tooltip("Sprite for empty heart (0/5)")]
    public Sprite heartEmpty;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Initialize with full health
        UpdateHealth(15, 15);
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (heartImages == null || heartImages.Length != 3)
        {
            Debug.LogWarning("[HealthUI] Heart images array must contain exactly 3 images!");
            return;
        }

        // Each heart represents 5 health points
        int segmentsPerHeart = 5;

        for (int i = 0; i < 3; i++)
        {
            // Calculate how much health this heart should show
            int heartStartHealth = i * segmentsPerHeart;
            int heartEndHealth = (i + 1) * segmentsPerHeart;

            int healthInThisHeart = Mathf.Clamp(currentHealth - heartStartHealth, 0, segmentsPerHeart);

            // Set the appropriate sprite
            heartImages[i].sprite = GetHeartSprite(healthInThisHeart);
        }
    }

    private Sprite GetHeartSprite(int segments)
    {
        switch (segments)
        {
            case 5: return heartFull;
            case 4: return heart4;
            case 3: return heart3;
            case 2: return heart2;
            case 1: return heart1;
            case 0: return heartEmpty;
            default: return heartEmpty;
        }
    }
}
