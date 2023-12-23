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
    public class InputState
    {
        KeyCode[] _keysHeldDown;

        public IEnumerable<KeyCode> CurrentHeldKeys { get => _keysHeldDown; }

        /// <summary>
        /// The mouse position, in screen space.
        /// </summary>
        public Vector2 MousePosition { get; }

        /// <summary>
        /// The difference between this frame's scroll value, and last frame's scroll value.
        /// </summary>
        public Vector2 ScrollDelta { get; }

        // Max mouse movement to still be considered a click, in pixels.
        

        public bool IsLeftMouseClick()
        {
            Vector2 clickDelta = MousePosition - _clickMouse0;
            return Mathf.Abs( clickDelta.x ) < MaxClickMouseDelta && Mathf.Abs( clickDelta.y ) < MaxClickMouseDelta && _keysPressed.Contains( KeyCode.Mouse0 );
        }

        public bool IsRightMouseClick()
        {
            Vector2 clickDelta = MousePosition - _clickMouse1;
            return Mathf.Abs( clickDelta.x ) < MaxClickMouseDelta && Mathf.Abs( clickDelta.y ) < MaxClickMouseDelta && _keysPressed.Contains( KeyCode.Mouse1 );
        }

        public bool IsMiddleMouseClick()
        {
            Vector2 clickDelta = MousePosition - _clickMouse2;
            return Mathf.Abs( clickDelta.x ) < MaxClickMouseDelta && Mathf.Abs( clickDelta.y ) < MaxClickMouseDelta && _keysPressed.Contains( KeyCode.Mouse2 );
        }

        public Rect? GetLeftMouseRect()
        {

        }
        
        public Rect? GetRightMouseRect()
        {

        }
        
        public Rect? GetMiddleMouseRect()
        {

        }

        // todo - invert this. instead of checking using a delegate, this sort of structure should be passed *into* the input system to perform optimizations e.g. only check keys that are being listened to.

    }
}