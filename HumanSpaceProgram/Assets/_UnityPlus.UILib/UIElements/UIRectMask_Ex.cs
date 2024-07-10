using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIRectMask_Ex
    {
        public static T WithRectMask<T>( this T parent, UILayoutInfo layoutInfo, out UIRectMask uiMask ) where T : IUIElementContainer
        {
            uiMask = UIRectMask.Create<UIRectMask>( parent, layoutInfo );
            return parent;
        }

        public static UIRectMask AddRectMask( this IUIElementContainer parent, UILayoutInfo layoutInfo )
        {
            return UIRectMask.Create<UIRectMask>( parent, layoutInfo );
        }
    }
}