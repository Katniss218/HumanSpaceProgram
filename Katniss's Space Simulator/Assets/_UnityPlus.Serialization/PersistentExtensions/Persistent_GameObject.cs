using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_GameObject
	{
		/// <summary>
		/// Writes the 'data' part of a gameobject (only gameobject, not components).
		/// </summary>
		/// <param name="objects">The serialized array to add the serialized data object to.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this GameObject gameObject, IReverseReferenceMap s )
		{
			return new SerializedObject()
			{
				{ "name", gameObject.name },
				{ "layer", gameObject.layer },
				{ "is_active", gameObject.activeSelf },
				{ "is_static", gameObject.isStatic },
				{ "tag", gameObject.tag }
			};
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetData( this GameObject gameObject, SerializedData data, IForwardReferenceMap l )
		{
			if( data.TryGetValue( "name", out var name ) )
				gameObject.name = (string)name;

			if( data.TryGetValue( "layer", out var layer ) )
				gameObject.layer = (int)layer;

			if( data.TryGetValue( "is_active", out var isActive ) )
				gameObject.SetActive( (bool)isActive );

			if( data.TryGetValue( "is_static", out var isStatic ) )
				gameObject.isStatic = (bool)isStatic;

			if( data.TryGetValue( "tag", out var tag ) )
				gameObject.tag = (string)tag;
		}

		// persistent objects (factory).

		public static Component ToComponent( this SerializedObject data, GameObject gameObject, IForwardReferenceMap l )
		{
			Type type = data[KeyNames.TYPE].ToType();
			Guid id = data[KeyNames.ID].ToGuid();

			Component component = gameObject.GetTransformOrAddComponent( type );
			if( component is Behaviour behaviour )
			{
				// Disable the behaviour to prevent 'start' from firing prematurely if deserializing over multiple frames.
				// It will be re-enabled by SetData.
				behaviour.enabled = false;
			}

			l.SetObj( id, component );

			component.SetObjects( data, l );

			return component;
		}

		public static GameObject ToGameObject( this SerializedObject data, IForwardReferenceMap l )
		{
			// assume that data["$type"] is equal to typeof(GameObject), or missing (since this method is called recursively).

			Guid objectGuid = data[KeyNames.ID].ToGuid();

			GameObject gameObject = new GameObject();
			l.SetObj( objectGuid, gameObject );

			if( data.TryGetValue<SerializedArray>( "children", out var children ) )
			{
				Transform transform = gameObject.transform;
				foreach( var childData in children.OfType<SerializedObject>() )
				{
					// By calling the 'children' recursively before calling 'components', all children will be created first, from bottom to top.
					// Then all components will be created, from top to bottom.
					try
					{
						GameObject childGameObject = ToGameObject( childData, l );
						childGameObject.transform.SetParent( transform, false );
					}
					catch( Exception ex )
					{
						Debug.LogError( $"Failed to deserialize a gameobject with ID: `{(string)childData?[KeyNames.ID] ?? "<null>"}`." );
						Debug.LogException( ex );
					}
				}
			}

			if( data.TryGetValue<SerializedArray>( "components", out var components ) )
			{
				foreach( var compData in components.OfType<SerializedObject>() )
				{
					try
					{
						Component component = compData.ToComponent( gameObject, l );
					}
					catch( Exception ex )
					{
						Debug.LogError( $"Failed to deserialize a component with ID: `{(string)compData?[KeyNames.ID] ?? "<null>"}`." );
						Debug.LogException( ex );
					}
				}
			}

			return gameObject;
		}
	}
}