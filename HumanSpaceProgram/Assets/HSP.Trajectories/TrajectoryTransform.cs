using Assets.HSP.Trajectories;
using HSP.ReferenceFrames;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories
{
    /// <summary>
    /// Makes an object follow a trajectory.
    /// </summary>
    public class TrajectoryTransform : MonoBehaviour
    {
        IPhysicsTransform _physicsTransform;
        public IPhysicsTransform PhysicsTransform
        {
            get
            {
                if( _physicsTransform.IsUnityNull() )
                    _physicsTransform = this.GetComponent<IPhysicsTransform>();
                return _physicsTransform;
            }
        }

        IReferenceFrameTransform _preferenceFrameTransform;
        public IReferenceFrameTransform ReferenceFrameTransform
        {
            get
            {
                if( _preferenceFrameTransform.IsUnityNull() )
                    _preferenceFrameTransform = this.GetComponent<IReferenceFrameTransform>();
                return _preferenceFrameTransform;
            }
        }

        bool _isAttractor;
        public bool IsAttractor
        {
            get => _isAttractor;
            set
            {
                if( _isAttractor == value )
                    return;

                TryUnregister();
                _isAttractor = value;
                TryRegister();
            }
        }

        ITrajectory _trajectory;
        public ITrajectory Trajectory
        {
            get => _trajectory;
            set
            {
                TryUnregister();
                _trajectory = value;
                TryRegister();
            }
        }

        void OnEnable()
        {
            TryRegister();
        }

        void OnDisable()
        {
            TryUnregister();
        }

        private void TryRegister()
        {
            if( _isAttractor )
                TrajectoryManager.TryRegisterAttractor( _trajectory, this );
            else
                TrajectoryManager.TryRegisterFollower( _trajectory, this );
        }

        private void TryUnregister()
        {
            if( _isAttractor )
                TrajectoryManager.TryUnregisterAttractor( _trajectory );
            else
                TrajectoryManager.TryUnregisterFollower( _trajectory );
        }

        [MapsInheritingFrom( typeof( TrajectoryTransform ) )]
        public static SerializationMapping TrajectoryKinematicReferenceFrameTransformMapping()
        {
            return new MemberwiseSerializationMapping<TrajectoryTransform>()
            {
                ("trajectory", new Member<TrajectoryTransform, ITrajectory>( o => o._trajectory )),
            };
        }
    }
}
