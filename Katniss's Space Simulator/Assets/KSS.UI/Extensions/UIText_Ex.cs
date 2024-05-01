using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.UILib.UIElements;
using UnityPlus.UILib;
using UnityPlus.AssetManagement;
using UnityEngine;

namespace KSS.UI
{
    public static class UIText_Ex
    {
        /// <summary>
        /// Adds a text UI element with the default font.
        /// </summary>
        public static T WithStdText<T>( this T parent, UILayoutInfo layoutInfo, string text, out UIText uiText ) where T : IUIElementContainer
        {
            uiText = parent.AddStdText( layoutInfo, text );
            return parent;
        }

        /// <summary>
        /// Adds a text UI element with the default font.
        /// </summary>
        public static UIText AddStdText( this IUIElementContainer parent, UILayoutInfo layoutInfo, string text )
        {
            return parent.AddText( layoutInfo, text )
                .WithFont( UIDefaults.TextFont, UIDefaults.TextFontSize, UIDefaults.TextFontColor );
        }
    }
}