using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIElement
    {
        // The purpose of the UI elements' subclasses is to serve as lightweight wrappers around the actual UI components (and in some cases entire hierarchies),
        // - for the purpose of easier creation and chaining.

        public RectTransform transform;

        // Can also store additional info about how the children should lay themselves out.
        // Then set it with extension methods, for a static horizontal/vertical/grid layout.

        public UIElement( RectTransform transform )
        {
            this.transform = transform;
        }
    }
}