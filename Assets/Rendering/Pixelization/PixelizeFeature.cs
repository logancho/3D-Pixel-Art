using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelizeFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CustomPassSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public int screenHeight = 144;
        public int pixelSize = 3;
    }

    [SerializeField] private CustomPassSettings settings;

    public class PixelizePass : ScriptableRenderPass
    {
        public PixelizeFeature.CustomPassSettings settings;

        private RenderTargetIdentifier colorBuffer, pixelBuffer;
        private int pixelBufferID = Shader.PropertyToID("_PixelBuffer");

        private Material material;
        public int pixelScreenHeight, pixelScreenWidth;

        //Constructor
        public PixelizePass(PixelizeFeature.CustomPassSettings settings)
        {
            this.settings = settings;
            this.renderPassEvent = settings.renderPassEvent;
            if (material == null) material = CoreUtils.CreateEngineMaterial("Hidden/Pixelize");
        }

        //Render Pass Methods:
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            colorBuffer = renderingData.cameraData.renderer.cameraColorTarget;
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

            int w = renderingData.cameraData.camera.pixelWidth;
            //pixelScreenHeight = settings.screenHeight;
            //pixelScreenWidth = (int)(pixelScreenHeight * renderingData.cameraData.camera.aspect + 0.5f);
            pixelScreenHeight = (int)(renderingData.cameraData.camera.pixelHeight / settings.pixelSize + 0.5f);
            pixelScreenWidth = (int)(renderingData.cameraData.camera.pixelWidth / settings.pixelSize + 0.5f);

            material.SetVector("_BlockCount", new Vector2(pixelScreenWidth, pixelScreenHeight));
            material.SetVector("_BlockSize", new Vector2(1.0f / pixelScreenWidth, 1.0f / pixelScreenHeight));
            material.SetVector("_HalfBlockSize", new Vector2(0.5f / pixelScreenWidth, 0.5f / pixelScreenHeight));

            descriptor.height = pixelScreenHeight;
            descriptor.width = pixelScreenWidth;

            cmd.GetTemporaryRT(pixelBufferID, descriptor, FilterMode.Point);
            pixelBuffer = new RenderTargetIdentifier(pixelBufferID);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("Pixelize Pass")))
            {
                Blit(cmd, colorBuffer, pixelBuffer, material);
                Blit(cmd, pixelBuffer, colorBuffer);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null) throw new System.ArgumentNullException("cmd");
            cmd.ReleaseTemporaryRT(pixelBufferID);
        }
    }

    //Make m_ScriptablePass public so that we can read its variables from our camera
    public PixelizePass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new PixelizePass(settings);
        //{
        //    // Configures where the render pass should be injected.
        //    //renderPassEvent = RenderPassEvent.AfterRenderingOpaques
        //    //RenderPassEvent.
        //    //renderPassEvent = settings.renderPassEvent;
        //};
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


