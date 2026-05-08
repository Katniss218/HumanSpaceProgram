using UnityEngine;

namespace HSP.ReferenceFrames
{
    public static class IReferenceFrameTransform_Ex
    {
        // ---- Internal helpers ----

        private static IReferenceFrame SceneFrame( IReferenceFrameTransform t ) => t.SceneReferenceFrameProvider.GetSceneReferenceFrame();

        // ---- Scene-space ----

        /// <summary>
        /// Returns the position of this transform in the current scene reference frame. <br/>
        /// Includes a fast path for when the internal state is already stored in the scene frame, to avoid an unnecessary transformation. <br/>
        /// </summary>
        public static Vector3 GetPosition( this IReferenceFrameTransform self )
        {
            IReferenceFrame sceneFrame = SceneFrame( self );
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( ReferenceEquals( storedFrame, sceneFrame ) )
                return (Vector3)s.Position;

            return (Vector3)s.InFrame( sceneFrame ).Position;
        }

        /// <summary>
        /// Sets the position of this transform in the current scene reference frame.
        /// </summary>
        public static void SetPosition( this IReferenceFrameTransform self, Vector3 value )
        {
            self.ModifyState( SceneFrame( self ), ( ref KinematicState state ) =>
            {
                state.Position = (Vector3Dbl)value;
            } );
        }

        public static Quaternion GetRotation( this IReferenceFrameTransform self )
        {
            IReferenceFrame sceneFrame = SceneFrame( self );
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( ReferenceEquals( storedFrame, sceneFrame ) )
                return (Quaternion)s.Rotation;

            return (Quaternion)s.InFrame( sceneFrame ).Rotation;
        }

        public static void SetRotation( this IReferenceFrameTransform self, Quaternion value )
        {
            self.ModifyState( SceneFrame( self ), ( ref KinematicState state ) =>
            {
                state.Rotation = (QuaternionDbl)value;
            } );
        }

        public static Vector3 GetVelocity( this IReferenceFrameTransform self )
        {
            IReferenceFrame sceneFrame = SceneFrame( self );
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( ReferenceEquals( storedFrame, sceneFrame ) )
                return (Vector3)s.Velocity;

            return (Vector3)s.InFrame( sceneFrame ).Velocity;
        }

        public static void SetVelocity( this IReferenceFrameTransform self, Vector3 value )
        {
            self.ModifyState( SceneFrame( self ), ( ref KinematicState state ) =>
            {
                state.Velocity = (Vector3Dbl)value;
            } );
        }

        public static Vector3 GetAngularVelocity( this IReferenceFrameTransform self )
        {
            IReferenceFrame sceneFrame = SceneFrame( self );
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( ReferenceEquals( storedFrame, sceneFrame ) )
                return (Vector3)s.AngularVelocity;

            return (Vector3)s.InFrame( sceneFrame ).AngularVelocity;
        }

        public static void SetAngularVelocity( this IReferenceFrameTransform self, Vector3 value )
        {
            self.ModifyState( SceneFrame( self ), ( ref KinematicState state ) =>
            {
                state.AngularVelocity = (Vector3Dbl)value;
            } );
        }

        public static Vector3 GetAcceleration( this IReferenceFrameTransform self )
        {
            IReferenceFrame sceneFrame = SceneFrame( self );
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( ReferenceEquals( storedFrame, sceneFrame ) )
                return (Vector3)s.Acceleration;

            return (Vector3)s.InFrame( sceneFrame ).Acceleration;
        }

        public static void SetAcceleration( this IReferenceFrameTransform self, Vector3 value )
        {
            self.ModifyState( SceneFrame( self ), ( ref KinematicState state ) =>
            {
                state.Acceleration = (Vector3Dbl)value;
            } );
        }

        public static Vector3 GetAngularAcceleration( this IReferenceFrameTransform self )
        {
            IReferenceFrame sceneFrame = SceneFrame( self );
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( ReferenceEquals( storedFrame, sceneFrame ) )
                return (Vector3)s.AngularAcceleration;

            return (Vector3)s.InFrame( sceneFrame ).AngularAcceleration;
        }

