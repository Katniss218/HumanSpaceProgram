using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UITooltip_Ex
    {
        public static UITooltip CreateTooltip( this RectTransform track, UICanvas tooltipMenuCanvas, UILayoutInfo layoutInfo, Sprite background )
        {
            return UITooltip.Create<UITooltip>( track, tooltipMenuCanvas, layoutInfo, background );
        }
    }

    public partial class UITooltip
    {
        public UITooltip WithTint( Color tint )
        {
            this.backgroundComponent.color = tint;
            return this;
        }
    }
}