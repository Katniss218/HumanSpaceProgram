using HSP.ReferenceFrames;
using HSP.Trajectories;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    public class UITextReadout_Velocity : UIText
    {
        void LateUpdate()
        {
            var physObj = ActiveVesselManager.ActiveObject == null
                ? null
                : ActiveVesselManager.ActiveObject.GetComponent<IReferenceFrameTransform>();

#warning TODO - absolute is not very useful. Also, have a selarate field for what should be tracked and change it in the event. that would be better.
            this.Text = physObj == null ? "" : $"{physObj.AbsoluteVelocity.magnitude:#0} m/s";
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UITextReadout_Velocity
        {
            return UIText.Create<T>( parent, layout, "<placeholder_text>" );
        }
    }

    public static class UITextReadout_Velocity_Ex
    {
        public static UITextReadout_Velocity AddTextReadout_Velocity( this IUIElementContainer parent, UILayoutInfo layout )
        {
            return UITextReadout_Velocity.Create<UITextReadout_Velocity>( parent, layout );
        }
    }
}