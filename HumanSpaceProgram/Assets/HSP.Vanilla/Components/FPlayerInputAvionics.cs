using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using HSP.Input;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    /// <summary>
    /// Sends steering and throttle signals based on player input.
    /// </summary>
    public class FPlayerInputAvionics : MonoBehaviour
    {
        public FControlFrame ControlFrame { get; set; }

        private float _pitchSignal;
        private float _yawSignal;
        private float _rollSignal;
        private float _throttleSignal;

        private Vector3 _lastControlSignal;

        [field: SerializeField]
        public Vector3 AttitudeSensitivity { get; set; } = Vector3.one;
        [field: SerializeField]
        public Vector3 TranslationSensitivity { get; set; } = Vector3.one;

        /// <summary>
        /// Desired throttle level, in [0..1].
        /// </summary>
        [NamedControl( "Throttle" )]
        public ControllerOutput<float> OnSetThrottle = new();

        /// <summary>
        /// Desired vessel-space (pitch, yaw, roll) attitude change, in [-Inf..Inf].
        /// </summary>
        [NamedControl( "Attitude" )]
        public ControllerOutput<Vector3> OnSetAttitude = new();

        /// <summary>
        /// Desired scene-space (X, Y, Z) position change, in [-Inf..Inf].
        /// </summary>
        [NamedControl( "Translation" )]
        public ControllerOutput<Vector3> OnSetTranslation = new();

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH, HierarchicalInputPriority.MEDIUM, Input_Pitch );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW, HierarchicalInputPriority.MEDIUM, Input_Yaw );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL, HierarchicalInputPriority.MEDIUM, Input_Roll );

            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, HierarchicalInputPriority.MEDIUM, Input_FullThrottle );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, HierarchicalInputPriority.MEDIUM, Input_CutThrottle );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH, Input_Pitch );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW, Input_Yaw );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL, Input_Roll );

            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, Input_FullThrottle );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, Input_CutThrottle );
        }

        private bool Input_FullThrottle( float value )
        {
            _throttleSignal = 1.0f;

            OnSetThrottle.TrySendSignal( _throttleSignal );
            return false;
        }

        private bool Input_CutThrottle( float value )
        {
            _throttleSignal = 0.0f;

            OnSetThrottle.TrySendSignal( _throttleSignal );
            return false;
        }

        bool Input_Pitch( float value )
        {
            _pitchSignal = value * AttitudeSensitivity.x;

            Vector3 controlSignal = new Vector3( _pitchSignal, _yawSignal, _rollSignal );

            if( Mathf.Abs( _pitchSignal ) > 0.01f || controlSignal != _lastControlSignal )
            {
                OnSetAttitude.TrySendSignal( controlSignal );
                _lastControlSignal = controlSignal;
            }

            return false;
        }

        bool Input_Yaw( float value )
        {
            _yawSignal = value * AttitudeSensitivity.y;

            Vector3 controlSignal = new Vector3( _pitchSignal, _yawSignal, _rollSignal );

            if( Mathf.Abs( _yawSignal ) > 0.01f || controlSignal != _lastControlSignal )
            {
                OnSetAttitude.TrySendSignal( controlSignal );
                _lastControlSignal = controlSignal;
            }
            return false;
        }

        bool Input_Roll( float value )
        {
            _rollSignal = value * AttitudeSensitivity.z;

            Vector3 controlSignal = new Vector3( _pitchSignal, _yawSignal, _rollSignal );

            if( Mathf.Abs( _rollSignal ) > 0.01f || controlSignal != _lastControlSignal )
            {
                OnSetAttitude.TrySendSignal( controlSignal );
                _lastControlSignal = controlSignal;
            }
            return false;
        }


        [MapsInheritingFrom( typeof( FPlayerInputAvionics ) )]
        public static SerializationMapping FPlayerInputAvionicsMapping()
        {
            return new MemberwiseSerializationMapping<FPlayerInputAvionics>()
                .WithMember( "control_frame", ObjectContext.Ref, o => o.ControlFrame )
                .WithMember( "on_set_throttle", o => o.OnSetThrottle )
                .WithMember( "on_set_attitude", o => o.OnSetAttitude )
                .WithMember( "on_set_translation", o => o.OnSetTranslation );
        }
    }
}