using HSP.Vanilla.Scenes.MapScene;
using System;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Scenes.MapScene
{
    public class MapCelestialBodyHUD : UIPanel
    {
        public MapCelestialBody CelestialBody { get; private set; }

        void LateUpdate()
        {
            ((RectTransform)this.transform).SetScreenPosition( SceneCamera.GetCamera<MapSceneM>(), (Vector3)CelestialBody.transform.position );
        }

        protected internal static T Create<T>( IUIElementContainer parent, MapCelestialBody celestialBody ) where T : MapCelestialBodyHUD
        {
            if( celestialBody == null )
                throw new ArgumentNullException( nameof( celestialBody ) );

            T uiPanel = UIPanel.Create<T>( parent, new UILayoutInfo( UIAnchor.Center, (0, 0), (30, 30) ), null );

            UIButton button = uiPanel.AddButton( new UILayoutInfo( UIAnchor.Center, (0, 0), (30, 30) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_30x30" ), null );

            button.onClick = () => MapFocusedObjectManager.FocusedObject = celestialBody;
            uiPanel.CelestialBody = celestialBody;
            return uiPanel;
        }
    }

    public static class MapCelestialBodyHUD_Ex
    {
        public static MapCelestialBodyHUD AddMapCelestialBodyHUD( this IUIElementContainer parent, MapCelestialBody celestialBody )
        {
            return MapCelestialBodyHUD.Create<MapCelestialBodyHUD>( parent, celestialBody );
        }
    }
}