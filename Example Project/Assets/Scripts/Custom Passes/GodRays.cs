using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

// https://www.raywenderlich.com/22027819-volumetric-light-scattering-as-a-custom-renderer-feature-in-urp

[System.Serializable]
public class GodRaysSettings
{
    [Header("Properties")]
    [Range(0.1f, 1f)]
    public float resolutionScale = 0.5f;

    [Range(0.0f, 1.0f)]
    public float intensity = 1.0f;

    [Range(0.0f, 1.0f)]
    public float blurWidth = 0.85f;
}

public class GodRays : ScriptableRendererFeature
{
    class LightScatterPass : ScriptableRenderPass
    {

        private readonly RenderTargetHandle occluders = RenderTargetHandle.CameraTarget;
        private readonly float resolutionScale;
        private readonly float intensity;
        private readonly float blurWidth;

        private readonly Material occludersMaterial;

        private readonly List<ShaderTagId> shaderTagIdList = new List<ShaderTagId>();

        private FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        private readonly Material radialBlurMaterial;

        private RenderTargetIdentifier cameraColorTargetIdent;

        public LightScatterPass(GodRaysSettings settings)
        {
            occluders.Init("_OccludersMap");
            resolutionScale = settings.resolutionScale;
            intensity = settings.intensity;
            blurWidth = settings.blurWidth;

            occludersMaterial = new Material(Shader.Find("Hidden/UnlitColour"));

            shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            shaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
            shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
            shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));

            radialBlurMaterial = new Material(Shader.Find("Hidden/RadialBlur"));
        }

        public void SetCameraColorTarget(RenderTargetIdentifier cameraColorTargetIdent)
        {
            //this.cameraColorTargetIdent = cameraColorTargetIdent;
            this.cameraColorTargetIdent = new RenderTargetIdentifier("_CameraColorAttachmentA");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;

            cameraTextureDescriptor.depthBufferBits = 0;

            cameraTextureDescriptor.width = Mathf.RoundToInt(cameraTextureDescriptor.width * resolutionScale);
            cameraTextureDescriptor.height = Mathf.RoundToInt(cameraTextureDescriptor.height * resolutionScale);

            cmd.GetTemporaryRT(occluders.id, cameraTextureDescriptor, FilterMode.Bilinear);

            ConfigureTarget(occluders.Identifier());
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!occludersMaterial || !radialBlurMaterial)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("GodRays")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                Camera camera = renderingData.cameraData.camera;
                context.DrawSkybox(camera);

                DrawingSettings drawSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, SortingCriteria.CommonOpaque);
                drawSettings.overrideMaterial = occludersMaterial;

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);

                if (RenderSettings.sun != null)
                {
                    Vector3 sunDirectionWorldSpace = RenderSettings.sun.transform.forward;
                    Vector3 cameraPositionWorldSpace = camera.transform.position;
                    Vector3 sunPositionWorldSpace = cameraPositionWorldSpace + sunDirectionWorldSpace;
                    /*
                    Vector3 sunPositionScreenSpace = camera.WorldToScreenPoint(sunPositionWorldSpace);

                    float width = camera.pixelWidth, height = camera.pixelHeight;

                    if (sunPositionScreenSpace.z > 0f)
                        sunPositionScreenSpace = -sunPositionScreenSpace;

                    sunPositionScreenSpace.x = Mathf.Clamp(sunPositionScreenSpace.x, 0, width);
                    sunPositionScreenSpace.y = Mathf.Clamp(sunPositionScreenSpace.y, 0, height);
                    */

                    Vector3 sunPositionViewportSpace = camera.WorldToViewportPoint(sunPositionWorldSpace);
                    //Vector3 sunPositionViewportSpace = camera.ScreenToViewportPoint(sunPositionScreenSpace);

                    //Debug.Log("Cam screen: " + sunPositionScreenSpace);
                    // z of screen space is dot product, kinda cool

                    float dot = Vector3.Dot(sunDirectionWorldSpace, camera.transform.forward);
                    // positive when looking away from sun

                    dot = Mathf.Max(-dot, 0);

                    //if (dot < 0)
                    //{
                    radialBlurMaterial.SetFloat("_Dot", dot);
                    radialBlurMaterial.SetVector("_Center", new Vector4(sunPositionViewportSpace.x, sunPositionViewportSpace.y, 0, 0));
                    radialBlurMaterial.SetFloat("_Intensity", intensity);
                    radialBlurMaterial.SetFloat("_BlurWidth", blurWidth);
                }

                Blit(cmd, occluders.Identifier(), cameraColorTargetIdent, radialBlurMaterial);
                //}
                //cmd.Blit(occluders.Identifier(), cameraColorTargetIdent, radialBlurMaterial);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(occluders.id);
        }
    }

    LightScatterPass m_ScriptablePass;
    public GodRaysSettings settings = new GodRaysSettings();

    public override void Create()
    {
        m_ScriptablePass = new LightScatterPass(settings);

        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
        m_ScriptablePass.SetCameraColorTarget(renderer.cameraColorTarget);
    }
}


