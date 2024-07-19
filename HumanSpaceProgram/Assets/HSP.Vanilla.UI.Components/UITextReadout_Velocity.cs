using HSP.Core.Physics;
using HSP.ReferenceFrames;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    public class UITextReadout_Velocity : UIText
    {
        void LateUpdate()
        {
            var physObj = ActiveObjectManager.ActiveObject == null
                ? null
                : ActiveObjectManager.ActiveObject.GetComponent<FreePhysicsObject>();

            this.Text = physObj == null ? "" : $"{physObj.Velocity.magnitude:#0} m/s";
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