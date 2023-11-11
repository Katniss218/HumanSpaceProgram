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
#warning TODO - I think I want it to be a monobehaviour (no changes in the outside code).
        // this will achieve parity with the other custom UI elems which are MBs.

        // REASON:
        // - Many UI elements consist of multiple objects.
        // - Their initialization is an annoying ordeal and they can end up with an invalid or non-standard state easily.
        // - This fixes that by encapsulating everything.

        // Some UI elements here have somewhat duplicated purpose. This is for increased verbosity. Lets you specify exactly what you're creating.

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
            GameObject rootGO = new GameObject( name );

            RectTransform rootT = rootGO.AddComponent<RectTransform>();
            rootT.SetLayoutInfo( layout );
            rootT.localScale = Vector3.one;

            T uiElement = rootGO.AddComponent<T>();
            uiElement.SetParent( parent );
            return (rootGO, rootT, uiElement);
        }
    }
    public static class UIElement_Ex
    {
        public static void SetParent( this IUIElementChild child, IUIElementContainer parent )
        {
            if( child == null )
                throw new ArgumentNullException( nameof( child ) );
            if( parent == null )
                throw new ArgumentNullException( nameof( parent ) );

            if( child.Parent != null )
            {
                child.Parent.Children.Remove( child );
            }
            child.Parent = parent;
            parent.Children.Add( child );
            child.rectTransform.SetParent( parent.contents );
        }
    }
}