﻿using KSS.Core;
using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class UITextReadout_Altitude : UIText
    {
        void LateUpdate()
        {
            var activeObj = ActiveObjectManager.ActiveObject?.GetComponent<RootObjectTransform>();
            if( activeObj == null )
            {
                this.Text = "";
            }
            else
            {
                CelestialBody body = CelestialBodyManager.Get( "main" );
                Vector3Dbl posV = activeObj.AIRFPosition;
                Vector3Dbl posCB = body.AIRFPosition;

                double magn = (posV - posCB).magnitude;
                double alt = magn - body.Radius;

                this.Text = $"Altitude: {(alt / 1000.0):#0} km";
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