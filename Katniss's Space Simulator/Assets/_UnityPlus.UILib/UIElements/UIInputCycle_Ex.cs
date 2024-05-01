using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIInputCycle_Ex
    {
        public static T UIInputCycle<T, TValue>( this T parent, UILayoutInfo layoutInfo, Sprite background, out UIInputCycle<TValue> uiInputRadio ) where T : IUIElementContainer
        {
            uiInputRadio = UIInputCycle<TValue>.Create<UIInputCycle<TValue>>( parent, layoutInfo, background );
            return parent;
        }

        public static UIInputCycle<TValue> AddInputCycle<TValue>( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIInputCycle<TValue>.Create<UIInputCycle<TValue>>( parent, layoutInfo, background );
        }
    }

    public partial class UIInputCycle<TValue>
    {
        public UIInputCycle<TValue> WithValues( params (TValue, Sprite)[] values )
        {
            this.options = values;
            return this;
        }

        public UIInputCycle<TValue> WithValues( IEnumerable<(TValue, Sprite)> values )
        {
            this.options = values.ToArray();
            return this;
        }
    }
}