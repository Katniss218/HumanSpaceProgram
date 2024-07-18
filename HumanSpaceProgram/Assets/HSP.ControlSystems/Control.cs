using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ControlSystems
{
    /// <summary>
    /// Represents an arbitrary type of "socket / endpoint" that some form of a control connection can be routed to or from.
    /// </summary>
    public abstract class Control
    {
        /// <summary>
        /// The transform that this control belongs (i.e. is attached) to.
        /// </summary>
        internal Transform transform { get; set; }

        /// <summary>
        /// Gets the controls that this control is connected to.
        /// </summary>
        public abstract IEnumerable<Control> GetConnectedControls();

        /// <summary>
        /// Tries to connect this control to a given control. <br/>
        /// </summary>
        /// <returns>True if the controls are compatible, and the connection was created successfully.</returns>
        public abstract bool TryConnect( Control other );

        /// <summary>
        /// Tries to disconnect this control from the specific control.
        /// </summary>
        /// <returns>True if the connection was removed. False if the connection didn't exist or couldn't've been removed.</returns>
        public abstract bool TryDisconnect( Control other );

        /// <summary>
        /// Tries to disconnect this control from every other control it is connected to.
        /// </summary>
        /// <returns>True if the connection was removed. False if the connection didn't exist or couldn't've been removed.</returns>
        public abstract bool TryDisconnectAll();


        [MapsInheritingFrom( typeof( Control ) )]
        public static SerializationMapping ControlMapping()
        {
            return new MemberwiseSerializationMapping<Control>()
            {
                ("transform", new Member<Control, Transform>( ObjectContext.Ref, o => o.transform )),
            };
        }
    }
}