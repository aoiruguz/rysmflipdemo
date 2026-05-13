using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXSpriteSetup : MonoBehaviour
{
    [Header("数据来源：拖入另一个带有 Sprite 的物体")]
    public GameObject spriteSource;

    [Header("识别出的 Sprite (自动填充)")]
    public Sprite targetSprite;
    
    private VisualEffect vfx;

    // 当你在 Inspector 里拖入物体时立即识别
    private void OnValidate()
    {
        FetchSpriteFromSource();
    }

    private void FetchSpriteFromSource()
    {
        if (spriteSource == null) return;

        // 尝试从来源物体获取 SpriteRenderer 或 Image
        var sr = spriteSource.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            targetSprite = sr.sprite;
        }
        else
        {
            var img = spriteSource.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                targetSprite = img.sprite;
            }
        }
    }

    void Start()
    {
        vfx = GetComponent<VisualEffect>();
        ApplySpriteToVFX();
    }

    [ContextMenu("Apply Sprite Data")]
    public void ApplySpriteToVFX()
    {
        // 刷新一下引用，防止来源物体的 Sprite 变了
        FetchSpriteFromSource();

        if (targetSprite == null || vfx == null) return;

        string texProperty = "SpriteTexture"; 
        string scaleProperty = "SpriteScale";

        vfx.SetTexture(texProperty, targetSprite.texture);

        float physicalWidth = targetSprite.rect.width / targetSprite.pixelsPerUnit;
        float physicalHeight = targetSprite.rect.height / targetSprite.pixelsPerUnit;

        vfx.SetVector3(scaleProperty, new Vector3(physicalWidth, physicalHeight, 1f));
    }
}
