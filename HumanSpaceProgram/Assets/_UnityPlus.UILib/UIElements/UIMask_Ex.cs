using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIMask_Ex
    {
        public static T WithMask<T>( this T parent, UILayoutInfo layoutInfo, Sprite background, out UIMask uiMask ) where T : IUIElementContainer
        {
            uiMask = UIMask.Create<UIMask>( parent, layoutInfo, background );
            return parent;
        }

        public static UIMask AddMask( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIMask.Create<UIMask>( parent, layoutInfo, background );
        }
    }
}