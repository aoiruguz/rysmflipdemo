using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class BlinkEffect : ScriptableRendererFeature
{
    [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;
    [SerializeField] private Shader blinkShader;
    
    private BlinkPass blinkPass;
    private Material blinkMaterial;

    public override void Create()
    {
        if (blinkShader == null)
        {
            blinkShader = Shader.Find("Custom/BlinkEffect");
        }
        
        if (blinkShader == null)
        {
            Debug.LogError("BlinkEffect: Shader 'Custom/BlinkEffect' not found!");
            return;
        }

        blinkMaterial = CoreUtils.CreateEngineMaterial(blinkShader);
        blinkPass = new BlinkPass(blinkMaterial, injectionPoint);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var stack = VolumeManager.instance.stack;
        var comp = stack.GetComponent<BlinkEffectVolume>();
        
        // Return if not active
        if (comp == null || !comp.IsActive())
            return;

        if (blinkPass != null && blinkMaterial != null)
        {
            renderer.EnqueuePass(blinkPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            blinkPass?.Dispose();
            CoreUtils.Destroy(blinkMaterial);
        }
    }

    class BlinkPass : ScriptableRenderPass
    {
        private Material blinkMaterial;
        private RTHandle tempRTHandle;
        
        private static readonly int BlinkStrengthID = Shader.PropertyToID("_BlinkStrength");
        private static readonly int EdgeBlurID = Shader.PropertyToID("_EdgeBlur");
        private static readonly int EyelidColorID = Shader.PropertyToID("_EyelidColor");
        private static readonly int BlendAmountID = Shader.PropertyToID("_BlendAmount");

        public BlinkPass(Material material, RenderPassEvent injectionPoint)
        {
            blinkMaterial = material;
            renderPassEvent = injectionPoint;
            
            // Required for compatibility mode
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        // ==========================================
        // RENDER GRAPH API (Unity 6 / URP 17+)
        // ==========================================
        
        private class PassData
        {
            public Material material;
            public TextureHandle src;
            public float blinkStrength;
            public float edgeBlur;
            public Color eyelidColor;
            public float blendAmount;
        }

        private class CopyPassData
        {
            public TextureHandle src;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var stack = VolumeManager.instance.stack;
            var volumeComponent = stack.GetComponent<BlinkEffectVolume>();
            
            if (volumeComponent == null || !volumeComponent.IsActive() || blinkMaterial == null)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            TextureHandle srcCamColor = resourceData.activeColorTexture;
            if (!srcCamColor.IsValid())
                return;

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            
            // Create texture descriptor for Render Graph
            TextureDesc tempDesc = new TextureDesc(desc.width, desc.height)
            {
                colorFormat = desc.graphicsFormat,
                depthBufferBits = 0,
                name = "_BlinkTempRT"
            };
            
            TextureHandle tempTexture = renderGraph.CreateTexture(tempDesc);

            // Pass 1: Render Blink Effect to Temp Texture
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Blink Effect Pass", out var passData))
            {
                passData.material = blinkMaterial;
                passData.src = srcCamColor;
                passData.blinkStrength = volumeComponent.blinkStrength.value;
                passData.edgeBlur = volumeComponent.edgeBlur.value;
                passData.eyelidColor = volumeComponent.eyelidColor.value;
                passData.blendAmount = volumeComponent.blendAmount.value;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.SetRenderAttachment(tempTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.material.SetFloat(BlinkStrengthID, data.blinkStrength);
                    data.material.SetFloat(EdgeBlurID, data.edgeBlur);
                    data.material.SetColor(EyelidColorID, data.eyelidColor);
                    data.material.SetFloat(BlendAmountID, data.blendAmount);

                    Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            // Pass 2: Copy Temp Texture back to Camera Target
            using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("Blink Effect Copy Back", out var passData))
            {
                passData.src = tempTexture;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.SetRenderAttachment(srcCamColor, 0, AccessFlags.Write);

                builder.SetRenderFunc((CopyPassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), 0.0f, false);
                });
            }
        }

        // ==========================================
        // COMPATIBILITY API (Non-Render Graph Fallback)
        // ==========================================

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempRTHandle, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_BlinkTempRT");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var stack = VolumeManager.instance.stack;
            var volumeComponent = stack.GetComponent<BlinkEffectVolume>();
            
            if (volumeComponent == null || !volumeComponent.IsActive() || blinkMaterial == null)
                return;

            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            if (cameraColorTarget == null || cameraColorTarget.rt == null)
            {
                Debug.LogWarning("BlinkEffect 跳过渲染: cameraColorTarget 为 null (Compatibility Mode)");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("Blink Effect");

            blinkMaterial.SetFloat(BlinkStrengthID, volumeComponent.blinkStrength.value);
            blinkMaterial.SetFloat(EdgeBlurID, volumeComponent.edgeBlur.value);
            blinkMaterial.SetColor(EyelidColorID, volumeComponent.eyelidColor.value);
            blinkMaterial.SetFloat(BlendAmountID, volumeComponent.blendAmount.value);

            Blitter.BlitCameraTexture(cmd, cameraColorTarget, tempRTHandle, blinkMaterial, 0);
            Blitter.BlitCameraTexture(cmd, tempRTHandle, cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            tempRTHandle?.Release();
        }
    }
}
