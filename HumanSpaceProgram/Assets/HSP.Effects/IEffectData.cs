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

    public interface IEffectData<T> : IEffectData where T : IEffectHandle
    {
        void OnInit( T handle );
        void OnUpdate( T handle );
    }
}