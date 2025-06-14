using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene.Cameras;
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
    public sealed class ActiveCameraPOIGetter : IPOIGetter
    {
        public IEnumerable<Vector3Dbl> GetPOIs()
        {
            if ( GameplaySceneCameraManager.ViewTransform == null )
                return Enumerable.Empty<Vector3Dbl>();

            return new Vector3Dbl[]
            {
                SceneReferenceFrameManager.ReferenceFrame.TransformPosition( GameplaySceneCameraManager.ViewTransform.position ),
                SceneReferenceFrameManager.ReferenceFrame.TransformPosition( GameplaySceneCameraManager.ViewTransform.position + GameplaySceneCameraManager.ViewTransform.forward * 500 ),
            };
        }


        [MapsInheritingFrom( typeof( ActiveCameraPOIGetter ) )]
        public static SerializationMapping ActiveCameraPOIGetterMapping()
        {
            return new MemberwiseSerializationMapping<ActiveCameraPOIGetter>();
        }
    }
}