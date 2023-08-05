using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public class UIContextMenu : UIElement
    {
        public readonly ContextMenu contextMenuComponent;
        public readonly Image backgroundComponent;

        public UIContextMenu( RectTransform transform, ContextMenu contextMenuComponent, Image backgroundComponent ) : base( transform )
        {
            this.contextMenuComponent = contextMenuComponent;
            this.backgroundComponent = backgroundComponent;
        }
    }
}