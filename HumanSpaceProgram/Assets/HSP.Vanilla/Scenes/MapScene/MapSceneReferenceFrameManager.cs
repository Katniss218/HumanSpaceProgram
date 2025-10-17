using HSP.ReferenceFrames;

namespace HSP.Vanilla.Scenes.MapScene
{
    public class MapSceneReferenceFrameManager : SceneReferenceFrameManager
    {
#warning TODO - add auto-instance setting
        public static MapSceneReferenceFrameManager Instance;

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