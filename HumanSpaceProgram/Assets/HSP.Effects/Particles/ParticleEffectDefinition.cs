using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles
{
    public class ParticleEffectDefinition : IParticleEffectData
    {
        public sealed class Render
        {
            public IParticleEffectRenderMode RenderMode { get; set; }
            public Material Material { get; set; }
            /// <summary>
            /// If set, overrides the particles' vertex streams with the ones specified here.
            /// </summary>
            public ParticleSystemVertexStream[] VertexStreams { get; set; } = null;
            public ShadowCastingMode ShadowCastingMode { get; set; } = ShadowCastingMode.Off;
            public bool ReceiveShadows { get; set; } = false;
            public ConstantEffectValue<Vector3> Pivot { get; set; }

            public void OnInit( ParticleEffectHandle handle )
            {
                var renderer = handle.poolItem.renderer;

                this.RenderMode.OnInit( handle );

                handle.Material = this.Material;
                handle.poolItem.renderer.shadowCastingMode = this.ShadowCastingMode;
                handle.poolItem.renderer.receiveShadows = this.ReceiveShadows;
                if( this.VertexStreams != null )
                {
                    handle.poolItem.renderer.SetActiveVertexStreams( this.VertexStreams.ToList() );
                }
                if( this.Pivot != null )
                {
                    this.Pivot.InitDrivers( handle );
                    renderer.pivot = this.Pivot.Get();
                }
            }

            public void OnUpdate( ParticleEffectHandle handle )
            {
                var renderer = handle.poolItem.renderer;

                this.RenderMode.OnUpdate( handle );

                if( this.Pivot != null )
                    renderer.pivot = this.Pivot.Get();
            }


            [MapsInheritingFrom( typeof( Render ) )]
            public static SerializationMapping RenderMapping()
            {
                return new MemberwiseSerializationMapping<Render>()
                    .WithMember( "render_mode", o => o.RenderMode )
                    .WithMember( "material", ObjectContext.Asset, o => o.Material )
                    .WithMember( "vertex_streams", o => o.VertexStreams )
                    .WithMember( "shadow_casting_mode", o => o.VertexStreams )
                    .WithMember( "receive_shadows", o => o.VertexStreams )
                    .WithMember( "pivot", o => o.Pivot );
            }
        }
        public Render RenderValues { get; set; } = new();

        public float Duration { get; set; } = 5.0f;
        /// <summary>
        /// Whether the audio should loop or not.
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// The transform that the playing audio will follow.
        /// </summary>
        public Transform TargetTransform { get; set; }

        // if simulation space is set, it needs to spawn a new gameobject, add a airf fixer to it, and set that as the PS's sim space.
        //public Transform SimulationSpace { get; set; } = null;

        // list all properties...

        public sealed class Initial
        {
            public MinMaxEffectValue<float> Size { get; set; } = new( 1.0f, 1.0f );
            public MinMaxEffectValue<float> Speed { get; set; } = new( 5.0f, 5.0f );
            public MinMaxEffectValue<float> AngularSpeed { get; set; } = new( 5.0f, 5.0f ); // rotation over lifetime
            public MinMaxEffectValue<float> Rotation { get; set; } = new( -Mathf.PI, Mathf.PI );
            public MinMaxEffectValue<float> Lifetime { get; set; } = new( 1.0f, 1.0f );
            public MinMaxEffectValue<Color> Tint { get; set; } = null;

            /// <summary>
            /// Adds a constant linear velocity to the particles
            /// </summary>
            public MinMaxEffectValue<Vector3> AdditionalLinearVelocity { get; set; } = null;
            /// <summary>
            /// Adds a constant velocity to the particles, in the direction 'outwards' from the center of the particle system.
            /// </summary>
            public MinMaxEffectValue<float> AdditionalRadialVelocity { get; set; } = null;

            public void OnInit( ParticleEffectHandle handle )
            {
                var main = handle.poolItem.main;

                if( this.Size != null )
                {
                    this.Size.InitDrivers( handle );
                    main.startSize = this.Size.GetMinMaxCurve();
                }
                if( this.Tint != null )
                {
                    this.Tint.InitDrivers( handle );
                    main.startColor = this.Tint.GetMinMaxGradient();
                }
                if( this.Lifetime != null )
                {
                    this.Lifetime.InitDrivers( handle );
                    main.startLifetime = this.Lifetime.GetMinMaxCurve();
                }
                if( this.Rotation != null )
                {
                    this.Rotation.InitDrivers( handle );
                    main.startRotation = this.Rotation.GetMinMaxCurve();
                }
                if( this.Speed != null )
                {
                    this.Speed.InitDrivers( handle );
                    main.startSpeed = this.Speed.GetMinMaxCurve();
                }
                if( this.AngularSpeed != null )
                {
                    var rotationOverLifetime = handle.poolItem.particleSystem.rotationOverLifetime;
                    rotationOverLifetime.enabled = true;

                    this.AngularSpeed.InitDrivers( handle );
                    rotationOverLifetime.z = this.AngularSpeed.GetMinMaxCurve();
                }
                if( this.AdditionalLinearVelocity != null )
                {
                    var velocityOverLifetime = handle.poolItem.particleSystem.velocityOverLifetime;
                    velocityOverLifetime.enabled = true;

                    this.AdditionalLinearVelocity.InitDrivers( handle );
                    var (min, max) = this.AdditionalLinearVelocity.Get();
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve( min.x, max.x );
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve( min.y, max.y );
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve( min.z, max.z );
                }
                if( this.AdditionalRadialVelocity != null )
                {
                    var velocityOverLifetime = handle.poolItem.particleSystem.velocityOverLifetime;
                    velocityOverLifetime.enabled = true;

                    this.AdditionalRadialVelocity.InitDrivers( handle );
                    var (min, max) = this.AdditionalRadialVelocity.Get();
                    velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve( min, max );
                }
            }

            public void OnUpdate( ParticleEffectHandle handle )
            {
                var main = handle.poolItem.main;

                if( this.Size?.drivers != null )
                    main.startSize = this.Size.GetMinMaxCurve();
                if( this.Tint?.drivers != null )
                    main.startColor = this.Tint.GetMinMaxGradient();
                if( this.Lifetime?.drivers != null )
                    main.startLifetime = this.Lifetime.GetMinMaxCurve();
                if( this.Rotation?.drivers != null )
                    main.startRotation = this.Rotation.GetMinMaxCurve();
                if( this.Speed?.drivers != null )
                    main.startSpeed = this.Speed.GetMinMaxCurve();
                if( this.AngularSpeed?.drivers != null )
                {
                    var rotationOverLifetime = handle.poolItem.particleSystem.rotationOverLifetime;

                    rotationOverLifetime.z = this.AngularSpeed.GetMinMaxCurve();
                }
                if( this.AdditionalLinearVelocity?.drivers != null )
                {
                    var velocityOverLifetime = handle.poolItem.particleSystem.velocityOverLifetime;

                    var (min, max) = this.AdditionalLinearVelocity.Get();
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve( min.x, max.x );
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve( min.y, max.y );
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve( min.z, max.z );
                }
                if( this.AdditionalRadialVelocity?.drivers != null )
                {
                    var velocityOverLifetime = handle.poolItem.particleSystem.velocityOverLifetime;

                    var (min, max) = this.AdditionalRadialVelocity.Get();
                    velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve( min, max );
                }
            }


            [MapsInheritingFrom( typeof( Initial ) )]
            public static SerializationMapping InitialMapping()
            {
                return new MemberwiseSerializationMapping<Initial>()
                    .WithMember( "size", o => o.Size )
                    .WithMember( "speed", o => o.Speed )
                    .WithMember( "angular_speed", o => o.AngularSpeed )
                    .WithMember( "rotation", o => o.Rotation )
                    .WithMember( "lifetime", o => o.Lifetime )
                    .WithMember( "tint", o => o.Tint )
                    .WithMember( "additional_linear_velocity", o => o.AdditionalLinearVelocity )
                    .WithMember( "additional_radial_velocity", o => o.AdditionalRadialVelocity );
            }
        }
        public Initial InitialValues { get; set; } = new();

        public sealed class Emission
        {
            public ConstantEffectValue<Vector3> Position { get; set; } = null;
            public ConstantEffectValue<Quaternion> Rotation { get; set; } = null;

            public IParticleEffectEmissionShape SpawnShape { get; set; }

            public MinMaxEffectValue<float> SpawnRate { get; set; } = new( 10.0f );
            public ConstantEffectValue<int> MaxParticles { get; set; } = new( 1000 );

            public ConstantEffectValue<float> RandomPosition { get; set; } = null;
            public ConstantEffectValue<float> RandomDirection { get; set; } = null;

            public void OnInit( ParticleEffectHandle handle )
            {
                var main = handle.poolItem.main;
                var emission = handle.poolItem.particleSystem.emission;
                var shape = handle.poolItem.particleSystem.shape;

                this.SpawnShape.OnInit( handle );

                if( this.Position != null )
                {
                    this.Position.InitDrivers( handle );
                    shape.position = this.Position.Get();
                }
                if( this.Rotation != null )
                {
                    this.Rotation.InitDrivers( handle );
                    shape.rotation = this.Rotation.Get().eulerAngles;
                }
                if( this.MaxParticles != null )
                {
                    this.MaxParticles.InitDrivers( handle );
                    main.maxParticles = this.MaxParticles.Get();
                }
                if( this.SpawnRate != null )
                {
                    this.SpawnRate.InitDrivers( handle );
                    emission.rateOverTime = this.SpawnRate.GetMinMaxCurve();
                }
                if( this.SpawnRate != null )
                {
                    this.SpawnRate.InitDrivers( handle );
                    emission.rateOverTime = this.SpawnRate.GetMinMaxCurve();
                }
                if( this.RandomPosition != null )
                {
                    this.RandomPosition.InitDrivers( handle );
                    shape.randomPositionAmount = this.RandomPosition.Get();
                }
                if( this.RandomDirection != null )
                {
                    this.RandomDirection.InitDrivers( handle );
                    shape.randomDirectionAmount = this.RandomDirection.Get();
                }
            }
            public void OnUpdate( ParticleEffectHandle handle )
            {
                var main = handle.poolItem.main;
                var emission = handle.poolItem.particleSystem.emission;
                var shape = handle.poolItem.particleSystem.shape;

                this.SpawnShape.OnUpdate( handle );

                if( this.Position?.drivers != null )
                    shape.position = this.Position.Get();
                if( this.Rotation?.drivers != null )
                    shape.rotation = this.Rotation.Get().eulerAngles;
                if( this.MaxParticles?.drivers != null )
                    main.maxParticles = this.MaxParticles.Get();
                if( this.SpawnRate?.drivers != null )
                    emission.rateOverTime = this.SpawnRate.GetMinMaxCurve();
                if( this.SpawnRate?.drivers != null )
                    emission.rateOverTime = this.SpawnRate.GetMinMaxCurve();
                if( this.RandomPosition?.drivers != null )
                    shape.randomPositionAmount = this.RandomPosition.Get();
                if( this.RandomDirection?.drivers != null )
                    shape.randomDirectionAmount = this.RandomDirection.Get();
            }


            [MapsInheritingFrom( typeof( Emission ) )]
            public static SerializationMapping EmissionMapping()
            {
                return new MemberwiseSerializationMapping<Emission>()
                    .WithMember( "position", o => o.Position )
                    .WithMember( "rotation", o => o.Rotation )
                    .WithMember( "spawn_shape", o => o.SpawnShape )
                    .WithMember( "spawn_rate", o => o.SpawnRate )
                    .WithMember( "max_particles", o => o.MaxParticles )
                    .WithMember( "random_position", o => o.RandomPosition )
                    .WithMember( "random_direction", o => o.RandomDirection );
            }
        }
        public Emission EmissionValues { get; set; } = new();

        public sealed class Lifetime
        {
            public MinMaxEffectValue<Vector3> Force { get; set; } = null; // always local space, because scene space is fucked
            public MinMaxEffectValue<Color> Tint { get; set; } = null;
            public MinMaxEffectValue<AnimationCurve> Size { get; set; } = null;

            public void OnInit( ParticleEffectHandle handle )
            {
                if( this.Force != null )
                {
                    var forceOverLifetime = handle.poolItem.particleSystem.forceOverLifetime;
                    forceOverLifetime.enabled = true;

                    this.Force.InitDrivers( handle );
                    var (min, max) = this.Force.Get();
                    forceOverLifetime.x = new ParticleSystem.MinMaxCurve( min.x, max.x );
                    forceOverLifetime.y = new ParticleSystem.MinMaxCurve( min.y, max.y );
                    forceOverLifetime.z = new ParticleSystem.MinMaxCurve( min.z, max.z );
                }
                if( this.Tint != null )
                {
                    var colorOverLifetime = handle.poolItem.particleSystem.colorOverLifetime;
                    colorOverLifetime.enabled = true;

                    this.Tint.InitDrivers( handle );
                    colorOverLifetime.color = this.Tint.GetMinMaxGradient();
                }
                if( this.Size != null )
                {
                    var sizeOverLifetime = handle.poolItem.particleSystem.sizeOverLifetime;
                    sizeOverLifetime.enabled = true;

                    this.Size.InitDrivers( handle );
                    var (min, max) = this.Size.Get();
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve( 1f, min, max );
                }
            }

            public void OnUpdate( ParticleEffectHandle handle )
            {
                if( this.Force?.drivers != null )
                {
                    var forceOverLifetime = handle.poolItem.particleSystem.forceOverLifetime;

                    var (min, max) = this.Force.Get();
                    forceOverLifetime.x = new ParticleSystem.MinMaxCurve( min.x, max.x );
                    forceOverLifetime.y = new ParticleSystem.MinMaxCurve( min.y, max.y );
                    forceOverLifetime.z = new ParticleSystem.MinMaxCurve( min.z, max.z );
                }
                if( this.Tint?.drivers != null )
                {
                    var colorOverLifetime = handle.poolItem.particleSystem.colorOverLifetime;

                    colorOverLifetime.color = this.Tint.GetMinMaxGradient();
                }
                if( this.Size?.drivers != null )
                {
                    var sizeOverLifetime = handle.poolItem.particleSystem.sizeOverLifetime;

                    var (min, max) = this.Size.Get();
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve( 1f, min, max );
                }
            }


            [MapsInheritingFrom( typeof( Lifetime ) )]
            public static SerializationMapping LifetimeMapping()
            {
                return new MemberwiseSerializationMapping<Lifetime>()
                    .WithMember( "force", o => o.Force )
                    .WithMember( "tint", o => o.Tint )
                    .WithMember( "size", o => o.Size );
            }
        }
        public Lifetime LifetimeValues { get; set; } = null;

        public sealed class Trail
        {
            public Material Material { get; set; }
            public ParticleEffectTrailStyle Style { get; set; } = ParticleEffectTrailStyle.Individual;
            public bool InheritTint { get; set; } = false;
            public bool WidthFromSize { get; set; } = true;
            public bool LifetimeFromSize { get; set; } = true;
            public bool DieWithParticle { get; set; } = true;
            /// <summary>
            /// Tint gradient along the trail.
            /// </summary>
            public ConstantEffectValue<float> TrailFrequency { get; set; } = new( 1.0f );
            public ConstantEffectValue<float> MinVertexDistance { get; set; } = new( 1f );
            public MinMaxEffectValue<float> Lifetime { get; set; } = new( 1.0f, 1.0f );
            public MinMaxEffectValue<float> Width { get; set; } = new( 1f );
            public MinMaxEffectValue<Color> Tint { get; set; } = null;
            public MinMaxEffectValue<Color> TintOverLifetime { get; set; } = null;

            public void OnInit( ParticleEffectHandle handle )
            {
                var renderer = handle.poolItem.renderer;

                var trails = handle.poolItem.particleSystem.trails;
                trails.enabled = true;

                renderer.trailMaterial = this.Material;
                trails.mode = this.Style switch
                {
                    ParticleEffectTrailStyle.Individual => ParticleSystemTrailMode.PerParticle,
                    ParticleEffectTrailStyle.Connected => ParticleSystemTrailMode.Ribbon,
                    _ => throw new ArgumentException( $"Invalid TrailValues.Style '{this.Style}'." )
                };
                trails.inheritParticleColor = this.InheritTint;
                trails.generateLightingData = true;
                trails.sizeAffectsWidth = this.WidthFromSize;
                trails.sizeAffectsLifetime = this.LifetimeFromSize;
                trails.dieWithParticles = this.DieWithParticle;

                if( this.MinVertexDistance != null )
                {
                    this.MinVertexDistance.InitDrivers( handle );
                    trails.minVertexDistance = this.MinVertexDistance.Get();
                }
                if( this.TrailFrequency != null )
                {
                    this.TrailFrequency.InitDrivers( handle );
                    trails.ratio = this.TrailFrequency.Get();
                }
                if( this.Lifetime != null )
                {
                    this.Lifetime.InitDrivers( handle );
                    trails.lifetime = this.Lifetime.GetMinMaxCurve();
                }
                if( this.Width != null )
                {
                    this.Width.InitDrivers( handle );
                    trails.widthOverTrail = this.Width.GetMinMaxCurve();
                }
                if( this.Tint != null )
                {
                    this.Tint.InitDrivers( handle );
                    trails.colorOverTrail = this.Tint.GetMinMaxGradient();
                }
                if( this.TintOverLifetime != null )
                {
                    this.TintOverLifetime.InitDrivers( handle );
                    trails.colorOverLifetime = this.TintOverLifetime.GetMinMaxGradient();
                }
            }

            public void OnUpdate( ParticleEffectHandle handle )
            {
                var renderer = handle.poolItem.renderer;
                var trails = handle.poolItem.particleSystem.trails;

                renderer.trailMaterial = this.Material;
                trails.mode = this.Style switch
                {
                    ParticleEffectTrailStyle.Individual => ParticleSystemTrailMode.PerParticle,
                    ParticleEffectTrailStyle.Connected => ParticleSystemTrailMode.Ribbon,
                    _ => throw new ArgumentException( $"Invalid TrailValues.Style '{this.Style}'." )
                };
                trails.inheritParticleColor = this.InheritTint;
                trails.generateLightingData = true;
                trails.sizeAffectsWidth = this.WidthFromSize;
                trails.sizeAffectsLifetime = this.LifetimeFromSize;
                trails.dieWithParticles = this.DieWithParticle;

                if( this.MinVertexDistance?.drivers != null )
                    trails.minVertexDistance = this.MinVertexDistance.Get();
                if( this.TrailFrequency?.drivers != null )
                    trails.ratio = this.TrailFrequency.Get();
                if( this.Lifetime?.drivers != null )
                    trails.lifetime = this.Lifetime.GetMinMaxCurve();
                if( this.Width?.drivers != null )
                    trails.widthOverTrail = this.Width.GetMinMaxCurve();
                if( this.Tint?.drivers != null )
                    trails.colorOverTrail = this.Tint.GetMinMaxGradient();
                if( this.TintOverLifetime?.drivers != null )
                    trails.colorOverLifetime = this.TintOverLifetime.GetMinMaxGradient();
            }


            [MapsInheritingFrom( typeof( Trail ) )]
            public static SerializationMapping TrailMapping()
            {
                return new MemberwiseSerializationMapping<Trail>()
                    .WithMember( "material", ObjectContext.Asset, t => t.Material )
                    .WithMember( "style", t => t.Style )
                    .WithMember( "inherit_tint", t => t.InheritTint )
                    .WithMember( "width_from_size", t => t.WidthFromSize )
                    .WithMember( "lifetime_from_size", t => t.LifetimeFromSize )
                    .WithMember( "die_with_particle", t => t.DieWithParticle )
                    .WithMember( "trail_frequency", t => t.TrailFrequency )
                    .WithMember( "min_vertex_distance", t => t.MinVertexDistance )
                    .WithMember( "lifetime", t => t.Lifetime )
                    .WithMember( "width", t => t.Width )
                    .WithMember( "tint", t => t.Tint )
                    .WithMember( "tint_over_lifetime", t => t.TintOverLifetime );
            }
        }
        public Trail TrailValues { get; set; } = null;


        public void OnInit( ParticleEffectHandle handle )
        {
            handle.EnsureValid();

            var main = handle.poolItem.main;

            handle.TargetTransform = this.TargetTransform;
            main.duration = this.Duration;

            if( RenderValues != null )
            {
                RenderValues.OnInit( handle );
            }

            if( InitialValues != null )
            {
                InitialValues.OnInit( handle );
            }

            if( EmissionValues != null )
            {
                EmissionValues.OnInit( handle );
            }

            if( LifetimeValues != null )
            {
                LifetimeValues.OnInit( handle );
            }

            if( TrailValues != null )
            {
                TrailValues.OnInit( handle );
            }
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            handle.EnsureValid();

            if( RenderValues != null )
            {
                RenderValues.OnUpdate( handle );
            }

            if( InitialValues != null )
            {
                InitialValues.OnUpdate( handle );
            }

            if( EmissionValues != null )
            {
                EmissionValues.OnUpdate( handle );
            }

            if( LifetimeValues != null )
            {
                LifetimeValues.OnUpdate( handle );
            }

            if( TrailValues != null )
            {
                TrailValues.OnUpdate( handle );
            }
        }

        public IEffectHandle Play()
        {
            return ParticleEffectManager.Play( this );
        }


        [MapsInheritingFrom( typeof( ParticleEffectDefinition ) )]
        public static SerializationMapping ParticleEffectDefinitionMapping()
        {
            return new MemberwiseSerializationMapping<ParticleEffectDefinition>()
                .WithMember( "render", o => o.RenderValues )
                .WithMember( "duration", o => o.Duration )
                .WithMember( "loop", o => o.Loop )
                .WithMember( "target_transform", o => o.TargetTransform )
#warning TODO - simulation spaces (local, 'global', and mixes of both)
                //.WithMember( "simulation_space", o => o.SimulationSpace )

                .WithMember( "initial", o => o.InitialValues )
                .WithMember( "emission", o => o.EmissionValues )
                .WithMember( "lifetime", o => o.LifetimeValues )
                .WithMember( "trails", o => o.TrailValues );
        }
    }
}