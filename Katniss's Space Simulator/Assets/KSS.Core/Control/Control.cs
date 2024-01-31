using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Control
{
    /// <summary>
    /// Represents an arbitrary type of "socket" that some form of a control connection can be routed to or from.
    /// </summary>
    public abstract class Control
    {
        /// <summary>
        /// The transform that this "plug/socket" belongs (i.e. is attached) to.
        /// </summary>
        internal Transform transform { get; set; }

        /// <summary>
        /// Gets the control(s) that this control is connected to.
        /// </summary>
        public abstract IEnumerable<Control> GetConnections();

        /// <summary>
        /// Tries to connect this control to a given control.
        /// </summary>
        /// <returns>True if the connection was created successfully.</returns>
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
    }
}
