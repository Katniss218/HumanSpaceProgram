using KSS.Control;
using KSS.Control.Controls;
using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    public class KeyboardSequenceElement : SequenceElement
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

    public class TimedSequenceElement : SequenceElement
    {
        /// <summary>
        /// The delay, in [s], from the firing of the previous element, after which the sequence element should fire.
        /// </summary>
        public float Delay { get; set; }

        private double _startUT;

        public override void Initialize()
        {
            _startUT = TimeStepManager.UT;
        }

        public override bool CanInvoke()
        {
            return TimeStepManager.UT >= _startUT + Delay;
        }
    }

    public abstract class SequenceActionBase : ControlGroup // pass-through group with a single element. Required to be drawn.
    {
        public abstract ControllerOutputBase OnInvoke { get; }
        public abstract void TryInvoke();
    }

    /// <summary>
    /// The sequence action without a parameter.
    /// </summary>
    public class SequenceAction : SequenceActionBase, IPersistsObjects
    {
        public override ControllerOutputBase OnInvoke => OnInvokeTyped;

        [NamedControl( "x", Editable = false )]
        public ControllerOutput OnInvokeTyped;

        public override void TryInvoke()
        {
            OnInvokeTyped.TrySendSignal();
        }

        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {

            };
        }

        public void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            OnInvokeTyped = new ControllerOutput();
        }
    }

    /// <summary>
    /// The sequence action with a parameter of type T.
    /// </summary>
    public class SequenceAction<T> : SequenceActionBase, IPersistsObjects
    {
        public override ControllerOutputBase OnInvoke => OnInvokeTyped;

        [NamedControl( "x", Editable = false )]
        public ControllerOutput<T> OnInvokeTyped;

        public T SignalValue { get; set; }

        public override void TryInvoke()
        {
            OnInvokeTyped.TrySendSignal( SignalValue );
        }

        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {

            };
        }

        public void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            OnInvokeTyped = new ControllerOutput<T>();
        }
    }

    public abstract class SequenceElement : ControlGroup, IPersistsObjects
    {
        [NamedControl( "Actions", "", Editable = false )]
        /// <summary>
        /// The actions that this sequence element will call when it's fired.
        /// </summary>
        public List<SequenceActionBase> Actions = new();

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

        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {

            };
        }

        public void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            Actions = new List<SequenceActionBase>()
            {
                new SequenceAction<float>(),
                new SequenceAction<Vector3>()
            };

            Actions[0].SetObjects( null, l );
            Actions[1].SetObjects( null, l );
        }
    }

    public class Sequence : ControlGroup, IPersistsObjects
    {
        [NamedControl( "Elements", Editable = false )]
        public List<SequenceElement> Elements = new();

        public IEnumerable<SequenceElement> InvokedElements => Elements.Take( Current );
        
        public IEnumerable<SequenceElement> RemainingElements => Elements.Skip( Current );

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

            SequenceElement elem = Elements[Current];

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

        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {

            };
        }

        public void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            Elements = new List<SequenceElement>()
            {
                new KeyboardSequenceElement(),
                new KeyboardSequenceElement(),
                new KeyboardSequenceElement()
            };

            Elements[0].SetObjects( null, l );
            Elements[1].SetObjects( null, l );
            Elements[2].SetObjects( null, l );
        }
    }

    /// <summary>
    /// Represents a controller that can invoke an arbitrary control action from a queue.
    /// </summary>
    public class FSequencer : MonoBehaviour, IPersistsObjects, IPersistsData
    {
        // sequencer is a type of avionics, related to the control system.

        [NamedControl( "Sequence", Editable = false )]
        public Sequence Sequence = new Sequence();

        public Action OnAfterInvoked;

        // control group nest structure:

        // sequence
        // - element
        // - - action
        // - - - output*
        // - - action
        // - - - output*
        // - - ...
        // - element
        // - - action
        // - - - output*
        // - - action
        // - - - output*
        // - - ...
        // - ...

        void Start()
        {
            Sequence.TryInitialize();
        }

        void Update()
        {
            if( Sequence.TryInvoke() )
            {
                OnAfterInvoked?.Invoke();
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
            Sequence.SetObjects( null, l );
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