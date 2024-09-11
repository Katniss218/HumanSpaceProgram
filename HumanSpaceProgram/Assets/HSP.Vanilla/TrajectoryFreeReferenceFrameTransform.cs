using HSP.Time;
using HSP.Trajectories;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    public class TrajectoryFreeReferenceFrameTransform : FreeReferenceFrameTransform
    {
        public ITrajectory Trajectory { get; private set; }

        protected override void RecalculateCache()
        {
#warning TODO - need a way to communicate to the simulator that "hey, I need the properly computed position for the current time", regardless if it has been computed or not yet.
            Trajectory.ProlongToUT( TimeManager.UT );
            base.RecalculateCache();
        }

        protected override void MakeCacheInvalid()
        {
            Trajectory.InvalidateCache();
            base.MakeCacheInvalid();
        }

        protected override void FixedUpdate()
        {
            // update trajectory.

            // if collision or force is present, reject.
            base.FixedUpdate();
        }

        [MapsInheritingFrom( typeof( TrajectoryFreeReferenceFrameTransform ) )]
        public static SerializationMapping TrajectoryKinematicReferenceFrameTransformMapping()
        {
            return new MemberwiseSerializationMapping<TrajectoryFreeReferenceFrameTransform>()
            {
                ("trajectory", new Member<TrajectoryFreeReferenceFrameTransform, ITrajectory>( o => o.Trajectory )),
            };
        }
    }
}