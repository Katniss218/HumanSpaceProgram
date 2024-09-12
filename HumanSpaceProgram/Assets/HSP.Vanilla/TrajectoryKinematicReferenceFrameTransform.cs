using Assets.HSP.Trajectories;
using HSP.Trajectories;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    public class TrajectoryKinematicReferenceFrameTransform : KinematicReferenceFrameTransform
    {
        bool _isAttractor;
        ITrajectory _trajectory;

        protected override void MakeCacheInvalid()
        {
            _trajectory.SetPositionAndRotation();
            base.MakeCacheInvalid();
        }

        protected override void OnEnable()
        {
            if( _isAttractor )
                TrajectoryManager.RegisterAttractor( _trajectory, this );
            else
                TrajectoryManager.RegisterFollower( _trajectory, this );
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if( _isAttractor )
                TrajectoryManager.UnregisterAttractor( _trajectory );
            else
                TrajectoryManager.UnregisterFollower( _trajectory );
            base.OnDisable();
        }

        [MapsInheritingFrom( typeof( TrajectoryKinematicReferenceFrameTransform ) )]
        public static SerializationMapping TrajectoryKinematicReferenceFrameTransformMapping()
        {
            return new MemberwiseSerializationMapping<TrajectoryKinematicReferenceFrameTransform>()
            {
                ("trajectory", new Member<TrajectoryKinematicReferenceFrameTransform, ITrajectory>( o => o._trajectory )),
            };
        }
    }
}