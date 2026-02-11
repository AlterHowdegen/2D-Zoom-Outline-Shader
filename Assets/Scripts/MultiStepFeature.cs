using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class MultiStepOutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class FeatureSettings
    {
        public LayerMask targetLayerMask;
        [Tooltip("Shader must output to SV_Target0 and SV_Target1")]
        public Material objectMRTMaterial;
        public Material compositeMaterial;
    }

    public FeatureSettings settings = new FeatureSettings();
    private MultiStepPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new MultiStepPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }

    class MultiStepPass : ScriptableRenderPass
    {
        private FeatureSettings settings;

        public MultiStepPass(FeatureSettings settings)
        {
            this.settings = settings;
            // Using AfterRenderingPostProcessing to ensure we are the final output
            this.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        private class PassData
        {
            public RendererListHandle rendererList;
        }

        private class CompositeData
        {
            public TextureHandle colorTex;
            public TextureHandle shadingTex;
            public Material compositeMaterial;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

            var desc = cameraData.cameraTargetDescriptor;
            // IMPORTANT: RTs must have 0 depth bits to be used as Color Attachments
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;

            // FORCE an Alpha channel format (Standard 8-bit RGBA)
            desc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB;

            // 1. Create temporary textures
            TextureHandle texColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_TexColor", true);
            TextureHandle texShading = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_TexShading", true);

            // 2. Setup Drawing Logic
            SortingSettings sortingSettings = new SortingSettings(cameraData.camera);
            sortingSettings.criteria = SortingCriteria.CommonOpaque;

            DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("UniversalForward"), sortingSettings)
            {
                overrideShader = settings.objectMRTMaterial.shader,
                overrideShaderPassIndex = 0
            };

            var filterSettings = new FilteringSettings(RenderQueueRange.opaque, settings.targetLayerMask);
            var rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filterSettings);
            RendererListHandle rendererList = renderGraph.CreateRendererList(rendererListParams);

            // 3. MRT PASS
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("MRT Render Pass", out var passData))
            {
                passData.rendererList = rendererList;
                builder.UseRendererList(rendererList);

                builder.SetRenderAttachment(texColor, 0);
                builder.SetRenderAttachment(texShading, 1);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(true, true, Color.clear);
                    context.cmd.DrawRendererList(data.rendererList);
                });
            }

            // 4. COMPOSITE PASS
            using (var builder = renderGraph.AddRasterRenderPass<CompositeData>("Composite Outline Pass", out var compositeData))
            {
                compositeData.colorTex = texColor;
                compositeData.shadingTex = texShading;
                compositeData.compositeMaterial = settings.compositeMaterial;

                builder.UseTexture(texColor);
                builder.UseTexture(texShading);

                // Target cameraColor to ensure it's written to the actual screen/backbuffer
                builder.SetRenderAttachment(resourceData.cameraColor, 0);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((CompositeData data, RasterGraphContext context) =>
                {
                    if (data.compositeMaterial == null) return;

                    data.compositeMaterial.SetTexture("_ColorBuffer", data.colorTex);
                    data.compositeMaterial.SetTexture("_ShadingBuffer", data.shadingTex);

                    // The first texture in BlitTexture becomes '_BlitTexture' in Shader Graph
                    Blitter.BlitTexture(context.cmd, new Vector4(1, 1, 0, 0), data.compositeMaterial, 0);
                });
            }
        }
    }
}