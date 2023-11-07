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
    public sealed class UIPanel : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        internal Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public List<IUIElementChild> Children { get; private set; }
        internal IUIElementContainer _parent;
        public IUIElementContainer Parent { get => _parent; }

        public LayoutDriver LayoutDriver { get; set; }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public override void Destroy()
        {
            base.Destroy();
            this.Parent.Children.Remove( this );
        }

        public static UIPanel Create( IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( parent.contents, "uilib-panel", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = false;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            UIPanel panel = rootGameObject.AddComponent<UIPanel>();
            panel.Children = new List<IUIElementChild>();
            panel._parent = parent;
            panel.Parent.Children.Add( panel );
            panel.backgroundComponent = backgroundComponent;

            return panel;
        }
    }
}