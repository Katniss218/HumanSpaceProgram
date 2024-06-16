using KSS.Components;
using KSS.Control;
using KSS.Control.Controls;
using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    /// <summary>
    /// Represents an invokable element of a <see cref="FSequencer"/>'s sequence.
    /// </summary>
    public abstract class SequenceElement : ControlGroup
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

        [SerializationMappingProvider( typeof( SequenceElement ) )]
        public static SerializationMapping SequenceElementMapping()
        {
            return new MemberwiseSerializationMapping<SequenceElement>()
            {
                ("actions", new Member<SequenceElement, SequenceActionBase[]>( o => o.Actions.ToArray(), (o, value) => o.Actions = value.ToList() ))
            };
        }
    }

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

        [SerializationMappingProvider( typeof( KeyboardSequenceElement ) )]
        public static SerializationMapping KeyboardSequenceElementMapping()
        {
            return new MemberwiseSerializationMapping<KeyboardSequenceElement>()
            {
                ("key", new Member<KeyboardSequenceElement, KeyCode>( o => o.Key ))
            };
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

        [SerializationMappingProvider( typeof( TimedSequenceElement ) )]
        public static SerializationMapping TimedSequenceElementMapping()
        {
            return new MemberwiseSerializationMapping<TimedSequenceElement>()
            {
                ("delay", new Member<TimedSequenceElement, float>( o => o.Delay )),
                ("start_ut", new Member<TimedSequenceElement, double>( o => o._startUT ))
            };
        }
    }
}