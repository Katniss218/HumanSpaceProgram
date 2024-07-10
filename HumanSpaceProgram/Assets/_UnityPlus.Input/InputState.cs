using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Input
{
    /// <summary>
    /// Describes the state of all input devices at a given frame. <br/>
    /// Used by the <see cref="InputBinding"/>s to determine whether or not a given input is triggered.
    /// </summary>
    public class InputState
    {
        KeyCode[] _keysHeldDown;

        /// <summary>
        /// The keys being held down in this frmae.
        /// </summary>
        public IEnumerable<KeyCode> CurrentHeldKeys { get => _keysHeldDown; }

        /// <summary>
        /// The mouse position in this frmae, in screen space.
        /// </summary>
        public Vector2 MousePosition { get; }

        /// <summary>
        /// The difference between this frame's scroll value, and last frame's scroll value.
        /// </summary>
        public Vector2 ScrollDelta { get; }

        public InputState( IEnumerable<KeyCode> keys, Vector2 mousePosition, Vector2 scrollDelta )
        {
            this._keysHeldDown = keys.ToArray();
            this.MousePosition = mousePosition;
            this.ScrollDelta = scrollDelta;
        }
    }
}