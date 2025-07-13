using HSP.Vessels;
using System;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Vessels
{
    public class VesselHUD : UIPanel
    {
        public Vessel Vessel { get; private set; }

        void OnClick()
        {
            ActiveVesselManager.ActiveObject = Vessel.gameObject.transform;
        }

        void LateUpdate()
        {
            ((RectTransform)this.transform).SetScreenPosition( SceneCamera.Camera, (Vector3)Vessel.transform.position );
        }

        protected internal static T Create<T>( IUIElementContainer parent, Vessel vessel ) where T : VesselHUD
        {
            if( vessel == null )
                throw new ArgumentNullException( nameof( vessel ) );

            T uiPanel = UIPanel.Create<T>( parent, new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), null );

            UIButton button = uiPanel.AddButton( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );

            button.onClick = uiPanel.OnClick;
            uiPanel.Vessel = vessel;
            return uiPanel;
        }
    }

    public static class VesselHUD_Ex
    {
        public static VesselHUD AddVesselHUD( this IUIElementContainer parent, Vessel vessel )
        {
            return VesselHUD.Create<VesselHUD>( parent, vessel );
        }
    }
}