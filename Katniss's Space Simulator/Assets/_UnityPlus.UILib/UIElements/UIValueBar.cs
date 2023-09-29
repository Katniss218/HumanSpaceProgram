using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIValueBar : UIElement
    {
        internal readonly UnityPlus.UILib.ValueBar valueBarComponent;

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer parent { get => _parent; }

        public UIValueBar( RectTransform transform, IUIElementContainer parent, UnityPlus.UILib.ValueBar valueBarComponent ) : base( transform )
        {
            this._parent = parent;
            this.valueBarComponent = valueBarComponent;
        }

        public void ClearSegments()
        {
            valueBarComponent.ClearSegments();
        }

        public ValueBarSegment AddSegment( float width )
        {
            return valueBarComponent.AddSegment( width );
        }

        public ValueBarSegment InsertSegment( int index, float width )
        {
            return valueBarComponent.InsertSegment( index, width );
        }
    }
}