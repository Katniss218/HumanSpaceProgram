using HSP.CelestialBodies;
using HSP.CelestialBodies.Surfaces;
using HSP.Timelines;
using HSP.Vessels;
using System.Linq;
using UnityEngine;

namespace HSP._DevUtils
{
    public static class VanillaPlanetarySystemFactory
    {
        public const string CREATE_UNIVERSE = HSPEvent.NAMESPACE_HSP + ".create_universe";

        private static CelestialBody CreateCB( string id, Vector3Dbl airfPos, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory(id).Create( airfPos, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.PoIGetter = () => VesselManager.LoadedVessels.Select( v => v.AIRFPosition );
            return cb;
        }

        [HSPEventListener( HSPEvent_BEFORE_TIMELINE_NEW.ID, CREATE_UNIVERSE )]
        [HSPEventListener( HSPEvent_BEFORE_TIMELINE_LOAD.ID, CREATE_UNIVERSE )]
        public static void CreateDefaultPlanetarySystem( object e )
        {
            QuaternionDbl orientation = Quaternion.Euler( 270, 0, 0 );
            CelestialBody cb = CreateCB( "main", Vector3Dbl.zero, orientation );

            CelestialBody cb1 = CreateCB( "moon1", new Vector3Dbl( 440_000_000, 0, 0 ), orientation );
            CelestialBody cb2 = CreateCB( "moon2", new Vector3Dbl( 440_000_000, 100_000_000, 0 ), orientation );
            CelestialBody cb_farawayTEST = CreateCB( "far", new Vector3Dbl( 440_000_000_0.0, 100_000_000, 0 ), orientation );
            CelestialBody cb_farawayTEST2 = CreateCB( "further", new Vector3Dbl( 440_000_000_00.0, 100_000_000, 0 ), orientation );

            CelestialBody cb_farawayTEST3FAR = CreateCB( "100ly", new Vector3Dbl( 1e18, 100_000_000, 0 ), QuaternionDbl.identity ); // 1e18 is 100 ly away.
            // stuff really far away throws invalid world AABB and such. do not enable these, you can't see them anyway. 100 ly seems to work, but further away is a no-no.

            CelestialBodySurface srf = cb.GetComponent<CelestialBodySurface>();
            //var group = srf.SpawnGroup( "aabb", 28.5857702f, -80.6507262f, (float)(cb.Radius + 1.0) );

        }
    }
}
