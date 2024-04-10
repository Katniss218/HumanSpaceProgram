using KSS.Control.Controls;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    public class KeyboardSequenceElement : Sequence.Element
    {
        public KeyCode Key { get; set; } = KeyCode.Space;

        public override void Initialize()
        {
        }

        public override bool CanInvoke()
        {
            return UnityEngine.Input.GetKey( Key );
        }
    }

    public class TimedSequenceElement : Sequence.Element
    {
        /// <summary>
        /// The delay from the firing of the previous element, after which the sequence element should fire.
        /// </summary>
        public float Delay { get; set; }

        float _startTimestamp;
        float _timeSinceStart => _startTimestamp - Time.time;

        public override void Initialize()
        {
            _startTimestamp = Time.time;
        }

        public override bool CanInvoke()
        {
            return _timeSinceStart >= Delay;
        }
    }

    public abstract class SequencerOutput //: ControllerOutput
    {
        public abstract void TryInvoke();
    }

    public class SequencerOutput<T> : SequencerOutput
    {
        // sequencer action *holds* the parameters that will be used when invoking. It is in principle very simple.

        // should be able to be connected to the controlleeinput<T>

        ControlleeInput<T> _input;

        public MethodInfo varargMethod;
        public object target;
        public object[] parameters;

        public override void TryInvoke()
        {
            try
            {
                varargMethod.Invoke( target, parameters );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"Tried to invoke a sequence element." );
                Debug.LogException( ex );
            }
        }
    }

    public class Sequence
    {
        public abstract class Element
        {
            /// <summary>
            /// The actions that this sequence element will call when it's fired.
            /// </summary>
            public List<SequencerOutput> Actions { get; private set; } = new List<SequencerOutput>();

            /// <summary>
            /// Called when the previous action is triggerred, or on load (the first element).
            /// </summary>
            public abstract void Initialize();

            /// <summary>
            /// Called to check if the sequence element can fire in this frame.
            /// </summary>
            public abstract bool CanInvoke();

            /// <summary>
            /// Called to fire the sequence element.
            /// </summary>
            public void Invoke()
            {
                foreach( var action in Actions )
                {
                    action.TryInvoke();
                }
            }
        }

        public List<Element> Elements { get; private set; } = new List<Element>();

        public int Current { get; private set; } = 0;

        /// <summary>
        /// Tries to initialize the current element of the sequence.
        /// </summary>
        public bool TryInitialize()
        {
            if( Current < 0 || Current >= Elements.Count )
            {
                return false;
            }

            try
            {
                Elements[Current].Initialize();
                return true;
            }
            catch( Exception ex )
            {
                Debug.LogException( ex );
            }
            return false;
        }

        /// <summary>
        /// Tries to invoke the current element of the sequence.
        /// </summary>
        public bool TryInvoke()
        {
            if( Current < 0 || Current >= Elements.Count )
            {
                return false;
            }

            Element elem = Elements[Current];

            try
            {
                if( elem.CanInvoke() )
                {
                    elem.Invoke();
                    Current++;

                    TryInitialize(); // Initialize the next element.
                    return true;
                }
            }
            catch( Exception ex )
            {
                Debug.LogException( ex );
            }
            return false;
        }
    }

    /// <summary>
    /// Represents a controller that can invoke an arbitrary control action from a queue.
    /// </summary>
    public class FSequencer : MonoBehaviour, IPersistsObjects, IPersistsData
    {
        // sequencer is a type of avionics, related to the control system.

        public List<Sequence> Sequences { get; private set; } = new List<Sequence>();

        void Start()
        {
            foreach( var seq in Sequences )
            {
                seq.TryInitialize();
            }
        }

        void Update()
        {
            foreach( var seq in Sequences )
            {
                seq.TryInvoke();
            }
        }

        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {

            };
        }

        public void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {

        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            //ret.AddAll( new SerializedObject()

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            //
        }
    }
}