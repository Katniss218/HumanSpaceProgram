using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus;

namespace HSP.Effects.Meshes
{
    /// <summary>
    /// 
    /// Mesh Effects are a type of visual effect that uses a singular mesh (potentially rigged and skinned) to represent the effect. <br/>
    /// They allow the user to manipulate the mesh by changing its material properties, and bones.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class MeshEffectManager : SingletonMonoBehaviour<MeshEffectManager>
    {
        static ObjectPool<MeshEffectPoolItem, IMeshEffectData> _pool = new(
            ( i, data ) =>
            {
                i.SetMeshData( data );
            },
            i => i.State == ObjectPoolItemState.Finished );

        public static MeshEffectHandle Prepare( IMeshEffectData data )
        {
            var poolItem = _pool.Get( data );

            return poolItem.currentHandle;
        }

        public static MeshEffectHandle Play( IMeshEffectData data )
        {
            var poolItem = _pool.Get( data );

            poolItem.Play();
            return poolItem.currentHandle;
        }
    }
}