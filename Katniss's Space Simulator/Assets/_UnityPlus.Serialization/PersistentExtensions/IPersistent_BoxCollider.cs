using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityPlus.Serialization
{
	public static class IPersistent_BoxCollider
	{
		public static SerializedData GetData( this BoxCollider bc, IReverseReferenceMap s )
		{
			return new SerializedObject()
			{
				{ "size", bc.size.GetData() },
				{ "center", bc.center.GetData() },
				{ "is_trigger", bc.isTrigger }
			};
		}

		public static void SetData( this BoxCollider bc, SerializedData data, IForwardReferenceMap l )
		{
			if( data.TryGetValue( "size", out var size ) )
				bc.size = size.ToVector3();

			if( data.TryGetValue( "center", out var center ) )
				bc.center = center.ToVector3();

			if( data.TryGetValue( "is_trigger", out var isTrigger ) )
				bc.isTrigger = (bool)isTrigger;
		}
	}
}