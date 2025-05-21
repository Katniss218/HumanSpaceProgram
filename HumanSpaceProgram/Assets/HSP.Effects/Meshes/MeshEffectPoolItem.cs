using System;
using UnityEngine;

namespace HSP.Effects.Meshes
{
    [RequireComponent( typeof( Mesh ) )]
    internal class MeshEffectPoolItem : MonoBehaviour
    {
        internal ObjectPoolItemState State { get; private set; } = ObjectPoolItemState.Ready;

        internal Transform TargetTransform { get; set; }

        internal int version;
        internal MeshEffectHandle currentHandle; // kind of singleton (per pool item) with the handle management.

        private Mesh _mesh;
        private float _duration;
        private float _timeWhenFinished;
        private IMeshEffectData _data;

        void Awake()
        {
            _mesh = GetComponent<Mesh>();
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

        internal void SetMeshData( IMeshEffectData data )
        {
            _data = data;
            version++;
            currentHandle = new MeshEffectHandle( this );

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