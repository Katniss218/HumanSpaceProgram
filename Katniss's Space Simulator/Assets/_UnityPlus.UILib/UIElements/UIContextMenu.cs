using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A context menu contains a list of items.
    /// </summary>
    public partial class UIContextMenu : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven, IPointerEnterHandler, IPointerExitHandler
    {
        protected RectTransformTrackRectTransform trackerComponent;
        protected Image backgroundComponent;
        public virtual RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        public virtual Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public Action OnHide;

        public Vector2 Offset { get => trackerComponent.Offset; set => trackerComponent.Offset = value; }

        void OnDestroy()
        {
            OnHide?.Invoke();
        }

        private bool _isPointerExited = true;
        public bool AllowClickDestroy { get; set; }

        void LateUpdate()
        {
            if( !AllowClickDestroy )
            {
                return;
            }

            if( _isPointerExited )
            {
                if( Input.GetKeyDown( KeyCode.Mouse0 )
                 || Input.GetKeyDown( KeyCode.Mouse1 ) )
                {
                    this.Destroy();
                }
            }
        }

        public void OnPointerEnter( PointerEventData eventData )
        {
            _isPointerExited = false;
        }

        public void OnPointerExit( PointerEventData eventData )
        {
            // PointerEventData.fullyExited is false when pointer has exited to enter a child object.
            // This lets me check whether or not the cursor is over any of the descendants, regardless of their position.
            // This also will only be called after the pointer enters the menu and then leaves.
            if( eventData.fullyExited )
            {
                _isPointerExited = true;
            }
        }

        protected internal static T Create<T>( RectTransform track, UICanvas contextMenuCanvas, UILayoutInfo layoutInfo, Sprite background, Action onDestroy ) where T : UIContextMenu
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiContextMenu) = UIElement.CreateUIGameObject<T>( contextMenuCanvas, $"uilib-{typeof( T ).Name}", layoutInfo );
            rootTransform.pivot = new Vector2( 0, 1 ); // top-left

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = false;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            RectTransformTrackRectTransform trackerComponent = rootGameObject.AddComponent<RectTransformTrackRectTransform>();
            trackerComponent.Target = track;

            uiContextMenu.trackerComponent = trackerComponent;
            uiContextMenu.backgroundComponent = backgroundComponent;
            uiContextMenu.OnHide = onDestroy;
            return uiContextMenu;
        }
    }
}