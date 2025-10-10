using HSP.ReferenceFrames;

namespace HSP.Vanilla.Scenes.MapScene
{
    public class MapSceneReferenceFrameManager : SceneReferenceFrameManager
    {
        public static MapSceneReferenceFrameManager Instance;

        public static IReferenceFrame ReferenceFrame
        {
            get => Instance.referenceFrame;
            set => Instance.referenceFrame = value;
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