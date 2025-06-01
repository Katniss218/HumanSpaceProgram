using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Effects.Meshes
{
    internal class MeshEffectPoolItem : MonoBehaviour
    {
        internal ObjectPoolItemState State { get; private set; } = ObjectPoolItemState.Ready;

        internal Transform TargetTransform { get; set; }

        internal int version;
        internal MeshEffectHandle currentHandle; // kind of singleton (per pool item) with the handle management.

        private bool _isSkinned = false;
        internal MeshFilter meshFilter;
        internal MeshRenderer meshRenderer;
        internal SkinnedMeshRenderer skinnedMeshRenderer;
        internal float duration;
        internal bool loop;
        private float _timeWhenFinished;
        private IMeshEffectData _data;

        internal List<Transform> _bonePool;

        internal Vector3 localPos = Vector3.zero;
        internal Quaternion localRot = Quaternion.identity;
        internal Vector3 localScale = Vector3.one;

        public bool IsSkinned => _isSkinned;

        void Awake()
        {
        }

        void Update()
        {
            if( State == ObjectPoolItemState.Finished || State == ObjectPoolItemState.Ready )
                return;

            if( TargetTransform != null )
            {
                this.transform.SetPositionAndRotation( TargetTransform.position + localPos, TargetTransform.rotation * localRot );
                this.transform.localScale = localScale;
            }

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

            _timeWhenFinished = loop
                ? float.MaxValue
                : UnityEngine.Time.time + duration;

            State = ObjectPoolItemState.Playing;
        }

        private void SetState_Finished()
        {
            State = ObjectPoolItemState.Finished;
            gameObject.SetActive( false ); // Disables the gameobject to stop empty update calls and other processing.
        }

        internal void Play()
        {
            if( State != ObjectPoolItemState.Ready )
                throw new InvalidOperationException( $"Mesh effect can only be played when in the {nameof( ObjectPoolItemState.Ready )} state." );

            SetState_Playing();
        }

        internal void Stop()
        {
            if( State != ObjectPoolItemState.Playing )
                throw new InvalidOperationException( $"Mesh effect can only be stopped when in the {nameof( ObjectPoolItemState.Playing )} state." );

            SetState_Finished();
        }

        internal static MeshEffectPoolItem Create()
        {
            GameObject go = new GameObject( $"Object Pool Item - {typeof( MeshEffectPoolItem ).Name}" );

            MeshFilter mf = go.AddComponent<MeshFilter>();

            MeshEffectPoolItem item = go.AddComponent<MeshEffectPoolItem>();
            item.meshFilter = mf;
            return item;
        }

        internal void SetMeshData( IMeshEffectData data )
        {
            if( data.Bones != null )
            {
                _isSkinned = true;
                if( _bonePool == null )
                    _bonePool = new List<Transform>( data.Bones.Length );

                // New bone array, but only 
                Transform[] boneArray = skinnedMeshRenderer.bones;
                if( _bonePool.Count != data.Bones.Length )
                {
                    boneArray = new Transform[data.Bones.Length];
                }

                // Initialize the bones using their bindposes.
                for( int i = 0; i < data.Bones.Length; i++ )
                {
                    BoneData boneData = data.Bones[i];

                    Transform bone;
                    if( i > _bonePool.Count )
                    {
                        bone = new GameObject( "BONE" ).transform;
                        bone.SetParent( this.transform, false );
                        _bonePool.Add( bone );
                    }
                    else
                    {
                        bone = _bonePool[i];
                    }

                    bone.localPosition = boneData.BindPose.Position;
                    bone.localRotation = boneData.BindPose.Rotation;
                    bone.localScale = boneData.BindPose.Scale;
                    boneArray[i] = bone;
                }

                if( meshRenderer != null )
                    UnityEngine.Object.Destroy( meshRenderer ); // can only have one or the other, unfortunately.
                skinnedMeshRenderer = this.gameObject.GetOrAddComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.bones = boneArray;
            }
            else
            {
                meshRenderer = this.gameObject.GetOrAddComponent<MeshRenderer>();
                if( skinnedMeshRenderer != null )
                    UnityEngine.Object.Destroy( skinnedMeshRenderer ); // can only have one or the other, unfortunately.
            }

            _data = data;
            version++;
            currentHandle = new MeshEffectHandle( this );

            this.ResetState();
        }
    }
}