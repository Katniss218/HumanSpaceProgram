using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vessels;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Vanilla.UI.Vessels
{
    public class GameplayVesselHUDManager : SingletonMonoBehaviour<GameplayVesselHUDManager>
    {
        List<VesselHUD> _huds = new List<VesselHUD>();

        public const string CREATE_VESSEL_HUD = HSPEvent.NAMESPACE_HSP + ".create_vessel_hud";
        public const string DESTROY_VESSEL_HUD = HSPEvent.NAMESPACE_HSP + ".destroy_vessel_hud";
        public const string CREATE_OR_DESTROY_VESSEL_HUDS = HSPEvent.NAMESPACE_HSP + ".c_or_d_vessel_huds";

        [HSPEventListener( HSPEvent_AFTER_VESSEL_CREATED.ID, CREATE_VESSEL_HUD )]
        private static void AfterVesselCreated( Vessel vessel )
        {
            if( !instanceExists )
                return;

            if( ActiveVesselManager.ActiveObject == null )
            {
                var hud = GameplaySceneM.Instance.GetBackgroundCanvas().AddVesselHUD( vessel );
                instance._huds.Add( hud );
            }
        }

        [HSPEventListener( HSPEvent_AFTER_VESSEL_DESTROYED.ID, DESTROY_VESSEL_HUD )]
        private static void AfterVesselDestroyed( Vessel vessel )
        {
            if( !instanceExists )
                return;

            foreach( var hud in instance._huds.ToArray() )
            {
                if( hud == null ) // hud can be null if exiting a scene - it doesn't affect anything, but gives ugly warnings.
                    return;

                if( hud.Vessel == vessel )
                {
                    Destroy( hud.gameObject );
                    instance._huds.Remove( hud );
                }
            }
        }

        [HSPEventListener( HSPEvent_AFTER_ACTIVE_VESSEL_CHANGED.ID, CREATE_OR_DESTROY_VESSEL_HUDS )]
        private static void AfterActiveObjectChanged()
        {
            if( !instanceExists )
                return;

            if( ActiveVesselManager.ActiveObject == null )
            {
                var canvas = GameplaySceneM.Instance.GetBackgroundCanvas();

                foreach( var vessel in VesselManager.LoadedVessels )
                {
                    var hud = canvas.AddVesselHUD( vessel );
                    instance._huds.Add( hud );
                }
            }
            else
            {
                foreach( var hud in instance._huds )
                {
                    Destroy( hud.gameObject );
                }
                instance._huds.Clear();
            }
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_ACTIVATE.ID, CREATE_VESSEL_HUD )]
        private static void OnGameplaySceneActivate()
        {
            if( !instanceExists )
                return;

            if( ActiveVesselManager.ActiveObject == null )
            {
                var canvas = GameplaySceneM.Instance.GetBackgroundCanvas();

                foreach( var vessel in VesselManager.LoadedVessels )
                {
                    var hud = canvas.AddVesselHUD( vessel );
                    instance._huds.Add( hud );
                }
            }
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_DEACTIVATE.ID, DESTROY_VESSEL_HUD )]
        private static void OnGameplaySceneDeactivate()
        {
            if( !instanceExists )
                return;

            foreach( var hud in instance._huds )
            {
                Destroy( hud.gameObject );
            }
            instance._huds.Clear();
        }
    }
}