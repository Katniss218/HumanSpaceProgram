using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue">The type of the outputted value.</typeparam>
    public partial class UIInputRadio<TValue> : UIElementNonMonobehaviour, IUIInputElement<TValue>, IUIElementChild
    {
        protected readonly static Dictionary<string, List<UIInputRadio<TValue>>> globalRadioMap = new();

        protected Image imageComponent;

        /// <summary>
        /// Determines which radios this radio works with.
        /// </summary>
        protected string context;

        protected TValue value;

        private Sprite _selectedSprite;
        /// <summary>
        /// Gets or sets the sprite displayed when the radio button is selected.
        /// </summary>
        public Sprite SelectedSprite
        {
            get => _selectedSprite;
            set
            {
                _selectedSprite = value;
                if( _isSelected )
                {
                    SyncVisual();
                }
            }
        }
        private Sprite _deselectedSprite;
        /// <summary>
        /// Gets or sets the sprite displayed when the radio button is not selected.
        /// </summary>
        public Sprite DeselectedSprite
        {
            get => _deselectedSprite;
            set
            {
                _deselectedSprite = value;
                if( !_isSelected )
                {
                    SyncVisual();
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if( _isSelected == value )
                {
                    return;
                }

                if( globalRadioMap.TryGetValue( context, out var relatedRadios ) )
                {
                    this.IsSelected = value;
                    foreach( var radio in relatedRadios )
                    {
                        if( radio != this )
                        {
                            radio.IsSelected = false;
                        }
                        SyncVisual();
                    }
                    foreach( var radio in relatedRadios )
                    {
                        radio.OnValueChanged?.Invoke( IUIInputElement<TValue>.ValueChangedEventData.Value( this.value ) );
                    }
                }
            }
        }

        public event Action<IUIInputElement<TValue>.ValueChangedEventData> OnValueChanged;

        public IUIElementContainer Parent { get; set; }

        public bool TryGetValue( out TValue value )
        {
            if( globalRadioMap.TryGetValue( context, out var list ) )
            {
                UIInputRadio<TValue> selectedRadio = list.FirstOrDefault( e => e.IsSelected );
                if( selectedRadio != null )
                {
                    value = selectedRadio.value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public TValue GetOrDefault( TValue defaultValue )
        {
            if( TryGetValue( out var value ) )
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Sets the value of this radio button. If the radio button is selected, this will change the currently selected value.
        /// </summary>
        public void SetValue( TValue value )
        {
            this.value = value;
            if( _isSelected )
            {
                if( globalRadioMap.TryGetValue( context, out var relatedRadios ) )
                {
                    foreach( var radio in relatedRadios )
                    {
                        radio.OnValueChanged?.Invoke( IUIInputElement<TValue>.ValueChangedEventData.Value( this.value ) );
                    }
                }
            }
        }

        void OnEnable()
        {
            if( !globalRadioMap.TryGetValue( context, out var list ) )
            {
                list = new List<UIInputRadio<TValue>>();
                globalRadioMap.Add( context, list );
            }

            list.Add( this );
        }

        void OnDisable()
        {
            if( _isSelected )
            {
                if( globalRadioMap.TryGetValue( context, out var relatedRadios ) )
                {
                    foreach( var radio in relatedRadios )
                    {
                        radio.OnValueChanged?.Invoke( IUIInputElement<TValue>.ValueChangedEventData.NoValue() );
                    }
                }
            }
            _isSelected = false;
            SyncVisual();
            if( globalRadioMap.TryGetValue( context, out var list ) )
            {
                list.Remove( this );
                if( list.Count == 0 )
                {
                    globalRadioMap.Remove( context );
                }
            }
        }

        protected virtual void SyncVisual()
        {
            if( imageComponent != null )
            {
                imageComponent.sprite = IsSelected ? SelectedSprite : DeselectedSprite;
            }
        }

        public void OnPointerClick()
        {
            IsSelected = true;
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, Sprite background, Sprite backgroundActive ) where T : UIInputRadio<TValue>
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiInputRadio) = UIElementNonMonobehaviour.CreateUIGameObject<T>( parent, $"uilib-{typeof( T ).Name}", layout );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = true;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

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

            uiInputRadio.DeselectedSprite = background;
            uiInputRadio.SelectedSprite = backgroundActive;

            btn.onClick.AddListener( () => uiInputRadio.OnPointerClick() );

            MonoBehaviourProxy proxy = rootGameObject.AddComponent<MonoBehaviourProxy>();
            proxy.onEnable = uiInputRadio.OnEnable;
            proxy.onDisable = uiInputRadio.OnDisable;

            return uiInputRadio;
        }
    }
}