using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class Transform_Ex
    {
        /// <summary>
        /// Returns every transform that is an immediate child of this transform.
        /// </summary>
        public static IEnumerable<Transform> Children( this Transform transform )
        {
            foreach( object child in transform )
            {
                yield return (Transform)child;
            }
        }

        /// <summary>
        /// Returns every transform that is nested within this transform.
        /// </summary>
        public static IEnumerable<Transform> Descendants( this Transform transform )
        {
            Stack<Transform> stack = new Stack<Transform>();
            stack.Push( transform );

            while( stack.Count > 0 )
            {
                Transform current = stack.Pop();

                yield return current;

                foreach( object child in current )
                {
                    stack.Push( (Transform)child );
                }
            }
        }

        /// <summary>
        /// Returns every transform that is nested within this transform, up to the specified depth.
        /// </summary>
        /// <param name="maxDepth">The maximum depth of recursion.</param>
        public static IEnumerable<Transform> Descendants( this Transform transform, int maxDepth )
        {
            Stack<(Transform, int)> stack = new Stack<(Transform, int)>();
            stack.Push( (transform, 0) );

            while( stack.Count > 0 )
            {
                (Transform current, int depth) = stack.Pop();

                yield return current;

                if( depth < maxDepth )
                {
                    foreach( object child in current )
                    {
                        stack.Push( ((Transform)child, depth + 1) );
                    }
                }
            }
        }

        public static Matrix4x4 GetLocalToParentMatrix( this Transform transform )
        {
            return Matrix4x4.TRS( transform.localPosition, transform.localRotation, transform.localScale );
        }

        /// <summary>
        /// Builds a transform matrix that maps local bone space into the space of an ancestor.
        /// </summary>
        /// <remarks>
        /// Uses the local position, rotation, and scale exclusively, to avoid precision issues if the transform is var away from scene origin. <br/>
        /// If this is not necessary, use Transform.TransformPoint() and Transform.InverseTransformPoint() instead.
        /// </remarks>
        public static Matrix4x4 GetLocalToAncestorMatrix( this Transform transform, Transform ancestor )
        {
            if( transform == null )
                throw new ArgumentNullException( nameof( transform ) );
            if( ancestor == null )
                throw new ArgumentNullException( nameof( ancestor ) );
            if( transform == ancestor )
                throw new ArgumentException( nameof( transform ), "Transform and ancestor are the same." );

            Matrix4x4 matrix = Matrix4x4.identity;
            Transform current = transform;

            while( current != null && current != ancestor )
            {
                Matrix4x4 localMatrix = current.GetLocalToParentMatrix();
                matrix = localMatrix * matrix;
                current = current.parent;
            }

            if( current != ancestor )
            {
                throw new ArgumentException( $"{ancestor.name} is not an ancestor of {transform.name}.", nameof( transform ) );
            }

            return matrix;
        }

        public static bool IsParentOf( this Transform parent, Transform other )
        {
            return other.parent == parent;
        }

        /// <summary>
        /// Checks if this transform lies on the parent chain of the <paramref name="other"/> transform.
        /// </summary>
        public static bool IsAncestorOf( this Transform ancestor, Transform other )
        {
            while( other != null )
            {
                if( other.parent == ancestor )
                    return true;

                other = other.parent;
            }

            return false;
        }
    }
}