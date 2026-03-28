using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A specialized stack for SerializationCursors that handles the propagation of changes 
    /// for boxed value types (structs) upon popping.
    /// </summary>
    public class ExecutionStack
    {
        private SerializationCursor[] _stack = new SerializationCursor[64];
        private int _count = 0;

        public int Count => _count;

        public SerializationCursor this[int index]
        {
            get
            {
                if( index < 0 || index >= _count )
                    throw new IndexOutOfRangeException();
                return _stack[index];
            }
        }

        public void Push( SerializationCursor cursor, int maxDepth = int.MaxValue )
        {
            if( _count >= maxDepth )
                throw new InvalidOperationException( $"Maximum recursion depth of {maxDepth} exceeded." );

            if( _count == _stack.Length )
            {
                Array.Resize( ref _stack, _stack.Length * 2 );
            }

            _stack[_count++] = cursor;
        }

        public SerializationCursor Peek()
        {
            if( _count == 0 )
                throw new InvalidOperationException( "Stack is empty" );
            return _stack[_count - 1];
        }

        public void UpdateTop( ref SerializationCursor cursor )
        {
            if( _count == 0 )
                throw new InvalidOperationException( "Stack is empty" );

            _stack[_count - 1] = cursor;
        }

        public void UpdateAt( int index, ref SerializationCursor cursor )
        {
            if( index < 0 || index >= _count )
                throw new IndexOutOfRangeException();

            _stack[index] = cursor;
        }

        /// <summary>
        /// Pops the top cursor and performs write-back if requested.
        /// </summary>
        public SerializationCursor PopAndWriteback()
        {
            if( _count == 0 )
                throw new InvalidOperationException( "Stack is empty" );

            int topIndex = --_count;
            SerializationCursor cursor = _stack[topIndex];
            _stack[topIndex] = default; // Clear reference

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
            if( _count == 0 )
                throw new InvalidOperationException( "Stack is empty" );

            int topIndex = --_count;
            SerializationCursor cursor = _stack[topIndex];
            _stack[topIndex] = default; // Clear reference

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
            for( int i = _count - 1; i >= 0; i-- )
            {
                // We have to copy the struct, modify it, and assign it back to the array.
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
            Array.Clear( _stack, 0, _count );
            _count = 0;
        }

        public bool ContainsTarget( object target )
        {
            if( target == null ) return false;
            var stack = _stack;
            int count = _count;
            for( int i = 0; i < count; i++ )
            {
                if( ReferenceEquals( stack[i].TargetObj.Target, target ) )
                    return true;
            }
            return false;
        }
    }
}