using UnityEngine;

namespace HSP.Effects
{
    /// <summary>
    /// Represents some playable effect along with the data to use when playing it.
    /// </summary>
    public interface IEffectData
    {
        /// <summary>
        /// The effect will follow this transform when playing.
        /// </summary>
        public Transform TargetTransform { get; set; }

        /// <summary>
        /// Plays the effect using the data in this effect data.
        /// </summary>
        /// <returns>The handle to the played effect.</returns>
        public IEffectHandle Play();
    }

    public interface IEffectData<THandle> : IEffectData where THandle : IEffectHandle
    {
        /// <summary>
        /// Invoked once, right before the effect begins playing.
        /// </summary>
        void OnInit( THandle handle );

        /// <summary>
        /// Invoked every Unity Update().
        /// </summary>
        void OnUpdate( THandle handle );

        /// <summary>
        /// Invoked when the effect is released back to the pool.
        /// </summary>
        void OnDispose( THandle handle );
    }
}