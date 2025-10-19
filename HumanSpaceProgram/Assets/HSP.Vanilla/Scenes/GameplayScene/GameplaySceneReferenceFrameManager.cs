using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public class GameplaySceneReferenceFrameManager : SceneReferenceFrameManager
    {
        private static GameplaySceneReferenceFrameManager __instance;
        public static GameplaySceneReferenceFrameManager Instance
        {
            get
            {
                if( __instance == null )
                    SingletonMonoBehaviourUtils.InstanceExists<GameplaySceneReferenceFrameManager>( out __instance );
                
                return __instance; // may be null.
            }
        }

        public static IReferenceFrame ReferenceFrame
        {
            get => Instance.referenceFrame;
            set => Instance.referenceFrame = value;
        }
        public static IReferenceFrame PendingReferenceFrame
        {
            get => Instance.pendingReferenceFrame;
        }

        public static IReferenceFrameTransform TargetObject
        {
            get { return Instance.targetObject; }
            set { Instance.targetObject = value; }
        }

        public static void RequestSceneReferenceFrameSwitch( IReferenceFrame referenceFrame )
        {
            Instance.RequestReferenceFrameSwitch( referenceFrame );
        }
    }
}