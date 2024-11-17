using HSP.ReferenceFrames;
using HSP.Vanilla.Components;
using UnityEngine;

namespace HSP.Vanilla
{
    public class OnActiveVesselChanged : MonoBehaviour
    {
        public const string TRY_SET_SCENE_REFERENCE_FRAME = HSPEvent.NAMESPACE_HSP + ".try_set_scene_reference_frame";
        public const string TRY_SET_DEFAULT_CONTROL_FRAME = HSPEvent.NAMESPACE_HSP + ".try_set_default_control_frame";

        [HSPEventListener( HSPEvent_AFTER_ACTIVE_VESSEL_CHANGED.ID, TRY_SET_SCENE_REFERENCE_FRAME )]
        private static void TrySetSceneReferenceFrame()
        {
            SceneReferenceFrameManager.TargetObject = ActiveVesselManager.ActiveVessel == null
                ? null
                : ActiveVesselManager.ActiveVessel.ReferenceFrameTransform;
        }

        [HSPEventListener( HSPEvent_AFTER_ACTIVE_VESSEL_CHANGED.ID, TRY_SET_DEFAULT_CONTROL_FRAME )]
        private static void TrySetDefaultControlFrame()
        {
            if( ActiveVesselManager.ActiveVessel == null )
            {
                SelectedControlFrameManager.ControlFrame = null;
                return;
            }

            FControlFrame frame = ActiveVesselManager.ActiveVessel.GetComponentInChildren<FControlFrame>();
            SelectedControlFrameManager.ControlFrame = frame; // May be null and that's okay.
        }
    }
}