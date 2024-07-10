using HSP.Core;
using HSP.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.UI
{
    public class UITextReadout_Acceleration : UIText
    {
        void LateUpdate()
        {
            var physObj = ActiveObjectManager.ActiveObject?.GetComponent<FreePhysicsObject>();

            this.Text = physObj == null ? "" : $"Acceleration: {physObj.Acceleration.magnitude:#0.0} m/s^2";
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