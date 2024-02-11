using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Extensions
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