using HSP.Effects;
using HSP.Vanilla.Components;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Effects
{
    public class FRocketEngineFX : MonoBehaviour
    {
        public IPropulsion Engine;

        public IEffectData[] OnIgnition;
        public IEffectData[] OnRunning;
        public IEffectData[] OnShutdown;

        IEffectHandle[] _ignitionHandles;
        IEffectHandle[] _runningHandles;
        IEffectHandle[] _shutdownHandles;

        void OnEnable()
        {
            if( Engine == null )
                Engine = this.GetComponent<IPropulsion>();

            Engine.OnAfterIgnite += OnIgnitionListener;
            Engine.OnAfterShutdown += OnShutdownListener;
            Engine.OnAfterThrustChanged += OnThrustChangedListener;
        }

        void OnDisable()
        {
            if( Engine == null )
                return;

            Engine.OnAfterIgnite -= OnIgnitionListener;
            Engine.OnAfterShutdown -= OnShutdownListener;
            Engine.OnAfterThrustChanged -= OnThrustChangedListener;
        }

        bool isFiring = false;

        void OnIgnitionListener()
        {
            isFiring = true;
            if( this._ignitionHandles != null )
            {
                foreach( var handle in this._ignitionHandles )
                    handle?.TryStop();
            }
            if( this._runningHandles != null )
            {
                foreach( var handle in this._runningHandles )
                    handle?.TryStop(); 
            }
            if( this._shutdownHandles != null )
            {
                foreach( var handle in this._shutdownHandles )
                    handle?.TryStop();
            }

            Transform t = Engine.ThrustTransform;

            if( this.OnIgnition != null )
            {
                if( this.OnIgnition.Length != (this._shutdownHandles?.Length ?? 0) )
                    _ignitionHandles = new IEffectHandle[this.OnIgnition.Length];

                for( int i = 0; i < this.OnIgnition.Length; i++ )
                {
                    var onIgnition = this.OnIgnition[i];
                    if( onIgnition == null )
                        continue;

                    if( onIgnition.TargetTransform == null )
                        onIgnition.TargetTransform = t;
                    _ignitionHandles[i] = onIgnition.Play();
                }
            }
            if( this.OnRunning != null )
            {
                if( this.OnRunning.Length != (this._runningHandles?.Length ?? 0) )
                    _runningHandles = new IEffectHandle[this.OnRunning.Length];
                for( int i = 0; i < this.OnRunning.Length; i++ )
                {
                    var onRunning = this.OnRunning[i];
                    if( onRunning == null )
                        continue;

                    if( onRunning.TargetTransform == null )
                        onRunning.TargetTransform = t;
                    _runningHandles[i] = onRunning.Play();
                }
            }
        }

        void OnShutdownListener()
        {
            isFiring = false;
            if( this._ignitionHandles != null )
            {
                foreach( var handle in this._ignitionHandles )
                    handle?.TryStop();
            }
            if( this._runningHandles != null )
            {
                foreach( var handle in this._runningHandles )
                    handle?.TryStop();
            }
            if( this._shutdownHandles != null )
            {
                foreach( var handle in this._shutdownHandles )
                    handle?.TryStop();
            }

            Transform t = Engine.ThrustTransform;

            if( this.OnShutdown != null )
            {
                if( this.OnShutdown.Length != (this._shutdownHandles?.Length ?? 0) )
                    _shutdownHandles = new IEffectHandle[this.OnShutdown.Length];

                for( int i = 0; i < this.OnShutdown.Length; i++ )
                {
                    var onShutdown = this.OnShutdown[i];
                    if( onShutdown == null )
                        continue;

                    if( onShutdown.TargetTransform == null )
                        onShutdown.TargetTransform = t;
                    _shutdownHandles[i] = onShutdown.Play();
                }
            }
        }

        void OnThrustChangedListener()
        {
            if( Engine.Thrust <= 0 && isFiring )
            {
                OnShutdownListener();
            }
            else if( Engine.Thrust > 0 && !isFiring )
            {
                OnIgnitionListener();
            }
        }


        [MapsInheritingFrom( typeof( FRocketEngineFX ) )]
        public static SerializationMapping FRocketEngineExhaustFxMapping()
        {
            return new MemberwiseSerializationMapping<FRocketEngineFX>()
                .WithMember( "engine", ObjectContext.Ref, o => o.Engine )
                .WithMember( "ignition_system", o => o.OnIgnition )
                .WithMember( "loop_system", o => o.OnRunning )
                .WithMember( "shutdown_system", o => o.OnShutdown );
        }
    }
}