using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using UnityEngine;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    public class UITextReadout_Altitude : UIText
    {
        void LateUpdate()
        {
            var activeObj = ActiveVesselManager.ActiveVessel == null
                ? null
                : ActiveVesselManager.ActiveVessel.ReferenceFrameTransform;

            if( activeObj == null )
            {
                this.Text = "";
            }
            else
            {
                CelestialBody body = CelestialBodyManager.Get( "main" );
                Vector3Dbl bodySpacePosition = body.ReferenceFrameTransform.CenteredInertialReferenceFrame().InverseTransformPosition( activeObj.AbsolutePosition );

                double alt = bodySpacePosition.magnitude - body.Radius;

                this.Text = $"{(alt / 1000.0):#0.#} km";
            }
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UITextReadout_Altitude
        {
            return UIText.Create<T>( parent, layout, "<placeholder_text>" );
        }
    }

    public static class UITextReadout_Altitude_Ex
    {
        public static UITextReadout_Altitude AddTextReadout_Altitude( this IUIElementContainer parent, UILayoutInfo layout )
        {
            return UITextReadout_Altitude.Create<UITextReadout_Altitude>( parent, layout );
        }
    }
}