using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue">The type of the outputted value.</typeparam>
    public partial class UIInputDropdown<TValue> : UIElementNonMonobehaviour, IUIInputElement<TValue>, IUIElementChild
    {
        protected TMPro.TextMeshProUGUI textComponent;
        protected TMPro.TextMeshProUGUI placeholderComponent;
        protected Image backgroundComponent;

        public IUIElementContainer Parent { get; set; }

        public virtual string Placeholder { get => placeholderComponent.text; set => placeholderComponent.text = value; }

        public virtual Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        protected TValue[] options;
        protected int? selectedValue;

        protected Func<TValue, string> valueToString;

        protected UICanvas contextMenuCanvas;
        protected UIContextMenu contextMenu;
        protected Sprite background;
        protected Sprite backgroundActive;
        protected Sprite contextMenuBackground;
        protected Sprite contextMenuElement;

        TMPro.TMP_FontAsset contextMenuFont;
        float contextMenuFontSize;
        Color contextMenuFontColor;

        public event Action<IUIInputElement<TValue>.ValueChangedEventData> OnValueChanged;

        public bool TryGetValue( out TValue value )
        {
            if( selectedValue.HasValue )
            {
                value = options[selectedValue.Value];
                return true;
            }

            value = default;
            return false;
        }

        public TValue GetOrDefault( TValue defaultValue )
        {
            return selectedValue.HasValue ? this.options[selectedValue.Value] : defaultValue;
        }

        public void TrySelect( int index )
        {
            if( index < 0 || index >= options.Length )
                return;
            if( options == null )
                return;

            selectedValue = index;
            SyncVisual();
            OnValueChanged?.Invoke( IUIInputElement<TValue>.ValueChangedEventData.Value( options[selectedValue.Value] ) );
        }

        public void ClearValue()
        {
            if( options == null )
                return;

            selectedValue = null;
            SyncVisual();
            OnValueChanged?.Invoke( default );
        }

        protected virtual void SyncVisual()
        {
            textComponent.enabled = selectedValue.HasValue;
            if( selectedValue.HasValue )
            {
                textComponent.text = valueToString.Invoke( options[selectedValue.Value] );
            }

            placeholderComponent.enabled = !selectedValue.HasValue;

            this.backgroundComponent.sprite = this.background;

            if( !this.contextMenu.IsNullOrDestroyed() )
            {
                this.contextMenu.Destroy();
            }
        }

        // TODO - The context menu could alternatively 'stay' until the player clicks somewhere else (input.getkeydown on all possible mouse keys) with any mouse button - instead of disappearing when the mouse exits.

        public void OnPointerClick()
        {
            if( this.contextMenu.IsNullOrDestroyed() )
            {
                float step = this.rectTransform.GetActualSize().y;

                float cmHeight = Mathf.Min( 250, step * this.options.Length );

                // open a context menu with a scrollable list of options.
                this.backgroundComponent.sprite = this.backgroundActive;
                this.contextMenu = this.CreateContextMenu( contextMenuCanvas, new UILayoutInfo( UIAnchor.Top, (0, 0), (this.rectTransform.GetActualSize().x, cmHeight) ), contextMenuBackground );
                this.contextMenu.OnHide = () =>
                {
                    if( this.textComponent != null && this.placeholderComponent != null && this.backgroundComponent != null ) // prevents being invoked when scene is destroyed.
                    {
                        this.SyncVisual();
                    }
                };
                var TEMP_TRACKER_DO_IT_PROPERLY = this.contextMenu.GetComponent<RectTransformTrackRectTransform>();
                TEMP_TRACKER_DO_IT_PROPERLY.SelfCorner = new Vector2( 0, 1 );
                TEMP_TRACKER_DO_IT_PROPERLY.TargetCorner = new Vector2( 0, 0 );

                UIScrollView uiScrollView = this.contextMenu.AddVerticalScrollView( new UILayoutInfo( UIFill.Fill() ), step * this.options.Length );

                for( int i = 0; i < this.options.Length; i++ )
                {
                    string str = valueToString.Invoke( this.options[i] );

                    int val = i;
                    var button = uiScrollView.AddButton( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, -step * i, step ), this.contextMenuElement, () =>
                    {
                        this.TrySelect( val );
                        this.contextMenu.Destroy();
                    } )
                        .AddText( new UILayoutInfo( UIFill.Fill() ), str )
                            .WithAlignment( TMPro.HorizontalAlignmentOptions.Left )
                            .WithFont( contextMenuFont, contextMenuFontSize, contextMenuFontColor );
                }
            }
            else
            {
                this.contextMenu.Destroy();
            }
        }

        void LateUpdate()
        {
            if( contextMenu != null )
            {
                //Vector2 halfSize = this.rectTransform.rect.size * 0.5f;
                //contextMenu.Offset = new Vector2( -halfSize.x, -halfSize.y );

                contextMenu.AllowClickDestroy = !this.rectTransform.rect.Contains( this.rectTransform.InverseTransformPoint( Input.mousePosition ) );
            }
        }

        protected internal static T Create<T>( IUIElementContainer parent, UICanvas contextMenuCanvas, UILayoutInfo layout, Sprite background, Sprite backgroundActive, Sprite contextMenuBackground, Sprite contextMenuElement, Func<TValue, string> valueToString ) where T : UIInputDropdown<TValue>
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiInputDropdown) = UIElementNonMonobehaviour.CreateUIGameObject<T>( parent, $"uilib-{typeof( T ).Name}", layout );

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

            (GameObject textareaGameObject, RectTransform textareaTransform) = UIElement.CreateUIGameObject( rootTransform, $"uilib-{typeof( T ).Name}-textarea", new UILayoutInfo( UIFill.Fill( 5, 5, 5, 5 ) ) );

            RectMask2D mask = textareaGameObject.AddComponent<RectMask2D>();
            mask.padding = new Vector4( -5, -5, -5, -5 );

            (GameObject placeholderGameObject, _) = UIElement.CreateUIGameObject( textareaTransform, $"uilib-{typeof( T ).Name}-placeholder", new UILayoutInfo( UIFill.Fill() ) );

            TMPro.TextMeshProUGUI placeholderText = placeholderGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            placeholderText.raycastTarget = false;
            placeholderText.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
            placeholderText.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            placeholderText.fontStyle = TMPro.FontStyles.Italic;

            (GameObject textGameObject, _) = UIElement.CreateUIGameObject( textareaTransform, $"uilib-{typeof( T ).Name}-text", new UILayoutInfo( UIFill.Fill() ) );

            TMPro.TextMeshProUGUI realText = textGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            realText.raycastTarget = false;
            realText.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
            realText.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;

            uiInputDropdown.placeholderComponent = placeholderText;
            uiInputDropdown.textComponent = realText;
            uiInputDropdown.background = background;
            uiInputDropdown.backgroundActive = backgroundActive;
            uiInputDropdown.contextMenuElement = contextMenuElement;
            uiInputDropdown.backgroundComponent = backgroundComponent;
            uiInputDropdown.contextMenuBackground = contextMenuBackground;
            uiInputDropdown.contextMenuCanvas = contextMenuCanvas;
            uiInputDropdown.options = null;
            uiInputDropdown.valueToString = valueToString;

            btn.onClick.AddListener( () => uiInputDropdown.OnPointerClick() );

            MonoBehaviourProxy proxy = rootGameObject.AddComponent<MonoBehaviourProxy>();
            proxy.onLateUpdate = uiInputDropdown.LateUpdate;

            return uiInputDropdown;
        }
    }
}