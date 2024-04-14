using KSS.Core;
using KSS.Core.ResourceFlowSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class UITimewarpSelector : UIPanel
    {
        private UIButton[] _warpButtons;
        private UIText _text;

        private float[] _warpRates;

        private static void OnClick0()
        {
            TimeStepManager.Pause();
        }

        private static void OnClickNon0( float rate )
        {
            if( TimeStepManager.IsPaused )
            {
                TimeStepManager.SetTimeScale( 1f );
                return;
            }

            float newscale = rate;

            if( newscale > TimeStepManager.GetMaxTimescale() )
                return;

            TimeStepManager.SetTimeScale( newscale );
        }

        void OnEnable()
        {
            TimeStepManager.OnAfterTimescaleChanged += OnTimescaleChanged_Listener;
        }

        void Start()
        {
            Refresh();
        }

        void OnDisable()
        {
            TimeStepManager.OnAfterTimescaleChanged -= OnTimescaleChanged_Listener;
        }

        void OnTimescaleChanged_Listener( TimeStepManager.TimeScaleChangedData data )
        {
            Refresh();
        }

        public static T Choose3Way<T>( T[] array3Elems, int length, int index )
        {
            if( index == 0 )
                return array3Elems[0];
            if( index == length - 1 )
                return array3Elems[2];
            return array3Elems[1];
        }

        static Vector2[] _sizes = new Vector2[3]
        {
            new Vector2( 16, 26 ),
            new Vector2( 13, 26 ),
            new Vector2( 16, 26 )
        };

        static Sprite[] _sprites = new Sprite[3]
        {
            AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/timewarp_forward_left" ),
            AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/timewarp_forward_middle" ),
            AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/timewarp_forward_right" )
        };
        static Sprite[] _spritesActive = new Sprite[3]
        {
            AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/timewarp_forward_active_left" ),
            AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/timewarp_forward_active_middle" ),
            AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/timewarp_forward_active_right" )
        };

        private void Refresh()
        {
            float currentWarpRate = TimeStepManager.TimeScale;

            Array.Sort( _warpRates );
            if( _warpButtons != null )
            {
                foreach( var btn in _warpButtons )
                {
                    btn.Destroy();
                }
            }
            if( _text != null )
            {
                _text.Destroy();
            }

            _warpButtons = new UIButton[_warpRates.Length];
            for( int i = 0; i < _warpRates.Length; i++ )
            {
                float warpRate = _warpRates[i];

                if( warpRate == 0.0f )
                {
                    _warpButtons[i] = this.AddButton( new UILayoutInfo( UIAnchor.Left, (0, 0), ( 16, 26 ) ),
                        currentWarpRate == 0
                        ? AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/timewarp_pause_active" )
                        : AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/timewarp_pause" ), OnClick0 );
                }
                else
                {
                    if( currentWarpRate == 0 )
                        _warpButtons[i] = this.AddButton( new UILayoutInfo( UIAnchor.Left, (0,0), (UISize)Choose3Way( _sizes, _warpRates.Length, i ) ), Choose3Way( _sprites, _warpRates.Length, i ), () => OnClickNon0( warpRate ) );
                    else
                        _warpButtons[i] = this.AddButton( new UILayoutInfo( UIAnchor.Left, (0, 0), (UISize)Choose3Way( _sizes, _warpRates.Length, i ) ),
                            currentWarpRate >= warpRate
                            ? Choose3Way( _spritesActive, _warpRates.Length, i )
                            : Choose3Way( _sprites, _warpRates.Length, i ), () => OnClickNon0( warpRate ) );
                }
            }

            string rateText = currentWarpRate == 0
                ? $"Paused"
                : $"{currentWarpRate}x";

            _text = this.AddText( new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 0, 50f ), rateText )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Left )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            UILayoutManager.BroadcastLayoutUpdate( this );
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, IEnumerable<float> warpRates ) where T : UITimewarpSelector
        {
            if( warpRates.Any( rate => rate < 0.0f ) )
            {
                throw new ArgumentOutOfRangeException( nameof( warpRates ), $"Every warp rate must be either positive, or zero (pause)." );
            }

            T timewarpSelectorUI = UIPanel.Create<T>( parent, layout, null );

            timewarpSelectorUI._warpRates = warpRates.ToArray();

            timewarpSelectorUI.LayoutDriver = new HorizontalLayoutDriver()
            {
                Dir = HorizontalLayoutDriver.Direction.LeftToRight,
                Spacing = 0f,
                FitToSize = true
            };

            timewarpSelectorUI.Refresh();

            return timewarpSelectorUI;
        }
    }

    public static class UITimewarpSelector_Ex
    {
        public static UITimewarpSelector AddTimewarpSelector( this IUIElementContainer parent, UILayoutInfo layout, IEnumerable<float> warpRates )
        {
            return UITimewarpSelector.Create<UITimewarpSelector>( parent, layout, warpRates );
        }
    }
}