using System;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.Serialization;

namespace HSP.Effects.Meshes
{
    public class MeshEffectDefinition : IMeshEffectData
    {
        /// <summary>
        /// The transform that the mesh effect will follow.
        /// </summary>
        public Transform TargetTransform { get; set; }

        //
        // Driven properties:

        public ConstantEffectValue<Vector3> Position { get; set; } = null;
        public ConstantEffectValue<Quaternion> Rotation { get; set; } = null;
        public ConstantEffectValue<Vector3> Scale { get; set; } = null;

        public Mesh Mesh { get; set; }
        public Material Material { get; set; }

        public IMeshEffectSimulationFrame SimulationFrame { get; set; }

        public BoneData[] Bones { get; set; } = null;
        public float Duration { get; set; }
        public bool Loop { get; set; }

        public IMeshRigger MeshRigger { get; set; } = null;


        public void OnInit( MeshEffectHandle handle )
        {
            handle.EnsureValid();

            handle.TargetTransform = this.TargetTransform;
            handle.Mesh = this.Mesh;
            handle.Material = this.Material;
            handle.Duration = this.Duration;
            handle.Loop = this.Loop;

            if( this.Position != null )
            {
                this.Position.InitDrivers( handle );
                handle.Position = this.Position.Get();
            }
            if( this.Rotation != null )
            {
                this.Rotation.InitDrivers( handle );
                handle.Rotation = this.Rotation.Get();
            }
            if( this.Scale != null )
            {
                this.Scale.InitDrivers( handle );
                handle.Scale = this.Scale.Get();
            }

            if( Bones != null ) // handle.IsSkinned should be true starting at initialization, unless something messed with the backing objects.
            {
                for( int i = 0; i < Bones.Length; i++ )
                {
                    BoneData boneData = Bones[i];
                    if( boneData == null )
                        throw new InvalidOperationException( $"Bone data at index {i} is null." );

                    Transform boneTransform = handle.Bones[i].transform;

                    if( boneData.Position != null )
                    {
                        boneData.Position.InitDrivers( handle );
                        boneTransform.localPosition = boneData.Position.Get();
                    }
                    if( boneData.Rotation != null )
                    {
                        boneData.Rotation.InitDrivers( handle );
                        boneTransform.localRotation = boneData.Rotation.Get();
                    }
                    if( boneData.Scale != null )
                    {
                        boneData.Scale.InitDrivers( handle );
                        boneTransform.localScale = boneData.Scale.Get();
                    }
                }

                if( MeshRigger != null )
                {
                    MeshRigger.RigInPlace( handle.Mesh, handle.Bones );
                }
            }

            if( SimulationFrame != null )
                SimulationFrame.OnInit( handle );
        }

        public void OnUpdate( MeshEffectHandle handle )
        {
            handle.EnsureValid();

            if( this.Position != null && this.Position.drivers != null )
                handle.Position = this.Position.Get();
            if( this.Rotation != null && this.Rotation.drivers != null )
                handle.Rotation = this.Rotation.Get();
            if( this.Scale != null && this.Scale.drivers != null )
                handle.Scale = this.Scale.Get();

            if( Bones != null ) // handle.IsSkinned should be true starting at initialization, unless something messed with the backing objects.
            {
                for( int i = 0; i < Bones.Length; i++ )
                {
                    BoneData boneData = Bones[i];
                    if( boneData == null )
                        throw new InvalidOperationException( $"Bone data at index {i} is null." );

                    Transform boneTransform = handle.Bones[i].transform;

                    if( boneData.Position != null && boneData.Position.drivers != null )
                        boneTransform.localPosition = boneData.Position.Get();
                    if( boneData.Rotation != null && boneData.Rotation.drivers != null )
                        boneTransform.localRotation = boneData.Rotation.Get();
                    if( boneData.Scale != null && boneData.Scale.drivers != null )
                        boneTransform.localScale = boneData.Scale.Get();
                }
            }

            if( SimulationFrame != null )
                SimulationFrame.OnUpdate( handle );
        }

        public IEffectHandle Play()
        {
            return MeshEffectManager.Play( this );
        }


        [MapsInheritingFrom( typeof( MeshEffectDefinition ) )]
        public static SerializationMapping MeshEffectDefinitionMapping()
        {
            return new MemberwiseSerializationMapping<MeshEffectDefinition>()
                .WithMember( "mesh", ObjectContext.Asset, o => o.Mesh )
                .WithMember( "material", ObjectContext.Asset, o => o.Material )
                .WithMember( "simulation_frame", o => o.SimulationFrame )
                .WithMember( "duration", o => o.Duration )
                .WithMember( "loop", o => o.Loop )
                .WithMember( "bones", o => o.Bones )
                .WithMember( "mesh_rigger", o => o.MeshRigger );
        }
    }
}