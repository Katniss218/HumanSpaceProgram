using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A specialized stack for SerializationCursors that handles the propagation of changes 
    /// for boxed value types (structs) upon popping.
    /// </summary>
    public class ExecutionStack
    {
        // We use List instead of Stack to allow index-based access for updating parent cursors.
        private readonly List<SerializationCursor> _stack = new List<SerializationCursor>( 64 );

        public int Count => _stack.Count;

        /// <summary>
        /// Provides read-only access to the stack frames for diagnostics/path building.
        /// Index 0 is the Root. Index Count-1 is the Current (Top).
        /// </summary>
        public IReadOnlyList<SerializationCursor> Frames => _stack;

        public void Push( SerializationCursor cursor )
        {
            _stack.Add( cursor );
        }

        public SerializationCursor Peek()
        {
            if( _stack.Count == 0 )
                throw new InvalidOperationException( "Stack is empty" );
            return _stack[_stack.Count - 1];
        }

        /// <summary>
        /// Pops the top cursor and performs write-back if requested.
        /// </summary>
        public SerializationCursor PopAndWriteback()
        {
            if( _stack.Count == 0 )
                throw new InvalidOperationException( "Stack is empty" );

            int topIndex = _stack.Count - 1;
            SerializationCursor cursor = _stack[topIndex];
            _stack.RemoveAt( topIndex );

            if( cursor.WriteBackOnPop && cursor.TargetObj.Parent != null )
            {
                ApplyWriteBack( cursor );
            }

            return cursor;
        }

        /// <summary>
        /// Removes the top cursor WITHOUT performing write-back.
        /// Used when moving cursors to deferred queues or rearranging the stack.
        /// </summary>
        public SerializationCursor Pop()
        {
            if( _stack.Count == 0 ) 
                throw new InvalidOperationException( "Stack is empty" );

            int topIndex = _stack.Count - 1;
            SerializationCursor cursor = _stack[topIndex];
            _stack.RemoveAt( topIndex );

            return cursor;
        }

        private void ApplyWriteBack( SerializationCursor child )
        {
            object currentTarget = child.TargetObj.Target;
            object parentTarget = child.TargetObj.Parent;
            IMemberInfo member = child.TargetObj.Member;

            // 1. Write the modified child (currentTarget) into the parent (parentTarget).
            //    If parentTarget is a boxed struct, 'ref parentTarget' reassigns the local variable 
            //    to point to the NEW boxed copy.
            member.SetValue( ref parentTarget, currentTarget );

            // 2. Check if the parent object was replaced (Boxing occurred).
            //    This happens when Parent is a struct and SetValue updated the box.
            if( !ReferenceEquals( parentTarget, child.TargetObj.Parent ) )
            {
                // The parent was a value type and has been re-boxed. 
                // We must update the stack to point to this new box, so that when the parent eventually pops,
                // it writes the *new* box back to *its* parent.
                UpdateParentTargetInStack( child.TargetObj.Parent, parentTarget );
            }
        }

        private void UpdateParentTargetInStack( object oldTarget, object newTarget )
        {
            // Scan down the stack to find the cursor that owns 'oldTarget'.
            // It will be near the top.
            for( int i = _stack.Count - 1; i >= 0; i-- )
            {
                // We have to copy the struct, modify it, and assign it back to the list.
                var cursor = _stack[i];

                if( ReferenceEquals( cursor.TargetObj.Target, oldTarget ) )
                {
                    cursor.TargetObj = cursor.TargetObj.WithTarget( newTarget );
                    _stack[i] = cursor;

                    // We stop here. We do not need to propagate further up immediately.
                    // When *this* cursor pops, it will trigger the next level of write-back 
                    // because its 'WriteBackOnPop' flag should already be true (if it was deserializing or a struct).
                    return;
                }
            }
        }

        public void Clear()
        {
            _stack.Clear();
        }
    }
}