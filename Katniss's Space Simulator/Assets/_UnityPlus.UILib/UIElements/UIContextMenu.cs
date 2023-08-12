using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public class UIContextMenu : UIElement
    {
        internal readonly ContextMenu contextMenuComponent;
        internal readonly Image backgroundComponent;

        public UIContextMenu( RectTransform transform, ContextMenu contextMenuComponent, Image backgroundComponent ) : base( transform )
        {
            this.contextMenuComponent = contextMenuComponent;
            this.backgroundComponent = backgroundComponent;
        }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }
    }
}