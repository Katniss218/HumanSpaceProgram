using KSS.Control;
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
    public class FTestAvionics : MonoBehaviour
    {
        // test avionics to test the avionics system.

        // avionics will be a complicated and very broad system with many different subsystems, subcomponents, etc.

        // a control part (e.g. probe core) will have multiple control components (modules). One of those will almost always be a sequencer.




        // avionics will have internal channels that activate based on "things"

        private float _pitchAxis; // channel representing the pitch control signal.
        private float Throttle;

        [ControlOut( "throttle", "Throttle" )]
        private event Action<float> OnSetThrottle;

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_DOWN, HierarchicalInputPriority.MEDIUM, Input_PitchDown );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_UP, HierarchicalInputPriority.MEDIUM, Input_PitchUp );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, HierarchicalInputPriority.MEDIUM, Input_FullThrottle );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, HierarchicalInputPriority.MEDIUM, Input_CutThrottle );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_DOWN, Input_PitchDown );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_UP, Input_PitchUp );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, Input_FullThrottle );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, Input_CutThrottle );
        }

        private bool Input_FullThrottle()
        {
            Throttle = 1.0f;
            return false;
        }

        private bool Input_CutThrottle()
        {
            Throttle = 0.0f;
            return false;
        }


#warning TODO - if not pressed - set to 0, figure out a good way of doing that.

        bool Input_PitchUp()
        {
            _pitchAxis = 1.0f;
            return false;
        }

        bool Input_PitchDown()
        {
            _pitchAxis = -1.0f;
            return false;
        }

        void FixedUpdate()
        {
        }
    }
}