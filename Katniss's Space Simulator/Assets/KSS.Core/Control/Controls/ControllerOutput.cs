using System;
using UnityPlus.Serialization;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;

namespace KSS.Control.Controls
{
    /// <summary>
    /// Represents a control that produces an empty control signal.
    /// </summary>
    public sealed class ControllerOutput : ControllerOutputBase
    {
        public ControlleeInput Input { get; internal set; }

        public ControllerOutput()
        {
        }

        /// <summary>
        /// Tries to send a control signal from this controller to its connected inputs.
        /// </summary>
        public void TrySendSignal()
        {
            Input?.onInvoke.Invoke();
        }

        public override IEnumerable<Control> GetConnectedControls()
        {
            if( Input == null )
            {
                return Enumerable.Empty<Control>();
            }
            return new[] { Input };
        }

        public override bool TryConnect( Control other )
        {
            if( other is not ControlleeInput input )
                return false;

            ControlleeInput.Connect( input, this );
            return true;
        }

        public override bool TryDisconnect( Control other )
        {
            if( other is not ControlleeInput input )
                return false;

            if( this.Input != input )
                return false;

            ControlleeInput.Disconnect( this );
            return true;
        }

        public override bool TryDisconnectAll()
        {
            if( this.Input == null )
                return false;

            ControlleeInput.Disconnect( this );
            return true;
        }

        [SerializationMappingProvider( typeof( ControllerOutput ) )]
        public static SerializationMapping ControllerOutputMapping()
        {
            return new MemberwiseSerializationMapping<ControllerOutput>(); // empty dummy referencable thing.
        }
    }
}