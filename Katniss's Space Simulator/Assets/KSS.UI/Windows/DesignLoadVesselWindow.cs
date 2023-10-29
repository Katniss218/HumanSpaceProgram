using KSS.Core.DesignScene;
using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI.Windows
{
    public class DesignLoadVesselWindow : MonoBehaviour
    {
        UIInputField inputField;
        UIWindow window;

        void Load()
        {
            DesignVesselManager.LoadVessel( IOHelper.SanitizeFileName( inputField.Text ) );
            window.Destroy();
        }

        public static DesignLoadVesselWindow Create()
        {
            UIWindow window = CanvasManager.Get( CanvasName.WINDOWS ).AddWindow( new UILayoutInfo( new Vector2( 0.5f, 0.5f ), Vector2.zero, new Vector2( 250f, 100f ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/part_window" ) )
                .Draggable()
                .Focusable()
           .WithCloseButton( new UILayoutInfo( Vector2.one, new Vector2( -7, -5 ), new Vector2( 20, 20 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_x_gold_large" ), out _ );

            DesignLoadVesselWindow c = window.gameObject.AddComponent<DesignLoadVesselWindow>();

            UIInputField input = window.AddInputField( UILayoutInfo.FillHorizontal( 2, 2, 1f, -30, 15 ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );
            c.inputField = input;
            c.window = window;
            UIButton btn = window.AddButton( new UILayoutInfo( Vector2.right, new Vector2( 2, 2 ), new Vector2( 45, 15 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_horizontal" ), c.Load );

            return c;
        }
    }
}