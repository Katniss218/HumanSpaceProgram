using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIInputRadio_Ex
    {
        public static T WithInputRadio<T, TValue>( this T parent, UILayoutInfo layoutInfo, Sprite background, Sprite backgroundActive, out UIInputRadio<TValue> uiInputRadio ) where T : IUIElementContainer
        {
            uiInputRadio = UIInputRadio<TValue>.Create<UIInputRadio<TValue>>( parent, layoutInfo, background, backgroundActive );
            return parent;
        }

        public static UIInputRadio<TValue> AddInputRadio<TValue>( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background, Sprite backgroundActive )
        {
            return UIInputRadio<TValue>.Create<UIInputRadio<TValue>>( parent, layoutInfo, background, backgroundActive );
        }


        public static UIInputRadio<bool> AddBooleanInputRadio( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background, Sprite backgroundActive )
        {
            return UIInputRadio<bool>.Create<UIInputRadio<bool>>( parent, layoutInfo, background, backgroundActive );
        }
    }

    public partial class UIInputRadio<TValue>
    {
        public UIInputRadio<TValue> WithValue( TValue value )
        {
            this.value = value;
            return this;
        }

        public UIInputRadio<TValue> WithSprites( Sprite background, Sprite backgroundActive )
        {
            this.DeselectedSprite = background;
            this.SelectedSprite = backgroundActive;
            return this;
        }
    }
}