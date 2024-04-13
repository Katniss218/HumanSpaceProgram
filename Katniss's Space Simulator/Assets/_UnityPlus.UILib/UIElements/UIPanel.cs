using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a section of the canvas, or of a different UI element.
    /// </summary>
    public partial class UIPanel : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        protected Image backgroundComponent;
        public virtual RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        public virtual Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background ) where T : UIPanel
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiPanel) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{typeof( T ).Name}", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = false;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            uiPanel.backgroundComponent = backgroundComponent;
            return uiPanel;
        }
    }
}