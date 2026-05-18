using UnityEngine;

/// <summary>
/// 控制 PlayScene 中的背景材质参数
/// 根据当前关卡的 LevelData 配置应用材质参数
/// </summary>
public class BackgroundMaterialController : MonoBehaviour
{
    [Header("材质引用")]
    [Tooltip("要控制的背景材质（通常是 UI_BG_normal）")]
    public Material backgroundMaterial;



    private void Start()
    {


        // 应用关卡材质配置
        ApplyLevelMaterialSettings();
    }

    /// <summary>
    /// 应用当前关卡的材质配置
    /// </summary>
    private void ApplyLevelMaterialSettings()
    {
        if (backgroundMaterial == null)
        {
            Debug.LogWarning("[BackgroundMaterialController] Background material is null!");
            return;
        }

        // 从 LevelUIManager 获取当前关卡数据
        LevelData currentLevel = LevelUIManager.GetCurrentLevelData();

        if (currentLevel == null)
        {
            Debug.LogWarning("[BackgroundMaterialController] No level data found, using default material settings");
            return;
        }

        // 应用材质参数
        BackgroundMaterialSettings settings = currentLevel.backgroundMaterial;

        backgroundMaterial.SetFloat("_WobbleSpeed", settings.wobbleSpeed);
        backgroundMaterial.SetFloat("_WobbleAmount", settings.wobbleAmount);
        backgroundMaterial.SetFloat("_Speed", settings.expansionSpeed);
        backgroundMaterial.SetFloat("_RingDensity", settings.ringDensity);

        backgroundMaterial.SetColor("_Color1", settings.color1);
        backgroundMaterial.SetColor("_Color2", settings.color2);
        backgroundMaterial.SetColor("_Color3", settings.color3);
        backgroundMaterial.SetColor("_Color4", settings.color4);
        backgroundMaterial.SetColor("_Color5", settings.color5);

        // 设置效果纹理参数
        if (settings.effectTexture != null)
        {
            backgroundMaterial.SetTexture("_EffectTex", settings.effectTexture);
        }
        backgroundMaterial.SetFloat("_EffectFrameCount", settings.effectFrameCount);

        Debug.Log($"[BackgroundMaterialController] Applied material settings for level: {currentLevel.levelName}");
    }

    /// <summary>
    /// 手动应用指定的材质配置（供外部调用）
    /// </summary>
    public void ApplySettings(BackgroundMaterialSettings settings)
    {
        if (backgroundMaterial == null)
        {
            Debug.LogWarning("[BackgroundMaterialController] Background material is null!");
            return;
        }

        backgroundMaterial.SetFloat("_WobbleSpeed", settings.wobbleSpeed);
        backgroundMaterial.SetFloat("_WobbleAmount", settings.wobbleAmount);
        backgroundMaterial.SetFloat("_Speed", settings.expansionSpeed);
        backgroundMaterial.SetFloat("_RingDensity", settings.ringDensity);

        backgroundMaterial.SetColor("_Color1", settings.color1);
        backgroundMaterial.SetColor("_Color2", settings.color2);
        backgroundMaterial.SetColor("_Color3", settings.color3);
        backgroundMaterial.SetColor("_Color4", settings.color4);
        backgroundMaterial.SetColor("_Color5", settings.color5);

        // 设置效果纹理参数
        if (settings.effectTexture != null)
        {
            backgroundMaterial.SetTexture("_EffectTex", settings.effectTexture);
        }
        backgroundMaterial.SetFloat("_EffectFrameCount", settings.effectFrameCount);
    }

    private void OnDestroy()
    {
        // 可选：在退出时重置材质为默认值
        // 如果不需要重置，可以删除这个方法
    }
}
