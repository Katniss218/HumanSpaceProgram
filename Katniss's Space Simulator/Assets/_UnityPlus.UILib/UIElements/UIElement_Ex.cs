using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    public static class UIElement_Ex
    {
        /// <summary>
        /// Checks whether or not the specified UI element has been destroyed.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNullOrDestroyed( this IUIElement uiElement )
        {
            return Object_Ex.IsUnityNull( uiElement );
        }

        /// <summary>
        /// Sets <paramref name="parent"/> to be the parent of <paramref name="child"/>.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
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

            UILayoutManager.ForceLayoutUpdate( child );
        }

        public static void MoveBefore( this IUIElementChild child, IUIElementChild anotherChild )
        {
            if( child == null )
                throw new ArgumentNullException( nameof( child ) );
            if( anotherChild == null )
                throw new ArgumentNullException( nameof( anotherChild ) );

            if( child.Parent != anotherChild.Parent )
                throw new ArgumentException( $"Both children have to have the same parent." );

            child.rectTransform.SetSiblingIndex( anotherChild.rectTransform.GetSiblingIndex() - 1 );

            UILayoutManager.ForceLayoutUpdate( child );
        }

        public static void MoveAfter( this IUIElementChild child, IUIElementChild anotherChild )
        {
            if( child == null )
                throw new ArgumentNullException( nameof( child ) );
            if( anotherChild == null )
                throw new ArgumentNullException( nameof( anotherChild ) );

            if( child.Parent != anotherChild.Parent )
                throw new ArgumentException( $"Both children have to have the same parent." );

            child.rectTransform.SetSiblingIndex( anotherChild.rectTransform.GetSiblingIndex() );

            UILayoutManager.ForceLayoutUpdate( child );
        }

        public static void MoveToStart( this IUIElementChild child )
        {
            if( child == null )
                throw new ArgumentNullException( nameof( child ) );

            child.rectTransform.SetAsFirstSibling();

            UILayoutManager.ForceLayoutUpdate( child );
        }

        public static void MoveToEnd( this IUIElementChild child )
        {
            if( child == null )
                throw new ArgumentNullException( nameof( child ) );

            child.rectTransform.SetAsLastSibling();

            UILayoutManager.ForceLayoutUpdate( child );
        }
    }
}