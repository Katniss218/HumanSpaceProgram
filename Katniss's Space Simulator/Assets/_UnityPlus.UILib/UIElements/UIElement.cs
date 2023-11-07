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

        /// <summary>
        /// Destroys the specified UI element along with its children UI elements.
        /// </summary>
        /// <remarks>
        /// Override this method if a more complex destroy logic is required.
        /// </remarks>
        public virtual void Destroy()
        {
            if( this.gameObject == null )
            {
                return; // Silent quit.
            }

            Destroy( this.gameObject );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static (GameObject go, RectTransform t) CreateUI( RectTransform parent, string name, UILayoutInfo layout )
        {
            GameObject rootGO = new GameObject( name );

            RectTransform rootT = rootGO.AddComponent<RectTransform>();
            rootT.SetParent( parent );
            rootT.SetLayoutInfo( layout );
            rootT.localScale = Vector3.one;

            return (rootGO, rootT);
        }
    }
}