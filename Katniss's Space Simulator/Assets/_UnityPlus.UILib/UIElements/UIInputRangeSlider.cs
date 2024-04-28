using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public struct ValueRange<TValue>
    {
        public TValue min;
        public TValue max;
    }

    /// <summary>
    /// A range slider allows you to select an interval from a wider interval of possible input values.
    /// </summary>
    /// <typeparam name="TValue">The type of the outputted value.</typeparam>
    [Obsolete( "not implemented" )]
    public partial class UIInputRangeSlider<TValue> : UIElementNonMonobehaviour, IUIInputElement<ValueRange<TValue>>, IUIElementChild
    {
        public IUIElementContainer Parent { get; set; }

        protected float minT;
        protected float maxT;

        protected List<(float t, TValue v)> sortedValues = new();

        protected Func<TValue, TValue, float, TValue> interpolator;
        public bool IsInterpolating => interpolator != null;

        protected float step = 0.00001f; // step as percent of max t (0..1)

        public event Action<IUIInputElement<ValueRange<TValue>>.ValueChangedEventData> OnValueChanged;

        public bool TryGetValue( out ValueRange<TValue> value )
        {
            if( sortedValues.Count < 2 )
            {
                value = default;
                return false;
            }

            int segmentIndex = -1;
            for( int i = 0; i < sortedValues.Count - 1; i++ )
            {
                if( (minT >= sortedValues[i].t) && (minT < sortedValues[i + 1].t) )
                {
                    segmentIndex = i;
                    break;
                }
            }

            if( segmentIndex == -1 )
            {
                value = default;
                return false;
            }

            if( !IsInterpolating )
            {
                value = default;// sortedValues[segmentIndex].v;
                return true;
            }

            float interpolant = (minT - sortedValues[segmentIndex].t) / (sortedValues[segmentIndex + 1].t - sortedValues[segmentIndex].t);

            value = default;// interpolator( sortedValues[segmentIndex].v, sortedValues[segmentIndex + 1].v, interpolant );
            return true;
        }

        public ValueRange<TValue> GetOrDefault( ValueRange<TValue> defaultValue )
        {
            throw new NotImplementedException();
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UIInputSlider<TValue>
        {
            throw new NotImplementedException();
        }
    }
}