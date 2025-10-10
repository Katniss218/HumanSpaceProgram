
namespace HSP.ReferenceFrames
{
    public static class IReferenceFrameTransform_Ex
    {
        /// <summary>
        /// Constructs a reference frame centered on this object, with axes aligned with the absolute frame.
        /// </summary>
        public static IReferenceFrame CenteredReferenceFrame( this IReferenceFrameTransform self ) => new CenteredReferenceFrame( self.SceneReferenceFrameProvider.GetSceneReferenceFrame().ReferenceUT, self.AbsolutePosition );

        /// <summary>
        /// Constructs a reference frame centered on this object, with axes aligned with the absolute frame, and the frame's velocity matching that of the object.
        /// </summary>
        public static IReferenceFrame CenteredInertialReferenceFrame( this IReferenceFrameTransform self ) => new CenteredInertialReferenceFrame( self.SceneReferenceFrameProvider.GetSceneReferenceFrame().ReferenceUT, self.AbsolutePosition, self.AbsoluteVelocity );

        /// <summary>
        /// Constructs a reference frame centered on this object, with axes aligned with the object (i.e. local space).
        /// </summary>
        public static IReferenceFrame OrientedReferenceFrame( this IReferenceFrameTransform self ) => new OrientedReferenceFrame( self.SceneReferenceFrameProvider.GetSceneReferenceFrame().ReferenceUT, self.AbsolutePosition, self.AbsoluteRotation );

        /// <summary>
        /// Constructs a reference frame centered on this object, with axes aligned with the object (i.e. local space), and the frame's velocity matching that of the object.
        /// </summary>
        public static IReferenceFrame OrientedInertialReferenceFrame( this IReferenceFrameTransform self ) => new OrientedInertialReferenceFrame( self.SceneReferenceFrameProvider.GetSceneReferenceFrame().ReferenceUT, self.AbsolutePosition, self.AbsoluteRotation, self.AbsoluteVelocity );

        /// <summary>
        /// Constructs a non-inertial reference frame centered on this object, with axes aligned with the object (i.e. local space), and the velocities/accelerations matching the instantaneous values for the current moment in time.
        /// </summary>
        public static INonInertialReferenceFrame NonInertialReferenceFrame( this IReferenceFrameTransform self ) => new OrientedNonInertialReferenceFrame( self.SceneReferenceFrameProvider.GetSceneReferenceFrame().ReferenceUT, self.AbsolutePosition, self.AbsoluteRotation, self.AbsoluteVelocity, self.AbsoluteAngularVelocity, self.AbsoluteAcceleration, self.AbsoluteAngularAcceleration );
    }
}