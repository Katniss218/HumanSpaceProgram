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
            /// <summary>
            /// True if the new value exists.
            /// </summary>
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
        /// Called when the current value of the input element changes (e.g. because the user selected a different value).
        /// </summary>
        event Action<ValueChangedEventData> OnValueChanged;

        /// <summary>
        /// Tries to retrieve the current value from the input element.
        /// </summary>
        /// <param name="value">If the return value was true, the retrieved value of the input element. Otherwise undefined.</param>
        /// <returns>False if the input element currently doesn't have a value, or the value couldn't be retrieved. Otherwise true.</returns>
        bool TryGetValue( out TValue value );

        /// <summary>
        /// Gets the current value of the input element (if any), or the specified default value.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the input element doesn't have a set value at the current time.</param>
        /// <returns>The current value of the input element, or the specified default value.</returns>
        TValue GetOrDefault( TValue defaultValue );
    }
}