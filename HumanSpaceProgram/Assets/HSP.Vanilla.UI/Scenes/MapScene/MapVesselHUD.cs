using HSP.CelestialBodies;
using HSP.Vanilla.Scenes.MapScene;
using System;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Scenes.MapScene
{
    public class MapVesselHUD : UIPanel
    {
        public MapVessel Vessel { get; private set; }

        void LateUpdate()
        {
            ((RectTransform)this.transform).SetScreenPosition( SceneCamera.GetCamera<MapSceneM>(), (Vector3)Vessel.transform.position );
        }

        protected internal static T Create<T>( IUIElementContainer parent, MapVessel vessel ) where T : MapVesselHUD
        {
            if( vessel == null )
                throw new ArgumentNullException( nameof( vessel ) );

            T uiPanel = UIPanel.Create<T>( parent, new UILayoutInfo( UIAnchor.Center, (0, 0), (30, 30) ), null );

            UIButton button = uiPanel.AddButton( new UILayoutInfo( UIAnchor.Center, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );

            button.onClick = () => MapFocusedObjectManager.FocusedObject = vessel;
            uiPanel.Vessel = vessel;
            return uiPanel;
        }
    }

    public static class MapVesselHUD_Ex
    {
        public static MapVesselHUD AddMapVesselHUD( this IUIElementContainer parent, MapVessel vessel )
        {
            return MapVesselHUD.Create<MapVesselHUD>( parent, vessel );
        }
    }
}