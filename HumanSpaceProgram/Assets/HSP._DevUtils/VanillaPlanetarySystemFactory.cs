using HSP.CelestialBodies;
using HSP.CelestialBodies.Surfaces;
using HSP.ReferenceFrames;
using HSP.Timelines;
using HSP.Trajectories;
using HSP.Vanilla.Trajectories;
using HSP.Vessels;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP._DevUtils
{
    public static class VanillaPlanetarySystemFactory
    {
        public const string CREATE_CELESTIAL_BODIES = HSPEvent.NAMESPACE_HSP + ".create_universe";

        private static CelestialBody CreateCB( string id, Vector3Dbl airfPos, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory( id ) { radius = 696_340_000, mass = 1.989e30 }.Create( airfPos, airfRot );
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

            CelestialBody cb = CreateCB( "main", "sun", 150_000_000_000, 0, 0, 0, 0, 0, orientation );
            CelestialBody cbSun = CreateCB( "sun", Vector3Dbl.zero, orientation );

            foreach( var x in CelestialBodyManager.CelestialBodies )
            {
                var state = x.GetComponent<TrajectoryTransform>().Trajectory.GetCurrentState();
                x.ReferenceFrameTransform.AbsolutePosition = state.AbsolutePosition;
                x.ReferenceFrameTransform.AbsoluteVelocity = state.AbsoluteVelocity;
            }


            //CelestialBody cb = CreateCB( "main", Vector3Dbl.zero, Vector3Dbl.zero, orientation );
            //CelestialBody cb = CreateCB( "main", new Vector3Dbl( 150_000_000_000_000, 0, 0 ), new Vector3Dbl( 0, 29800, 0 ), orientation );
            //CelestialBody cb = CreateCB( "main", new Vector3Dbl( 150_000_000_000_000_000, 0, 0 ), new Vector3Dbl( 0, 29749.1543788567, 0 ), orientation );

#warning TODO - all trajectories need to be instantiated and assigned before setting the pos/vel.
            // this requirement here to set the pos/vel also means we can't really load the parent lazily like I was doing.
            // this 'requirement' also needs to be satisfied for deserialization

            // we need to achieve this because the trajectory manager expects the correct value to be there.

#warning TODO maybe instead of issynchronized, we can have an enum specifying what is more up to date, or that both are synchronized.
            // this would eliminate the requirement by stating that the trajectory has correct values and the transform can be overwritten (would stop the initial back-feed)

#warning TODO - transform values NEED to be overwritten basically immediately, to spawn vessels and other stuff.

            // every external direct change of values should be passed through.

            // cb
            // body is created
            // traj is assigned

            // vessel
            // vessel is created
            // traj is assigned
            // 


            // deserialization would be fine because the vel is saved directly.




#warning TODO - first switch doesn't switch correctly, only after switching back and switching again, it switches right.
#warning TODO - at large SMA it doesn't want to switch at all.

            //CelestialBody cb1 = CreateCB( "moon1", "main", 440_000_00, 0, 0, 0, 0, 0, orientation );
            //CelestialBody cb1 = CreateCB( "moon2", new Vector3Dbl( 440_000_00, 100_000_000, 0 ), new Vector3Dbl( 0, 0, 0 ), orientation );
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
