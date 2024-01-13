using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Extensions
{
    public static class Transform_Ex
    {
        public static IEnumerable<Transform> Children( this Transform transform )
        {
            foreach( object child in transform )
            {
                yield return (Transform)child;
            }
        }

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

        public static bool IsParentOf( this Transform parent, Transform other )
        {
            return other.parent == parent;
        }

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