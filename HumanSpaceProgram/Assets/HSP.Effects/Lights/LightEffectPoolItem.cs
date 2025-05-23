using System;
using UnityEngine;

namespace HSP.Effects.Lights
{
    [RequireComponent( typeof( Light ) )]
    internal class LightEffectPoolItem : MonoBehaviour
    {
        internal ObjectPoolItemState State { get; private set; } = ObjectPoolItemState.Ready;

        internal Transform TargetTransform { get; set; }

        internal int version;
        internal LightEffectHandle currentHandle; // kind of singleton (per pool item) with the handle management.

        internal Light light;
        private float _duration;
        private float _timeWhenFinished;
        private ILightEffectData _data;

        void Awake()
        {
            light = GetComponent<Light>();
        }

        void Update()
        {
            if( State == ObjectPoolItemState.Finished || State == ObjectPoolItemState.Ready )
                return;

            if( TargetTransform != null )
                this.transform.position = TargetTransform.position;

            _data.OnUpdate( this.currentHandle );

            if( UnityEngine.Time.time >= _timeWhenFinished ) // When finished, just stop.
            {
                SetState_Finished();
            }
        }

        private void ResetState()
        {
            State = ObjectPoolItemState.Ready;
        }

        private void SetState_Playing()
        {
            this.gameObject.SetActive( true );

            _data.OnInit( this.currentHandle );

            _timeWhenFinished = UnityEngine.Time.time + _duration;

            State = ObjectPoolItemState.Playing;
        }

        private void SetState_Finished()
        {
            State = ObjectPoolItemState.Finished;
            gameObject.SetActive( false ); // Disables the gameobject to stop empty update calls and other processing.
        }

        internal void SetLightData( ILightEffectData data )
        {
            _data = data;
            version++;
            currentHandle = new LightEffectHandle( this );

            this.ResetState();
        }

        internal void Play()
        {
            if( State != ObjectPoolItemState.Ready )
                throw new InvalidOperationException( $"Audio can only be played when in the {nameof( ObjectPoolItemState.Ready )} state." );

            SetState_Playing();
        }

        internal void Stop()
        {
            if( State != ObjectPoolItemState.Playing )
                throw new InvalidOperationException( $"Audio can only be stopped when in the {nameof( ObjectPoolItemState.Playing )} state." );

            SetState_Finished();
        }
    }
}