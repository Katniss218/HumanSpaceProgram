using UnityEngine;
using UnityPlus;

namespace HSP.Effects.Particles
{
    public class ParticleEffectManager : SingletonMonoBehaviour<ParticleEffectManager>
    {
        static ObjectPool<ParticleEffectPoolItem, IParticleEffectData> _pool = new(
            ( i, data ) =>
            {
                i.SetParticleData( data );
            },
            i => i.State == ObjectPoolItemState.Finished );

        public static ParticleEffectHandle Prepare( IParticleEffectData data )
        {
            var poolItem = _pool.Get( data );

            return poolItem.currentHandle;
        }
        public static ParticleEffectHandle Play( IParticleEffectData data )
        {
            var poolItem = _pool.Get( data );

            poolItem.Play();
            return poolItem.currentHandle;
        }
    }
}