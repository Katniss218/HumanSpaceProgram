using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A slider allows you to select a single value from an interval of possible input values.
    /// </summary>
    /// <typeparam name="TValue">The type of the outputted value.</typeparam>
    public partial class UIInputSlider<TValue> : UIElementNonMonobehaviour, IUIInputElement<TValue>, IUIElementChild
    {
        public IUIElementContainer Parent { get; set; }

        protected float currentT;

        protected List<(float t, TValue v)> sortedValues = new();

        protected Func<TValue, TValue, float, TValue> interpolator;
        public bool IsInterpolating => interpolator != null;

        protected float step = 0.00001f; // step as percent of max t (0..1)

        public event Action<IUIInputElement<TValue>.ValueChangedEventData> OnValueChanged;

        // A slider produces float values, between a and b, with rounding to the nearest multiple of x.

        // slider has 3 sprites
        // - background
        // - foreground (fills between start and handle)
        // - handle (grabbable)

        //

        public bool TryGetValue( out TValue value )
        {
            if( sortedValues.Count < 2 )
            {
                value = default;
                return false;
            }

            int segmentIndex = -1;
            for( int i = 0; i < sortedValues.Count - 1; i++ )
            {
                if( (currentT >= sortedValues[i].t) && (currentT < sortedValues[i + 1].t) )
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
                value = sortedValues[segmentIndex].v;
                return true;
            }

            float interpolant = (currentT - sortedValues[segmentIndex].t) / (sortedValues[segmentIndex + 1].t - sortedValues[segmentIndex].t);

            value = interpolator( sortedValues[segmentIndex].v, sortedValues[segmentIndex + 1].v, interpolant );
            return true;
        }

        public TValue GetOrDefault( TValue defaultValue )
        {
            throw new NotImplementedException();
        }

        protected virtual void SyncVisual()
        {
            // put knob where it should go
            // set range's size to match knob
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UIInputSlider<TValue>
        {
            throw new NotImplementedException();
        }
    }
}