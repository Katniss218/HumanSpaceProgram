using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Vanilla.Trajectories;
using HSP.Vessels;
using System;
using UnityEngine;

namespace HSP.Vanilla
{
    public static class VesselPinning_Ex
    {
        public static bool IsPinned( this Vessel vessel )
        {
            return vessel.ReferenceFrameTransform is PinnedCelestialBodyReferenceFrameTransform;
        }

        public static void Pin( this Vessel vessel )
        {
            // Pins the vessel to the most influential celestial body, at the current position.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Pins the vessel to the celestial body at the specified location.
        /// </summary>
        public static void Pin( this Vessel vessel, ICelestialBody body, Vector3Dbl referencePosition, QuaternionDbl referenceRotation )
        {
            IReferenceFrameTransform oldReferenceFrameTransform = vessel.ReferenceFrameTransform;
            ISceneReferenceFrameProvider sceneFrameProvider = oldReferenceFrameTransform.SceneReferenceFrameProvider;
            IPhysicsTransform oldPhysTransform = vessel.PhysicsTransform;

            UnityEngine.Object.DestroyImmediate( (Component)vessel.PhysicsTransform );
            var ppo = vessel.gameObject.AddComponent<PinnedCelestialBodyReferenceFrameTransform>();
            ppo.SceneReferenceFrameProvider = sceneFrameProvider;
            ppo.SetReference( body, referencePosition, referenceRotation );

            ppo.Mass = oldPhysTransform.Mass;
            ppo.LocalCenterOfMass = oldPhysTransform.LocalCenterOfMass;
            ppo.MomentsOfInertia = oldPhysTransform.MomentsOfInertia;
            ppo.MomentsOfInertiaRotation = oldPhysTransform.MomentsOfInertiaRotation;

            TrajectoryTransform tt = vessel.gameObject.GetComponent<TrajectoryTransform>();
            tt.enabled = false;
        }

        /// <summary>
        /// Unpins the vessel from a celestial body at its current location.
        /// </summary>
        public static void Unpin( this Vessel vessel )
        {
            // possibly needs to set velocity.
            IReferenceFrameTransform oldReferenceFrameTransform = vessel.ReferenceFrameTransform;
            ISceneReferenceFrameProvider sceneFrameProvider = oldReferenceFrameTransform.SceneReferenceFrameProvider;
            IPhysicsTransform oldPhysTransform = vessel.PhysicsTransform;

            UnityEngine.Object.DestroyImmediate( (Component)vessel.PhysicsTransform );
            var ppo = vessel.gameObject.AddComponent<HybridReferenceFrameTransform>();
            ppo.SceneReferenceFrameProvider = sceneFrameProvider;
            ppo.PositionRange = OnVesselCreated.VESSEL_POSITION_RANGE;
            ppo.VelocityRange = OnVesselCreated.VESSEL_VELOCITY_RANGE;
            ppo.MaxTimeScale = OnVesselCreated.VESSEL_MAX_TIMESCALE;
            ppo.AllowSceneSimulation = true;

            ppo.Mass = oldPhysTransform.Mass;
            ppo.LocalCenterOfMass = oldPhysTransform.LocalCenterOfMass;
            ppo.MomentsOfInertia = oldPhysTransform.MomentsOfInertia;
            ppo.MomentsOfInertiaRotation = oldPhysTransform.MomentsOfInertiaRotation;

            TrajectoryTransform tt = vessel.gameObject.GetComponent<TrajectoryTransform>();
            tt.enabled = true;
        }
    }
}