using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
public class Creditpanelui : MonoBehaviour
{
    [Header("关闭按钮")]
    public Button closeButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    // Update is called once per frame
    public void Close()
    {
        gameObject.SetActive(false);
        Debug.Log("[Settings] 设置面板已关闭");
    }
    void Update()
    {
        
    }
}
