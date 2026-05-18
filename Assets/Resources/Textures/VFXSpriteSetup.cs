using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXSpriteSetup : MonoBehaviour
{
    private VisualEffect vfx;

    // --- 供外部调用，动态替换拖尾贴图 ---
    public void SetDynamicSprite(Sprite newSprite)
    {
        if (vfx == null) vfx = GetComponent<VisualEffect>();
        if (newSprite == null || vfx == null) return;

        string texProperty = "SpriteTexture"; 
        string scaleProperty = "SpriteScale";

        vfx.SetTexture(texProperty, newSprite.texture);

        float physicalWidth = newSprite.rect.width / newSprite.pixelsPerUnit;
        float physicalHeight = newSprite.rect.height / newSprite.pixelsPerUnit;

        vfx.SetVector3(scaleProperty, new Vector3(physicalWidth, physicalHeight, 1f));

        // 传递父物体的世界坐标
        string posProperty = "WorldPos";
        Vector3 worldPos = transform.parent != null ? transform.parent.position : transform.position;
        vfx.SetVector3(posProperty, worldPos);
    }
}
