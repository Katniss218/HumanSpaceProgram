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
        /// Disconnects the connection leading to this control.
        /// </summary>
        public abstract void Disconnect();
    }
}
