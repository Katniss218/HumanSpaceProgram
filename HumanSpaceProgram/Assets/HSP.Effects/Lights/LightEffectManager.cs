using UnityEngine;
using UnityPlus;

namespace HSP.Effects.Lights
{
    public class LightEffectManager : SingletonMonoBehaviour<LightEffectManager>
    {
        static ObjectPool<LightEffectPoolItem, ILightEffectData> _pool = new(
            ( i, data ) =>
            {
                i.SetLightData( data );
            },
            i => i.State == ObjectPoolItemState.Finished );


        public static LightEffectHandle Prepare( ILightEffectData data )
        {
            var poolItem = _pool.Get( data );

            return poolItem.currentHandle;
        }

        public static LightEffectHandle Play( ILightEffectData data )
        {
            var poolItem = _pool.Get( data );

            poolItem.Play();
            return poolItem.currentHandle;
        }
    }
}