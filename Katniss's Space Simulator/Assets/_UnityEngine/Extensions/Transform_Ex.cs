using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Extensions
{
    public static class Transform_Ex
    {
        public static IEnumerable<Transform> Children( Transform transform )
        {
            foreach( object child in transform )
            {
                yield return (Transform)child;
            }
        }

        public static IEnumerable<Transform> Descendants( Transform transform )
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
    }
}