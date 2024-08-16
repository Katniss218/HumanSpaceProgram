using HSP.ReferenceFrames;
using HSP.Trajectories;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    public class UITextReadout_Acceleration : UIText
    {
        void LateUpdate()
        {
            var physObj = ActiveVesselManager.ActiveObject == null
                ? null
                : ActiveVesselManager.ActiveObject.GetComponent<IReferenceFrameTransform>();

#warning TODO - absolute is not very useful.
            this.Text = physObj == null ? "" : $"Acceleration: {physObj.AbsoluteAcceleration.magnitude:#0.0} m/s^2";
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UITextReadout_Acceleration
        {
            return UIText.Create<T>( parent, layout, "<placeholder_text>" );
        }
    }

    public static class UITextReadout_Acceleration_Ex
    {
        public static UITextReadout_Acceleration AddTextReadout_Acceleration( this IUIElementContainer parent, UILayoutInfo layout )
        {
            return UITextReadout_Acceleration.Create<UITextReadout_Acceleration>( parent, layout );
        }
    }
}