        public static void SetAngularAcceleration( this IReferenceFrameTransform self, Vector3 value )
        {
            self.ModifyState( SceneFrame( self ), ( ref KinematicState state ) =>
            {
                state.AngularAcceleration = (Vector3Dbl)value;
            } );
        }

        // ---- Absolute ----

        public static Vector3Dbl GetAbsolutePosition( this IReferenceFrameTransform self )
        {
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( storedFrame == null )
                return s.Position;

            return s.InFrame( null ).Position;
        }

        public static void SetAbsolutePosition( this IReferenceFrameTransform self, Vector3Dbl value )
        {
            self.ModifyState( null, ( ref KinematicState state ) =>
            {
                state.Position = value;
            } );
        }

        public static QuaternionDbl GetAbsoluteRotation( this IReferenceFrameTransform self )
        {
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( storedFrame == null )
                return s.Rotation;

            return s.InFrame( null ).Rotation;
        }

        public static void SetAbsoluteRotation( this IReferenceFrameTransform self, QuaternionDbl value )
        {
            self.ModifyState( null, ( ref KinematicState state ) =>
            {
                state.Rotation = value;
            } );
        }

        public static Vector3Dbl GetAbsoluteVelocity( this IReferenceFrameTransform self )
        {
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( storedFrame == null )
                return s.Velocity;

            return s.InFrame( null ).Velocity;
        }

        public static void SetAbsoluteVelocity( this IReferenceFrameTransform self, Vector3Dbl value )
        {
            self.ModifyState( null, ( ref KinematicState state ) =>
            {
                state.Velocity = value;
            } );
        }

        public static Vector3Dbl GetAbsoluteAngularVelocity( this IReferenceFrameTransform self )
        {
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( storedFrame == null )
                return s.AngularVelocity;

            return s.InFrame( null ).AngularVelocity;
        }

        public static void SetAbsoluteAngularVelocity( this IReferenceFrameTransform self, Vector3Dbl value )
        {
            self.ModifyState( null, ( ref KinematicState state ) =>
            {
                state.AngularVelocity = value;
            } );
        }

        public static Vector3Dbl GetAbsoluteAcceleration( this IReferenceFrameTransform self )
        {
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( storedFrame == null )
                return s.Acceleration;

            return s.InFrame( null ).Acceleration;
        }

        public static void SetAbsoluteAcceleration( this IReferenceFrameTransform self, Vector3Dbl value )
        {
            self.ModifyState( null, ( ref KinematicState state ) =>
            {
                state.Acceleration = value;
            } );
        }

        public static Vector3Dbl GetAbsoluteAngularAcceleration( this IReferenceFrameTransform self )
        {
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            if( storedFrame == null )
                return s.AngularAcceleration;

            return s.InFrame( null ).AngularAcceleration;
        }

        public static void SetAbsoluteAngularAcceleration( this IReferenceFrameTransform self, Vector3Dbl value )
        {
            self.ModifyState( null, ( ref KinematicState state ) =>
            {
                state.AngularAcceleration = value;
            } );
        }

        // ---- Bulk setters ----

        public static void SetAbsoluteState(
            this IReferenceFrameTransform self,
            Vector3Dbl? position = null,
            QuaternionDbl? rotation = null,
            Vector3Dbl? velocity = null,
            Vector3Dbl? angularVelocity = null,
            Vector3Dbl? acceleration = null,
            Vector3Dbl? angularAcceleration = null )
        {
            self.ModifyState( null, ( ref KinematicState state ) =>
            {
                if( position.HasValue ) state.Position = position.Value;
                if( rotation.HasValue ) state.Rotation = rotation.Value;
                if( velocity.HasValue ) state.Velocity = velocity.Value;
                if( angularVelocity.HasValue ) state.AngularVelocity = angularVelocity.Value;
                if( acceleration.HasValue ) state.Acceleration = acceleration.Value;
                if( angularAcceleration.HasValue ) state.AngularAcceleration = angularAcceleration.Value;
            } );
        }

