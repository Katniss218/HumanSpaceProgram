﻿using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using UnityEngine;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    public class UITextReadout_Velocity : UIText
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
                CelestialBody closestBody = CelestialBodyManager.GetClosest( activeObj.AbsolutePosition );
                Vector3Dbl bodySpaceVelocity = closestBody.ReferenceFrameTransform.CenteredInertialReferenceFrame().InverseTransformVelocity( activeObj.AbsoluteVelocity );

                double vel = bodySpaceVelocity.magnitude;

                this.Text = $"{vel:#0} m/s";
            }
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