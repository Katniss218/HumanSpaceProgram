using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.UILib.UIElements
{
    public interface IUIInputElement<TValue>
    {
        public class ValueChangedEventData
        {
            public bool HasValue { get; set; }
            public TValue NewValue { get; set; }

            public static ValueChangedEventData Value( TValue value )
            {
                return new ValueChangedEventData()
                {
                    HasValue = true,
                    NewValue = value
                };
            }

            public static ValueChangedEventData NoValue()
            {
                return new ValueChangedEventData()
                {
                    HasValue = false,
                    NewValue = default
                };
            }
        }

        /// <summary>
        /// Called when the value returned by calling <see cref="TryGetValue(out TValue)"/> changes.
        /// </summary>
        event Action<ValueChangedEventData> OnValueChanged;

        /// <summary>
        /// Tries to retrieve the current value from the input element.
        /// </summary>
        /// <param name="value">If the return value was true, the retrieved value of the input element. Otherwise undefined.</param>
        /// <returns>False if the input element currently doesn't have a value, or the value couldn't be retrieved. Otherwise true.</returns>
        bool TryGetValue( out TValue value );

        TValue GetOrDefault( TValue defaultValue );
    }
}