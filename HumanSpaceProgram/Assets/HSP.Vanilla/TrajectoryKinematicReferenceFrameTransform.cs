using HSP.Trajectories;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    public class TrajectoryKinematicReferenceFrameTransform : KinematicReferenceFrameTransform
    {
        public ITrajectory Trajectory { get; private set; }

        protected override void FixedUpdate()
        {
            // update trajectory.

            // if collision or force is present, reject.
            base.FixedUpdate();
        }

        [MapsInheritingFrom( typeof( TrajectoryKinematicReferenceFrameTransform ) )]
        public static SerializationMapping TrajectoryKinematicReferenceFrameTransformMapping()
        {
            return new MemberwiseSerializationMapping<TrajectoryKinematicReferenceFrameTransform>()
            {
                ("trajectory", new Member<TrajectoryKinematicReferenceFrameTransform, ITrajectory>( o => o.Trajectory )),
            };
        }
    }
}