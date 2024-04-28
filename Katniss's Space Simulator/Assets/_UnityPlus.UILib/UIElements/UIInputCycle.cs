using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// An input element that has a cycle of values to toggle between. The sprite changes for each value.
    /// </summary>
    /// <typeparam name="TValue">The type of the outputted value.</typeparam>
    public partial class UIInputCycle<TValue> : UIElementNonMonobehaviour, IUIInputElement<TValue>, IUIElementChild
    {
        public enum Dir
        {
            Forward,
            Backward
        }

        protected Image backgroundComponent;

        public IUIElementContainer Parent { get; set; }

        protected (TValue v, Sprite sprite)[] options;

        protected int current;

        public event Action<IUIInputElement<TValue>.ValueChangedEventData> OnValueChanged;

        /// <summary>
        /// Determines which way the value will cycle when clicked.
        /// </summary>
        public Dir ClickDir { get; set; } = Dir.Forward;

        public bool TryGetValue( out TValue value )
        {
            if( current < 0 || current >= options.Length )
            {
                value = default;
                return false;
            }

            value = options[current].v;
            return true;
        }

        public TValue GetOrDefault( TValue defaultValue )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the selected value to the next value in sequence.
        /// </summary>
        public void CycleForwards( int step = 1 )
        {
            current = (current + step) % options.Length;
            SyncVisual();
            OnValueChanged?.Invoke( IUIInputElement<TValue>.ValueChangedEventData.Value( options[current].v ) );
        }

        /// <summary>
        /// Sets the selected value to the previous value in sequence.
        /// </summary>
        public void CycleBackwards( int step = 1 )
        {
            current = (current - step) % options.Length;
            SyncVisual();
            OnValueChanged?.Invoke( IUIInputElement<TValue>.ValueChangedEventData.Value( options[current].v ) );
        }

        protected virtual void SyncVisual()
        {
            Sprite sprite = options[current].sprite;
            backgroundComponent.sprite = sprite;

            if( sprite == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }
        }

        public void OnPointerClick()
        {
            if( ClickDir == Dir.Forward )
            {
                CycleForwards();
            }
            else if( ClickDir == Dir.Backward )
            {
                CycleBackwards();
            }
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, (TValue v, Sprite sprite)[] options ) where T : UIInputCycle<TValue>
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiInputCycle) = UIElementNonMonobehaviour.CreateUIGameObject<T>( parent, $"uilib-{typeof( T ).Name}", layout );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = true;
            backgroundComponent.type = Image.Type.Sliced;

            Button btn = rootGameObject.AddComponent<Button>();
            btn.colors = new ColorBlock()
            {
                normalColor = Color.white,
                selectedColor = Color.white,
                colorMultiplier = 1.0f,
                highlightedColor = Color.white,
                pressedColor = Color.white,
                disabledColor = Color.gray
            };

            uiInputCycle.options = options;
            uiInputCycle.current = 0;

            return uiInputCycle;
        }
    }
}