        public static void SetSceneState(
            this IReferenceFrameTransform self,
            Vector3? position = null,
            Quaternion? rotation = null,
            Vector3? velocity = null,
            Vector3? angularVelocity = null,
            Vector3? acceleration = null,
            Vector3? angularAcceleration = null )
        {
            self.ModifyState( SceneFrame( self ), ( ref KinematicState state ) =>
            {
                if( position.HasValue ) state.Position = (Vector3Dbl)position.Value;
                if( rotation.HasValue ) state.Rotation = (QuaternionDbl)rotation.Value;
                if( velocity.HasValue ) state.Velocity = (Vector3Dbl)velocity.Value;
                if( angularVelocity.HasValue ) state.AngularVelocity = (Vector3Dbl)angularVelocity.Value;
                if( acceleration.HasValue ) state.Acceleration = (Vector3Dbl)acceleration.Value;
                if( angularAcceleration.HasValue ) state.AngularAcceleration = (Vector3Dbl)angularAcceleration.Value;
            } );
        }

        /// <summary>
        /// Constructs a reference frame centered on this object, with axes aligned with the absolute frame.
        /// </summary>
        public static IReferenceFrame CenteredReferenceFrame( this IReferenceFrameTransform self )
        {
            IReferenceFrame sceneFrame = SceneFrame( self );
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            Vector3Dbl absPos = (storedFrame == null) ? s.Position : s.InFrame( null ).Position;

            return new CenteredReferenceFrame(
                sceneFrame.ReferenceUT,
                absPos );
        }

        /// <summary>
        /// Constructs a reference frame centered on this object, with axes aligned with the absolute frame, and the frame's velocity matching that of the object.
        /// </summary>
        public static IReferenceFrame CenteredInertialReferenceFrame( this IReferenceFrameTransform self )
        {
            IReferenceFrame sceneFrame = SceneFrame( self );
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            KinematicState abs = (storedFrame == null) ? s : s.InFrame( null );

            return new CenteredInertialReferenceFrame(
                sceneFrame.ReferenceUT,
                abs.Position,
                abs.Velocity );
        }

        /// <summary>
        /// Constructs a reference frame centered on this object, with axes aligned with the object (i.e. local space).
        /// </summary>
        public static IReferenceFrame OrientedReferenceFrame( this IReferenceFrameTransform self )
        {
            IReferenceFrame sceneFrame = SceneFrame( self );
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            KinematicState abs = (storedFrame == null) ? s : s.InFrame( null );

            return new OrientedReferenceFrame(
                sceneFrame.ReferenceUT,
                abs.Position,
                abs.Rotation );
        }

        /// <summary>
        /// Constructs a reference frame centered on this object, with axes aligned with the object (i.e. local space), and the frame's velocity matching that of the object.
        /// </summary>
        public static IReferenceFrame OrientedInertialReferenceFrame( this IReferenceFrameTransform self )
        {
            IReferenceFrame sceneFrame = SceneFrame( self );
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            KinematicState abs = (storedFrame == null) ? s : s.InFrame( null );

            return new OrientedInertialReferenceFrame(
                sceneFrame.ReferenceUT,
                abs.Position,
                abs.Rotation,
                abs.Velocity );
        }

        /// <summary>
        /// Constructs a non-inertial reference frame centered on this object, with axes aligned with the object (i.e. local space), and the velocities/accelerations matching the instantaneous values for the current moment in time.
        /// </summary>
        public static INonInertialReferenceFrame NonInertialReferenceFrame( this IReferenceFrameTransform self )
        {
            IReferenceFrame sceneFrame = SceneFrame( self );
            ref readonly KinematicState s = ref self.GetStateRef( out IReferenceFrame storedFrame );
            KinematicState abs = (storedFrame == null) ? s : s.InFrame( null );

            return new OrientedNonInertialReferenceFrame(
                sceneFrame.ReferenceUT,
                abs.Position,
                abs.Rotation,
                abs.Velocity,
                abs.AngularVelocity,
                abs.Acceleration,
                abs.AngularAcceleration );
        }
    }
}