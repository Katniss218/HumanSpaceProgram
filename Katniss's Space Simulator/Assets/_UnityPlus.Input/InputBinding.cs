using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Input
{
    public abstract class InputBinding
    {
        /// <summary>
        /// Use this method to update any potential internal state with the input data from the parameter. <br/>
        /// Return value indicates whether the current state represents a successful condition or not.
        /// </summary>
        /// <param name="currentState">The input state in this frame.</param>
        /// <returns>True if the input action associated with this binding should trigger. False otherwise.</returns>
        public abstract bool CheckUpdate( InputState currentState );
    }

    /// <summary>
    /// Binds to a specific key being pressed (after being not pressed).
    /// </summary>
    public class KeyDownBinding : InputBinding
    {
        public KeyCode Key { get; }

        bool _previousFrameWasReleased = true;

        public KeyDownBinding( KeyCode key )
        {
            this.Key = key;
        }

        public override bool CheckUpdate( InputState currentState )
        {
            // key down = previous frame not pressed, this frame pressed.
            bool previousFrameWasReleased = _previousFrameWasReleased;
            bool currentFrameIsPressed = currentState.CurrentHeldKeys.Contains( Key );
            _previousFrameWasReleased = !currentFrameIsPressed;
            return previousFrameWasReleased && currentFrameIsPressed;
        }
    }

    /// <summary>
    /// Binds to a specific key being released (after being pressed).
    /// </summary>
    public class KeyUpBinding : InputBinding
    {
        public KeyCode Key { get; }

        bool _previousFrameWasPressed = true;

        public KeyUpBinding( KeyCode key )
        {
            this.Key = key;
        }

        public override bool CheckUpdate( InputState currentState )
        {
            bool previousFrameWasPressed = _previousFrameWasPressed;
            bool currentFrameIsReleased = !currentState.CurrentHeldKeys.Contains( Key );
            _previousFrameWasPressed = !currentFrameIsReleased;
            return previousFrameWasPressed && currentFrameIsReleased;
        }
    }

    /// <summary>
    /// Binds to a specific key being currently held down, disregarding previous state.
    /// </summary>
    public class KeyHoldBinding : InputBinding
    {
        public KeyCode Key { get; }

        public KeyHoldBinding( KeyCode key )
        {
            this.Key = key;
        }

        public override bool CheckUpdate( InputState currentState )
        {
            return currentState.CurrentHeldKeys.Contains( Key );
        }
    }

    /// <summary>
    /// Binds to a specific key being released after being pressed, restricting that the mouse mustn't have moved between when the key was pressed and released.
    /// </summary>
    public class MouseClickBinding : InputBinding
    {
        public const float MaxMouseDelta = 4.0f;

        // static array can be used to store the 6 starting locations of 6 possible mouse buttons

        public KeyCode Key { get; }

        bool _previousFrameWasHeld = false;
        Vector2 _originMousePosition = Vector2.zero;

        public MouseClickBinding( KeyCode key )
        {
            this.Key = key;
        }

        public override bool CheckUpdate( InputState currentState )
        {
            if( currentState.CurrentHeldKeys.Contains( Key ) )
            {
                if( !_previousFrameWasHeld )
                {
                    _originMousePosition = currentState.MousePosition;
                    _previousFrameWasHeld = true; // now will be held.
                }
                return false;
            }
            else
            {
                if( _previousFrameWasHeld )
                {
                    Vector2 clickDelta = _originMousePosition - currentState.MousePosition;
                    _previousFrameWasHeld = false; // now won't be held.
                    return Mathf.Abs( clickDelta.x ) < MaxMouseDelta && Mathf.Abs( clickDelta.y ) < MaxMouseDelta;
                }

                // never pressed in the first place.
                return false;
            }
        }
    }

    /// <summary>
    /// Binds to a specific key being released after being pressed, restricting that the mouse must have moved between when the key was pressed and released.
    /// </summary>
    public class MouseDragBinding : InputBinding
    {
        public const float MaxMouseDelta = 4.0f;

        public KeyCode Key { get; }

        bool _previousFrameWasHeld = false;
        Vector2 _originMousePosition = Vector2.zero;

        public MouseDragBinding( KeyCode key )
        {
            this.Key = key;
        }

        public override bool CheckUpdate( InputState currentState )
        {
            if( currentState.CurrentHeldKeys.Contains( Key ) )
            {
                if( !_previousFrameWasHeld )
                {
                    _originMousePosition = currentState.MousePosition;
                    _previousFrameWasHeld = true; // now will be held.
                }
                return false;
            }
            else
            {
                if( _previousFrameWasHeld )
                {
                    Vector2 clickDelta = _originMousePosition - currentState.MousePosition;
                    _previousFrameWasHeld = false; // now won't be held.
                    return Mathf.Abs( clickDelta.x ) > MaxMouseDelta || Mathf.Abs( clickDelta.y ) > MaxMouseDelta;
                }

                // never pressed in the first place.
                return false;
            }
        }
    }



    // we could do compound bindings, just pass the update to the inner children.
    [Obsolete( "this is just for educational purposes, I don't want the users to have to mess with combining channels and stuff." )]
    internal class CompoundBinding : InputBinding
    {
        InputBinding[] _bindings { get; }

        public CompoundBinding( IEnumerable<InputBinding> bindings )
        {
            this._bindings = bindings.ToArray();
        }

        public override bool CheckUpdate( InputState currentState )
        {
            bool wasTrue = false;

            foreach( var binding in _bindings )
                if( binding.CheckUpdate( currentState ) )
                    wasTrue = true;

            return wasTrue;
        }
    }
}