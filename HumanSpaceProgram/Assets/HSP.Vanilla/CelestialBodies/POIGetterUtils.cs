using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.SceneManagement;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.Scenes.GameplayScene.Cameras;
using HSP.Vanilla.Scenes.MapScene;
using HSP.Vessels;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.CelestialBodies
{
    /// <summary>
    /// Returns POIs for all loaded vessels.
    /// </summary>
    public sealed class AllLoadedVesselsPOIGetter : IPOIGetter
    {
        public IEnumerable<Vector3Dbl> GetPOIs()
        {
            return VesselManager.LoadedVessels.Select( v => v.ReferenceFrameTransform.AbsolutePosition );
        }


        [MapsInheritingFrom( typeof( AllLoadedVesselsPOIGetter ) )]
        public static SerializationMapping AllLoadedVesselsPOIGetterMapping()
        {
            return new MemberwiseSerializationMapping<AllLoadedVesselsPOIGetter>();
        }
    }

    /// <summary>
    /// Returns POIs for the active camera.
    /// </summary>
    public sealed class GameplaySceneCameraPOIGetter : IPOIGetter
    {
        public IEnumerable<Vector3Dbl> GetPOIs()
        {
            if ( !HSPSceneManager.IsForeground<GameplaySceneM>() )
                return Enumerable.Empty<Vector3Dbl>();

            Transform trans = SceneCamera.GetCamera<GameplaySceneM>().transform;
            return new Vector3Dbl[]
            {
                GameplaySceneReferenceFrameManager.ReferenceFrame.TransformPosition( trans.position ),
                GameplaySceneReferenceFrameManager.ReferenceFrame.TransformPosition( trans.position + trans.forward * 500 ),
            };
        }


        [MapsInheritingFrom( typeof( GameplaySceneCameraPOIGetter ) )]
        public static SerializationMapping ActiveCameraPOIGetterMapping()
        {
            return new MemberwiseSerializationMapping<GameplaySceneCameraPOIGetter>();
        }
    }

    /// <summary>
    /// Returns POIs for the active camera.
    /// </summary>
    public sealed class MapSceneCameraPOIGetter : IPOIGetter
    {
        public IEnumerable<Vector3Dbl> GetPOIs()
        {
            if( !HSPSceneManager.IsForeground<MapSceneM>() )
                return Enumerable.Empty<Vector3Dbl>();

            Transform trans = SceneCamera.GetCamera<MapSceneM>().transform;
            return new Vector3Dbl[]
            {
                MapSceneReferenceFrameManager.ReferenceFrame.TransformPosition( trans.position )
            };
        }


        [MapsInheritingFrom( typeof( MapSceneCameraPOIGetter ) )]
        public static SerializationMapping MapActiveCameraPOIGetterMapping()
        {
            return new MemberwiseSerializationMapping<MapSceneCameraPOIGetter>();
        }
    }
}