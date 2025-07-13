using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a generic UI element.
    /// </summary>
    public class UIElement : MonoBehaviour, IUIElement
    {
        /// <summary>
        /// Don't directly modify the fields/state of the rectTransform unless you know what you're doing. You can produce invalid state.
        /// </summary>
        public RectTransform rectTransform { get => (RectTransform)this.transform; }

        public virtual void Destroy() // UI elements should be attached to the root GameObject of their subtree.
        {
            this.DestroyFixChildren();
            Destroy( this.gameObject );
        }

        private void DestroyFixChildren() // UI elements should be attached to the root GameObject of their subtree.
        {
            if( this is IUIElementChild child )
                child.Parent.Children.Remove( child ); // Removing with OnDestroy lags one frame behind, making destroying and recreating elements (refreshing) layout act as if the old items were still there.

            if( this is IUIElementContainer co )
                co.Children.Clear();
        }

        /// <summary>
        /// A wrapper to create a UI gameobject
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static (GameObject go, RectTransform t) CreateUIGameObject( RectTransform parent, string name, UILayoutInfo layout )
        {
            GameObject rootGO = new GameObject( name );

            RectTransform rootT = rootGO.AddComponent<RectTransform>();
            rootT.SetParent( parent );
            rootT.SetLayoutInfo( layout );
            rootT.localScale = Vector3.one;

            return (rootGO, rootT);
        }

        /// <summary>
        /// A wrapper to create a UI gameobject with a UI element properly initialized. <br/>
        /// Use this to create the root gameobject for a custom UI element that doesn't inherit from any other element.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static (GameObject go, RectTransform t, T uiElement) CreateUIGameObject<T>( IUIElementContainer parent, string name, UILayoutInfo layout ) where T : Component, IUIElementChild
        {
            (GameObject rootGO, RectTransform rootT) = CreateUIGameObject( parent.contents, name, layout );

            T uiElement = rootGO.AddComponent<T>();
            uiElement.SetParent( parent );
            return (rootGO, rootT, uiElement);
        }
    }
}