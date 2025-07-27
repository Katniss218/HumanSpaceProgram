using HSP.ReferenceFrames;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public class GameplaySceneReferenceFrameManager : SceneReferenceFrameManager
    {
#warning TODO - add auto-instance setting
        public static GameplaySceneReferenceFrameManager Instance;

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