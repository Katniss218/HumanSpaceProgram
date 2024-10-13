using HSP.CelestialBodies;
using HSP.CelestialBodies.Surfaces;
using HSP.Timelines;
using HSP.Trajectories;
using HSP.Vanilla.Trajectories;
using HSP.Vessels;
using System.Linq;
using UnityEngine;

namespace HSP._DevUtils
{
    public static class VanillaPlanetarySystemFactory
    {
        public const string CREATE_CELESTIAL_BODIES = HSPEvent.NAMESPACE_HSP + ".create_universe";

        private static CelestialBody CreateCB( string id, Vector3Dbl airfPos, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory( id ).Create( airfPos, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.PoIGetter = () => VesselManager.LoadedVessels.Select( v => v.ReferenceFrameTransform.AbsolutePosition );

            TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
            comp.IsAttractor = true;
            comp.Trajectory = new FixedOrbit( Time.TimeManager.UT, airfPos, airfRot, cb.Mass );
            return cb;
        }

        private static CelestialBody CreateCB( string id, string parentId, double semiMajorAxis, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double meanAnomaly, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory( id ).Create( Vector3Dbl.zero, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.PoIGetter = () => VesselManager.LoadedVessels.Select( v => v.ReferenceFrameTransform.AbsolutePosition );

            TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
            comp.IsAttractor = true;
            comp.Trajectory = new KeplerianOrbit( Time.TimeManager.UT, parentId, semiMajorAxis, eccentricity, inclination, longitudeOfAscendingNode, argumentOfPeriapsis, meanAnomaly, cb.Mass );
            return cb;
        }

        private static CelestialBody CreateCB( string id, Vector3Dbl airfPos, Vector3Dbl airfVel, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory( id ).Create( airfPos, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.PoIGetter = () => VesselManager.LoadedVessels.Select( v => v.ReferenceFrameTransform.AbsolutePosition );

            TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
            comp.IsAttractor = true;
            comp.Trajectory = new NewtonianOrbit( Time.TimeManager.UT, airfPos, airfVel, Vector3Dbl.zero, cb.Mass );
            return cb;
        }

        [HSPEventListener( HSPEvent_BEFORE_TIMELINE_NEW.ID, CREATE_CELESTIAL_BODIES )]
        [HSPEventListener( HSPEvent_BEFORE_TIMELINE_LOAD.ID, CREATE_CELESTIAL_BODIES )]
        public static void CreateDefaultPlanetarySystem()
        {
            QuaternionDbl orientation = Quaternion.Euler( 270, 0, 0 );
           // CelestialBody cb = CreateCB( "main", Vector3Dbl.zero, Vector3Dbl.zero, orientation );
            CelestialBody cb = CreateCB( "main", Vector3Dbl.zero, orientation );

            //CelestialBody cb1 = CreateCB( "moon1", "main", 440_000_00, 0, 0, 0, 0, 0, orientation );
           // CelestialBody cb1 = CreateCB( "moon1", new Vector3Dbl( 440_000_00, 0, 0 ), new Vector3Dbl( 0, 100000, 0 ), orientation );
            //CelestialBody cb1 = CreateCB( "moon1", new Vector3Dbl( 440_000_00, 0, 0 ), orientation );
            //CelestialBody cb2 = CreateCB( "moon2", new Vector3Dbl( 440_000_000, 100_000_000, 0 ), orientation );
            //CelestialBody cb_farawayTEST = CreateCB( "far", new Vector3Dbl( 440_000_000_0.0, 100_000_000, 0 ), orientation );
            //CelestialBody cb_farawayTEST2 = CreateCB( "further", new Vector3Dbl( 440_000_000_00.0, 100_000_000, 0 ), orientation );

            //CelestialBody cb_farawayTEST3FAR = CreateCB( "100ly", new Vector3Dbl( 1e18, 100_000_000, 0 ), QuaternionDbl.identity ); // 1e18 is 100 ly away.
            // stuff really far away throws invalid world AABB and such. do not enable these, you can't see them anyway. 100 ly seems to work, but further away is a no-no.

            CelestialBodySurface srf = cb.GetComponent<CelestialBodySurface>();
            //var group = srf.SpawnGroup( "aabb", 28.5857702f, -80.6507262f, (float)(cb.Radius + 1.0) );

            //ConstantForceApplier ca = cb.gameObject.AddComponent<ConstantForceApplier>();
            // ca.Force = new Vector3( (float)cb.Mass / 2, 0, 0 );
            //ca.Vessel = cb.PhysicsTransform;
        }
    }
}
