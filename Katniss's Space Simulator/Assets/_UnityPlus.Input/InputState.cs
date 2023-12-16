using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Input
{
    /// <summary>
    /// Describes the state of the input devices at a given time.
    /// </summary>
    public struct InputState
    {
        /// <summary>
        /// The keys that are currently being held down, in the order in which they were pressed.
        /// </summary>
        public IEnumerable<KeyCode> OrderedKeyPresses;

        /// <summary>
        /// The difference between this frame's mouse position, and last frame's mouse position, in screen space.
        /// </summary>
        public Vector2 MouseDelta;

        /// <summary>
        /// The mouse position, in screen space.
        /// </summary>
        public Vector2 MousePosition;

        /// <summary>
        /// The difference between this frame's scroll value, and last frame's scroll value.
        /// </summary>
        public Vector2 ScrollDelta;
    }
}