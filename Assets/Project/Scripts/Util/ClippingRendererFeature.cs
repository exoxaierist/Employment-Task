// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;
//
// public class ClippingRendererFeature : ScriptableRendererFeature
// {
//     public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
//     public Material clippingMaterial;
//     public LayerMask wallObjectLayer;
//
//     private ClippingRenderPass clippingRenderPass;
//
//     public override void Create()
//     {
//         clippingRenderPass = new ClippingRenderPass(renderPassEvent, clippingMaterial, wallObjectLayer);
//     }
//
//     public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//     {
//         renderer.EnqueuePass(clippingRenderPass);
//     }
//
//     class ClippingRenderPass : ScriptableRenderPass
//     {
//         private Material clippingMaterial;
//         private LayerMask wallObjectLayer;
//         private RenderTargetIdentifier colorTarget;
//         private RenderTargetHandle tempTexture;
//
//         public ClippingRenderPass(RenderPassEvent renderPassEvent, Material material, LayerMask wallLayer)
//         {
//             this.renderPassEvent = renderPassEvent;
//             clippingMaterial = material;
//             wallObjectLayer = wallLayer;
//             tempTexture.Init("_TempClippingTexture");
//         }
//
//         public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//         {
//             if (clippingMaterial == null)
//                 return;
//
//             CommandBuffer cmd = CommandBufferPool.Get("Clipping Pass");
//             RenderTargetIdentifier source = renderingData.cameraData.renderer.cameraColorTarget;
//             colorTarget = source;
//
//             // 임시 텍스처 생성
//             cmd.GetTemporaryRT(tempTexture.id, renderingData.cameraData.camera.scaledPixelWidth, 
//                 renderingData.cameraData.camera.scaledPixelHeight, 0, FilterMode.Bilinear);
//
//             // 소스를 임시 텍스처로 복사
//             cmd.Blit(source, tempTexture.Identifier());
//
//             // 클리핑 머티리얼을 사용하여 벽 오브젝트 마스킹
//             cmd.SetGlobalTexture("_PreviousFrameTexture", tempTexture.Identifier());
//             cmd.Blit(tempTexture.Identifier(), source, clippingMaterial);
//
//             // 임시 텍스처 해제
//             cmd.ReleaseTemporaryRT(tempTexture.id);
//
//             context.ExecuteCommandBuffer(cmd);
//             CommandBufferPool.Release(cmd);
//         }
//     }
// }