using UnityEngine;

/// <summary>
/// CRT 复古显示器效果
/// 挂载到相机上，自动应用 CRT Shader 到整个屏幕
/// </summary>
[RequireComponent(typeof(Camera))]
public class CRTEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [Tooltip("是否启用 CRT 效果")]
    public bool enableEffect = true;

    private Material crtMaterial;

    private void Awake()
    {
        // 自动创建材质
        Shader crtShader = Shader.Find("Hidden/CRTEffect");
        if (crtShader != null)
        {
            crtMaterial = new Material(crtShader);
            crtMaterial.hideFlags = HideFlags.HideAndDontSave;
            Debug.Log("[CRTEffect] CRT Shader loaded successfully");
        }
        else
        {
            Debug.LogError("[CRTEffect] CRT Shader not found! Make sure CRTEffectSimple.shader exists.");
            enabled = false;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (crtMaterial != null && enableEffect)
        {
            // 应用 CRT 效果
            Graphics.Blit(source, destination, crtMaterial);
        }
        else
        {
            // 不应用效果，直接输出
            Graphics.Blit(source, destination);
        }
    }

    private void OnDestroy()
    {
        if (crtMaterial != null)
        {
            DestroyImmediate(crtMaterial);
        }
    }
}
