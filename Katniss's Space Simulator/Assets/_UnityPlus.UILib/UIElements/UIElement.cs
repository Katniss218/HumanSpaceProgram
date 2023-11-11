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
    public class UIElement : MonoBehaviour
    {
        // PURPOSE:
        // - This class (and its subclasses) is a wrapper around hierarchies of Unity components / gameobjects.
        // - This wrapper is itself a monobehaviour to eliminate the state where the underlying component has been destroyed, but the wrapper is still alive.

        // REASON:
        // - Many UI elements consist of multiple objects.
        // - Their initialization is an annoying ordeal and they can end up with an invalid or non-standard state easily.

        /// <summary>
        /// Don't directly modify the fields/state of the rectTransform unless you know what you're doing. You can produce invalid state.
        /// </summary>
        public RectTransform rectTransform { get => (RectTransform)this.transform; }

        public void Destroy() // UI elements should be attached to the root GameObject of their subtree.
        {
            Destroy( this.gameObject );
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
        /// A wrapper to create a UI gameobject with a UI element properly initialized. Use this to create the root gameobject for a custom UI element.
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