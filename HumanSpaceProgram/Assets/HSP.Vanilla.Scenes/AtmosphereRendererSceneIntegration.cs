using HSP.Core;
using HSP.GameplayScene.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace HSP.CelestialBodies
{
    public static class AtmosphereRendererSceneIntegration
    {
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.gameplayscene_postprocessing", After = new[] { "vanilla.gameplayscene_camera" } )]
        private static void CreateAtmosphereRenderer()
        {
            AtmosphereRenderer atmosphereRenderer = GameplaySceneCameraManager.EffectCamera.gameObject.AddComponent<AtmosphereRenderer>();
            atmosphereRenderer.light = GameObject.Find( "CBLight" ).GetComponent<Light>();
            atmosphereRenderer.ColorRenderTextureGetter = () => GameplaySceneCameraManager.ColorRenderTexture;
            atmosphereRenderer.DepthRenderTextureGetter = () => GameplaySceneDepthBufferCombiner.CombinedDepthRenderTexture;
        }
    }
}