using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Wrapper for generic input elements.
    /// </summary>
    /// <remarks>
    /// This is done to allow the use of generic UI elements (like <see cref="UIInputField{TValue}"/>). <br/>
    /// Normal UI elements are typically monobehaviours, but these ones are not, because <see cref="GameObject.AddComponent{T}()"/> doesn't work for T that is generic. <br/>
    /// So stupid Unity... There's no good reason for this other than laziness in serialization I suppose.
    /// </remarks>
    public class UIElementNonMonobehaviour : IUIElement
    {
        private GameObject _rootGameObject;

        public GameObject gameObject => _rootGameObject;
        public Transform transform => _rootGameObject.transform;

        protected UIElementNonMonobehaviour() { } // REQUIRED DEFAULT CONSTRUCTOR

        //
        //      BELOW SHOULD BE SYNCHRONIZED WITH `UIElement`
        //

        /// <summary>
        /// Don't directly modify the fields/state of the rectTransform unless you know what you're doing. You can produce invalid state.
        /// </summary>
        public RectTransform rectTransform { get => (RectTransform)this.transform; }

        public virtual void Destroy()
        {
            this.DestroyFixChildren();
            UnityEngine.Object.Destroy( this.gameObject );
        }

        private void DestroyFixChildren()
        {
            if( this is IUIElementChild child )
                child.Parent.Children.Remove( child ); // Removing with OnDestroy lags one frame behind, making destroying and recreating elements (refreshing) layout act as if the old items were still there.

            if( this is IUIElementContainer co )
                co.Children.Clear();
        }

        //
        //      ABOVE SHOULD BE SYNCHRONIZED WITH `UIElement`
        //

        /// <summary>
        /// A wrapper to create a UI gameobject with a UI element properly initialized. <br/>
        /// Use this to create the root gameobject for a custom UI element that doesn't inherit from any other element.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static (GameObject go, RectTransform t, T uiElement) CreateUIGameObject<T>( IUIElementContainer parent, string name, UILayoutInfo layout ) where T : UIElementNonMonobehaviour, IUIElementChild
        {
            (GameObject rootGO, RectTransform rootT) = UIElement.CreateUIGameObject( parent.contents, name, layout );

            T uiElement = Activator.CreateInstance<T>();
            uiElement._rootGameObject = rootGO;
            uiElement.SetParent( parent );
            return (rootGO, rootT, uiElement);
        }

        //
        //
        // Just in case someone tries to use these non-monobehaviour input elements as if they are monobehs - instead of using the builtin IsDestroyed() method.

        public override bool Equals( object obj )
        {
            if( obj is not UIElementNonMonobehaviour e )
                return false;

            return this._rootGameObject.Equals( e._rootGameObject );
        }

        public override int GetHashCode()
        {
            return this._rootGameObject.GetHashCode();
        }

        public override string ToString()
        {
            return this._rootGameObject.ToString();
        }

        public static bool operator ==( UIElementNonMonobehaviour x, UIElementNonMonobehaviour y )
        {
            if( x == null && y == null )
                return true;

            if( x != null && y != null )
                return x._rootGameObject == y._rootGameObject;

            return false;
        }

        public static bool operator !=( UIElementNonMonobehaviour x, UIElementNonMonobehaviour y )
        {
            if( x == null && y == null )
                return false;

            if( x != null && y != null )
                return x._rootGameObject != y._rootGameObject;

            return true;
        }
    }
}