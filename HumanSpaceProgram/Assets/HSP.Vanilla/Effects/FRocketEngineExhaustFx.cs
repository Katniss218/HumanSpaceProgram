using HSP.Audio;
using HSP.Vanilla.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Effects
{
    public enum ParticleEffectFrame
    {
        PositionedNearestCelestialBody,
        OrientedNearestCelestialBody,

        PositionedTransform,
        OrientedTransform,
        LocalTransform,
    }

    public class ParticleEffectManager : SingletonMonoBehaviour<ParticleEffectManager>
    {
        private struct ParticleEffectPoolData
        {
        }

        static ObjectPool<AudioSourcePoolItem, ParticleEffectPoolData> _pool = new(
            ( i, data ) =>
            {
                i.SetParticleData( data );
            },
            i => i.State == AudioHandleState.Finished );

        public static IParticleEffectHandle Poof( Transform transform, Material material, ParticleEffectDataSimple data )
        {

        }
    }

    public interface IParticleEffectHandle
    {

    }

    public class ParticleEffectPoolItem : MonoBehaviour, IParticleEffectHandle
    {
        private struct CachedEntry
        {
            public Action Setter;
        }

        // properties need *some kind of path* to identify the property in question?
        private ParticleEffectDefinition _definition;

        private ParticleSystem _ps;

        private CachedEntry[] _cachedEntriesWithoutDrivers; // set only on init
        private CachedEntry[] _cachedEntriesWithDrivers; // set in update, or whenever the value changed.

        void Update()
        {
            // build cached entries:
            // - pass in the particle system to bake the reference to its instance into the setter
            // - bake the getting of the value into the lambda as well.
            // - bake the comparison with new/old too.

            foreach( var cachedEntry in _cachedEntriesWithDrivers )
            {
                cachedEntry.Setter.Invoke();
            }
            //_ps.main.startSize.constantMin = _definition.size.GetMin();
            //_ps.main.startSize.constantMax = _definition.size.GetMax();
        }
    }

    /// <summary>
    /// NOTE TO IMPLEMENTERS: This should be used on top of <see cref="IValueGetter{T}"/>. Using this interface standalone makes no sense. <br/>
    /// But due to limitations of the language and type casting, I can't derive this non-generic interface from it.
    /// </summary>
    public interface IParticleEffectInitValueGetter
    {
        void OnInit( IParticleEffectHandle handle );
    }

    public class ParticleEffectDefinition
    {
        // particles need to specify a frame
        // following an object's position, rotation, in planet space, etc.

        // list all properties...
        public MinMaxEffectValue<float> size = new();
    }

    public class FRocketEngineExhaustFx : MonoBehaviour
    {
        public IPropulsion Engine;

        public ParticleEffectDefinition IgnitionSystem;
        public ParticleEffectDefinition LoopSystem;
        public ParticleEffectDefinition ShutdownSystem;

        void OnEnable()
        {
            if( Engine == null )
                Engine = this.GetComponent<IPropulsion>();

            Engine.OnAfterIgnite += OnIgnite;
            Engine.OnAfterShutdown += OnShutdown;
        }

        void OnDisable()
        {
            if( Engine == null )
                return;

            Engine.OnAfterIgnite -= OnIgnite;
            Engine.OnAfterShutdown -= OnShutdown;
        }

        void OnIgnite()
        {
            // TODO - play exhaust fx
        }

        void OnShutdown()
        {
            // TODO - stop exhaust fx
        }
    }
}