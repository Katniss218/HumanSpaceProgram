using KSS.Core;
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
    public class TimewarpSelectorUI : MonoBehaviour // : ui element addition to "panel" element type
    {
        private IUIElementContainer _root;
        private UIButton[] _warpButtons;
        private UIText _text;

        private float[] _warpRates;

        private static void OnClick0()
        {
            TimeManager.Pause();
        }

        private static void OnClickNon0( float rate )
        {
            if( TimeManager.IsPaused )
            {
                TimeManager.SetTimeScale( 1f );
                return;
            }

            float newscale = rate;

            if( newscale > TimeManager.GetMaxTimescale() )
                return;

            TimeManager.SetTimeScale( newscale );
        }

        void OnEnable()
        {
            TimeManager.OnAfterTimescaleChanged += OnTimescaleChanged_Listener;
        }

        void Start()
        {
            Refresh();
        }

        void OnDisable()
        {
            TimeManager.OnAfterTimescaleChanged -= OnTimescaleChanged_Listener;
        }

        void OnTimescaleChanged_Listener( TimeManager.TimeScaleChangedData data )
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
            float currentWarpRate = TimeManager.TimeScale;

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
                    _warpButtons[i] = _root.AddButton( new UILayoutInfo( UILayoutInfo.Left, Vector2.zero, new Vector2( 16, 26 ) ),
                        currentWarpRate == 0
                        ? AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/timewarp_pause_active" )
                        : AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/timewarp_pause" ), OnClick0 );
                }
                else
                {
                    if( currentWarpRate == 0 )
                        _warpButtons[i] = _root.AddButton( new UILayoutInfo( UILayoutInfo.Left, Vector2.zero, Choose3Way( _sizes, _warpRates.Length, i ) ), Choose3Way( _sprites, _warpRates.Length, i ), () => OnClickNon0( warpRate ) );
                    else
                        _warpButtons[i] = _root.AddButton( new UILayoutInfo( UILayoutInfo.Left, Vector2.zero, Choose3Way( _sizes, _warpRates.Length, i ) ),
                            currentWarpRate >= warpRate
                            ? Choose3Way( _spritesActive, _warpRates.Length, i )
                            : Choose3Way( _sprites, _warpRates.Length, i ), () => OnClickNon0( warpRate ) );
                }
            }

            string rateText = currentWarpRate == 0
                ? $"Paused"
                : $"{currentWarpRate}x";

            _text = _root.AddText( UILayoutInfo.FillVertical( 0, 0, UILayoutInfo.LeftF, 0, 50f ), rateText )
                .WithAlignment( TMPro.HorizontalAlignmentOptions.Left )
                .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );

            UILayout.BroadcastLayoutUpdate( _root );
        }

        public static TimewarpSelectorUI Create( IUIElementContainer parent, UILayoutInfo layoutInfo, IEnumerable<float> warpRates )
        {
            if( warpRates.Any( rate => rate < 0.0f ) )
            {
                throw new ArgumentOutOfRangeException( nameof( warpRates ), $"Every warp rate must be either positive, or zero (pause)." );
            }

            UIPanel rootPanel = parent.AddPanel( layoutInfo, null );

            TimewarpSelectorUI timewarpSelectorUI = rootPanel.gameObject.AddComponent<TimewarpSelectorUI>();
            timewarpSelectorUI._root = rootPanel;
            timewarpSelectorUI._warpRates = warpRates.ToArray();

            rootPanel.LayoutDriver = new HorizontalLayoutDriver()
            {
                Dir = HorizontalLayoutDriver.Direction.LeftToRight,
                Spacing = 0f,
                FitToSize = true
            };

            timewarpSelectorUI.Refresh();

            return timewarpSelectorUI;
        }
    }
}