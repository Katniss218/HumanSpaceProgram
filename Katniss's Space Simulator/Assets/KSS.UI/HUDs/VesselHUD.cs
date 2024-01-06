using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI.HUDs
{
    public class VesselHUD : MonoBehaviour
    {
        public Vessel Vessel { get; private set; }

        void OnClick()
        {
            ActiveObjectManager.ActiveObject = Vessel.gameObject;
        }

        void LateUpdate()
        {
            ((RectTransform)this.transform).SetScreenPosition( Cameras.GameplayCameraController.MainCamera, Vessel.transform.position );
        }

        public static VesselHUD Create( IUIElementContainer parent, Vessel vessel )
        {
            if( vessel == null )
                throw new ArgumentNullException( nameof( vessel ) );

            UIButton button = parent.AddButton( new UILayoutInfo( Vector2.zero, Vector2.zero, new Vector2( 30, 30 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_large_new" ), null );

            VesselHUD uiHUD = button.gameObject.AddComponent<VesselHUD>();
            button.onClick = uiHUD.OnClick;
            uiHUD.Vessel = vessel;
            return uiHUD;
        }
    }
}