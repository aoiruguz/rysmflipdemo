using UnityEngine;
using UnityEngine.UI;

public class SettingsButtonHandler : MonoBehaviour
{
    public SongSelectionSettingsUI settingsUI;

    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OpenSettings);
        }
    }

    public void OpenSettings()
    {
        if (settingsUI != null)
        {
            settingsUI.Open();
        }
        else
        {
            Debug.LogError("SettingsUI reference is not set!");
        }
    }
}
