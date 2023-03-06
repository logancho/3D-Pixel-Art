using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CustomPassSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        [Range(1, 4)] public int downsample = 1;
        [Range(0, 20)] public int blurStrength = 5;
        public RenderTexture test;
    }

    [SerializeField] private CustomPassSettings settings;

    class CustomRenderPass : ScriptableRenderPass
    {
        // The profiler tag that will show up in the frame debugger.
        const string ProfilerTag = "Debug Pass";
        public CustomRenderPassFeature.CustomPassSettings settings;

        RenderTargetIdentifier colorBuffer, temporaryBuffer;
        private int temporaryBufferID = Shader.PropertyToID("_TemporaryBuffer");

        private Material material;

        // Good practice to cache shader property ID here
        static readonly int BlurStrengthProperty = Shader.PropertyToID("_BlurStrength");

        public CustomRenderPass(CustomRenderPassFeature.CustomPassSettings passSettings) 
        {
            this.settings = passSettings;
            renderPassEvent = passSettings.renderPassEvent;

            if (material == null) material = CoreUtils.CreateEngineMaterial("Hidden/DebugShader");
            //material.SetInt(BlurStrengthProperty, passSettings.blurStrength);
        }
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.width /= settings.downsample;
            descriptor.height /= settings.downsample;
            descriptor.depthBufferBits = 0;

            colorBuffer = renderingData.cameraData.renderer.cameraColorTarget;

            cmd.GetTemporaryRT(temporaryBufferID, descriptor, FilterMode.Bilinear);
            temporaryBuffer = new RenderTargetIdentifier(temporaryBufferID);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler(ProfilerTag)))
            {
                // Blit from the color buffer to a temporary buffer and back. This is needed for a two-pass shader.
                //Blit(cmd, colorBuffer, temporaryBuffer, material); // shader pass 0
                Blit(cmd, colorBuffer, settings.test);
                //Blit(cmd, temporaryBuffer, colorBuffer); // shader pass 1
            }

            // Execute the command buffer and release it.
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            cmd.ReleaseTemporaryRT(temporaryBufferID);
        }
    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        // Pass the settings as a parameter to the constructor of the pass.
        m_ScriptablePass = new CustomRenderPass(settings);

        // Configures where the render pass should be injected.
        //m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


