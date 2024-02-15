using KSS.Control;
using KSS.Control.Controls;
using KSS.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Input;

namespace KSS.Components
{
    /// <summary>
    /// Sends steering and throttle signals based on player input.
    /// </summary>
    public class FPlayerInputAvionics : MonoBehaviour
    {
        // avionics will be a complicated and very broad system with many different subsystems, subcomponents, etc.

        // a control part (e.g. probe core) will have multiple control components (modules). One of those will almost always be a sequencer.




        // avionics will have internal channels that activate based on "things"

        private float _pitchSignal; // channel representing the pitch control signal.
        private float _yawSignal; // channel representing the yaw control signal.
        private float _rollSignal; // channel representing the roll control signal.
        private float _throttleSignal;

        [NamedControl( "Throttle" )]
        public ControllerOutput<float> OnSetThrottle = new();

        [NamedControl( "Steering Command" )]
        public ControllerOutput<Vector3> OnSteer = new(); // pitch, yaw, roll

		void OnEnable()
        {
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_DOWN, HierarchicalInputPriority.MEDIUM, Input_PitchDown );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_UP, HierarchicalInputPriority.MEDIUM, Input_PitchUp );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW_LEFT, HierarchicalInputPriority.MEDIUM, Input_YawLeft );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW_RIGHT, HierarchicalInputPriority.MEDIUM, Input_YawRight );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL_LEFT, HierarchicalInputPriority.MEDIUM, Input_RollLeft );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL_RIGHT, HierarchicalInputPriority.MEDIUM, Input_RollRight );

            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, HierarchicalInputPriority.MEDIUM, Input_FullThrottle );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, HierarchicalInputPriority.MEDIUM, Input_CutThrottle );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_DOWN, Input_PitchDown );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_UP, Input_PitchUp );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW_LEFT, Input_YawLeft );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW_RIGHT, Input_YawRight );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL_LEFT, Input_RollLeft );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL_RIGHT, Input_RollRight );

            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, Input_FullThrottle );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, Input_CutThrottle );
        }

        private bool Input_FullThrottle()
        {
            _throttleSignal = 1.0f;

            OnSetThrottle.TrySendSignal( _throttleSignal );
            return false;
        }

        private bool Input_CutThrottle()
        {
            _throttleSignal = 0.0f;

            OnSetThrottle.TrySendSignal( _throttleSignal );
            return false;
        }


#warning TODO - if not pressed - set to 0, figure out a good way of doing that. This should be done with control channels that can use multiple keys and invoke methods with a parameter (axes)

        bool Input_PitchUp()
        {
            _pitchSignal += 1.0f;

            OnSteer.TrySendSignal( new Vector3( _pitchSignal, _yawSignal, _rollSignal ) );
            return false;
        }

        bool Input_PitchDown()
        {
            _pitchSignal -= 1.0f;

            OnSteer.TrySendSignal( new Vector3(_pitchSignal, _yawSignal, _rollSignal ) );
            return false;
        }

        bool Input_YawRight()
        {
            _yawSignal += 1.0f;

            OnSteer.TrySendSignal( new Vector3( _pitchSignal, _yawSignal, _rollSignal ) );
            return false;
        }

        bool Input_YawLeft()
        {
            _yawSignal -= 1.0f;

            OnSteer.TrySendSignal( new Vector3( _pitchSignal, _yawSignal, _rollSignal ) );
            return false;
        }

        bool Input_RollRight()
        {
            _rollSignal += 1.0f;

            OnSteer.TrySendSignal( new Vector3( _pitchSignal, _yawSignal, _rollSignal ) );
            return false;
        }

        bool Input_RollLeft()
        {
            _rollSignal -= 1.0f;

            OnSteer.TrySendSignal( new Vector3( _pitchSignal, _yawSignal, _rollSignal ) );
            return false;
        }
    }
}