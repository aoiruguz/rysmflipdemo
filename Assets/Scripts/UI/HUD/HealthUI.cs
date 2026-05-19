using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance { get; private set; }

    [Header("Heart UI References")]
    [Tooltip("Array of 3 heart Image components (left to right)")]
    public Image[] heartImages;

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

            // Set the fill amount (0 to 1) based on segments left
            heartImages[i].fillAmount = (float)healthInThisHeart / segmentsPerHeart;
        }
    }
}
