using HSP.Core;
using HSP.GameplayScene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Scenes.GameplayScene
{
    public static class TimeScaleInputControllerFactory
    {
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".add_timescale_icontroller" )]
        private static void CreateInstanceInScene()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<TimeScaleInputController>();
        }
    }
}