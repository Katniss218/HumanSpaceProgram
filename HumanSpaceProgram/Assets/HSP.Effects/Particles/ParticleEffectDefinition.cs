using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles
{
    public interface IParticleEffectEmissionShape
    {
        public void OnInit( ParticleEffectHandle handle );

        public void OnUpdate( ParticleEffectHandle handle );
    }

    public interface IParticleEffectRenderMode
    {
        public void OnInit( ParticleEffectHandle handle );

        public void OnUpdate( ParticleEffectHandle handle );
    }

    public enum TrailStyle
    {
        Individual,
        Connected
    }

    public class ParticleEffectDefinition : IParticleEffectData
    {
        // particles need to specify a frame
        // following an object's position, rotation, in planet space, etc.

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

            public MinMaxEffectValue<float> SpawnRate { get; set; } = new( 10.0f );
            public ConstantEffectValue<int> MaxParticles { get; set; } = new( 1000 );

            public IParticleEffectEmissionShape SpawnShape { get; set; }

            public ConstantEffectValue<float> RandomPosition { get; set; } = null;
            public ConstantEffectValue<float> RandomDirection { get; set; } = null;


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
            public TrailStyle Style { get; set; } = TrailStyle.Individual;
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
            var main = handle.poolItem.main;
            var renderer = handle.poolItem.renderer;
            var emission = handle.poolItem.particleSystem.emission;
            var shape = handle.poolItem.particleSystem.shape;

            handle.TargetTransform = this.TargetTransform;
            main.duration = this.Duration;

            if( RenderValues != null )
            {
                RenderValues.RenderMode.OnInit( handle );
                handle.Material = this.RenderValues.Material;
                if( this.RenderValues.VertexStreams != null )
                {
                    handle.poolItem.renderer.SetActiveVertexStreams( this.RenderValues.VertexStreams.ToList() );
                }
                handle.poolItem.renderer.shadowCastingMode = this.RenderValues.ShadowCastingMode;
                handle.poolItem.renderer.receiveShadows = this.RenderValues.ReceiveShadows;
                if( this.RenderValues.Pivot != null )
                    renderer.pivot = this.RenderValues.Pivot.Get();
            }

#warning TODO - add initializers to all of these properties.

            if( InitialValues != null )
            {
                var rotationOverLifetime = handle.poolItem.particleSystem.rotationOverLifetime;

                if( this.InitialValues.Size != null )
                    main.startSize = this.InitialValues.Size.GetMinMaxCurve();
                if( this.InitialValues.Tint != null )
                    main.startColor = this.InitialValues.Tint.GetMinMaxGradient();
                if( this.InitialValues.Lifetime != null )
                    main.startLifetime = this.InitialValues.Lifetime.GetMinMaxCurve();
                if( this.InitialValues.Rotation != null )
                    main.startRotation = this.InitialValues.Rotation.GetMinMaxCurve();
                if( this.InitialValues.Speed != null )
                    main.startSpeed = this.InitialValues.Speed.GetMinMaxCurve();
                if( this.InitialValues.AngularSpeed != null )
                {
                    rotationOverLifetime.enabled = true;
                    rotationOverLifetime.z = this.InitialValues.AngularSpeed.GetMinMaxCurve();
                }
                if( this.InitialValues.AdditionalLinearVelocity != null )
                {
                    var velocityOverLifetime = handle.poolItem.particleSystem.velocityOverLifetime;
                    velocityOverLifetime.enabled = true;
                    var (min, max) = this.InitialValues.AdditionalLinearVelocity.Get();
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve( min.x, max.x );
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve( min.y, max.y );
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve( min.z, max.z );
                }
                if( this.InitialValues.AdditionalRadialVelocity != null )
                {
                    rotationOverLifetime.enabled = true;
                    rotationOverLifetime.z = this.InitialValues.AngularSpeed.GetMinMaxCurve();
                }
            }

            if( EmissionValues != null )
            {
                main.maxParticles = this.EmissionValues.MaxParticles.Get();
                if( this.EmissionValues.SpawnRate != null )
                    emission.rateOverTime = this.EmissionValues.SpawnRate.GetMinMaxCurve();
                if( this.EmissionValues.Position != null )
                    shape.position = this.EmissionValues.Position.Get();
                if( this.EmissionValues.Rotation != null )
                    shape.rotation = this.EmissionValues.Rotation.Get().eulerAngles;
                if( this.EmissionValues.SpawnRate != null )
                    emission.rateOverTime = this.EmissionValues.SpawnRate.GetMinMaxCurve();
                if( this.EmissionValues.RandomPosition != null )
                    shape.randomPositionAmount = this.EmissionValues.RandomPosition.Get();
                if( this.EmissionValues.RandomDirection != null )
                    shape.randomDirectionAmount = this.EmissionValues.RandomDirection.Get();
                this.EmissionValues.SpawnShape.OnInit( handle );
            }

            if( LifetimeValues != null )
            {
                if( this.LifetimeValues.Force != null )
                {
                    var forceOverLifetime = handle.poolItem.particleSystem.forceOverLifetime;
                    forceOverLifetime.enabled = true;
                    var (min, max) = this.LifetimeValues.Force.Get();
                    forceOverLifetime.x = new ParticleSystem.MinMaxCurve( min.x, max.x );
                    forceOverLifetime.y = new ParticleSystem.MinMaxCurve( min.y, max.y );
                    forceOverLifetime.z = new ParticleSystem.MinMaxCurve( min.z, max.z );
                }
                if( this.LifetimeValues.Tint != null )
                {
                    var colorOverLifetime = handle.poolItem.particleSystem.colorOverLifetime;

                    colorOverLifetime.color = this.LifetimeValues.Tint.GetMinMaxGradient();
                }
                if( this.LifetimeValues.Size != null )
                {
                    var sizeOverLifetime = handle.poolItem.particleSystem.sizeOverLifetime;
                    var (min, max) = this.LifetimeValues.Size.Get();
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve( 1f, min, max );
                }
            }

            if( TrailValues != null )
            {
                var trails = handle.poolItem.particleSystem.trails;
                trails.enabled = true;

                renderer.trailMaterial = this.TrailValues.Material;
                trails.mode = this.TrailValues.Style switch
                {
                    TrailStyle.Individual => ParticleSystemTrailMode.PerParticle,
                    TrailStyle.Connected => ParticleSystemTrailMode.Ribbon,
                    _ => throw new ArgumentException( $"Invalid TrailValues.Style '{this.TrailValues.Style}'." )
                };
                trails.inheritParticleColor = this.TrailValues.InheritTint;
                trails.generateLightingData = true;
                trails.sizeAffectsWidth = this.TrailValues.WidthFromSize;
                trails.sizeAffectsLifetime = this.TrailValues.LifetimeFromSize;
                trails.dieWithParticles = this.TrailValues.DieWithParticle;

                if( this.TrailValues.MinVertexDistance != null )
                    trails.minVertexDistance = this.TrailValues.MinVertexDistance.Get();
                if( this.TrailValues.TrailFrequency != null )
                    trails.ratio = this.TrailValues.TrailFrequency.Get();
                if( this.TrailValues.Lifetime != null )
                    trails.lifetime = this.TrailValues.Lifetime.GetMinMaxCurve();
                if( this.TrailValues.Width != null )
                    trails.widthOverTrail = this.TrailValues.Width.GetMinMaxCurve();
                if( this.TrailValues.Tint != null )
                    trails.colorOverTrail = this.TrailValues.Tint.GetMinMaxGradient();
                if( this.TrailValues.TintOverLifetime != null )
                    trails.colorOverLifetime = this.TrailValues.TintOverLifetime.GetMinMaxGradient();
            }
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            var main = handle.poolItem.main;
            var renderer = handle.poolItem.renderer;
            var emission = handle.poolItem.particleSystem.emission;
            var shape = handle.poolItem.particleSystem.shape;

            if( RenderValues != null )
            {
                RenderValues.RenderMode.OnUpdate( handle );
                if( this.RenderValues.Pivot != null )
                    renderer.pivot = this.RenderValues.Pivot.Get();
            }

            if( InitialValues != null )
            {
                var rotationOverLifetime = handle.poolItem.particleSystem.rotationOverLifetime;

                if( this.InitialValues.Size?.drivers != null )
                    main.startSize = this.InitialValues.Size.GetMinMaxCurve();
                if( this.InitialValues.Tint?.drivers != null )
                    main.startColor = this.InitialValues.Tint.GetMinMaxGradient();
                if( this.InitialValues.Lifetime?.drivers != null )
                    main.startLifetime = this.InitialValues.Lifetime.GetMinMaxCurve();
                if( this.InitialValues.Rotation?.drivers != null )
                    main.startRotation = this.InitialValues.Rotation.GetMinMaxCurve();
                if( this.InitialValues.Speed?.drivers != null )
                    main.startSpeed = this.InitialValues.Speed.GetMinMaxCurve();
                if( this.InitialValues.AngularSpeed?.drivers != null )
                {
                    rotationOverLifetime.enabled = true;
                    rotationOverLifetime.z = this.InitialValues.AngularSpeed.GetMinMaxCurve();
                }
                if( this.InitialValues.AdditionalLinearVelocity?.drivers != null )
                {
                    var velocityOverLifetime = handle.poolItem.particleSystem.velocityOverLifetime;
                    velocityOverLifetime.enabled = true;
                    var (min, max) = this.InitialValues.AdditionalLinearVelocity.Get();
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve( min.x, max.x );
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve( min.y, max.y );
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve( min.z, max.z );
                }
                if( this.InitialValues.AdditionalRadialVelocity?.drivers != null )
                {
                    rotationOverLifetime.enabled = true;
                    rotationOverLifetime.z = this.InitialValues.AngularSpeed.GetMinMaxCurve();
                }
            }

            if( EmissionValues != null )
            {
                if( this.EmissionValues.MaxParticles?.drivers != null )
                    main.maxParticles = this.EmissionValues.MaxParticles.Get();
                if( this.EmissionValues.SpawnRate?.drivers != null )
                    emission.rateOverTime = this.EmissionValues.SpawnRate.GetMinMaxCurve();
                if( this.EmissionValues.Position?.drivers != null )
                    shape.position = this.EmissionValues.Position.Get();
                if( this.EmissionValues.Rotation?.drivers != null )
                    shape.rotation = this.EmissionValues.Rotation.Get().eulerAngles;
                if( this.EmissionValues.SpawnRate?.drivers != null )
                    emission.rateOverTime = this.EmissionValues.SpawnRate.GetMinMaxCurve();
                if( this.EmissionValues.RandomPosition?.drivers != null )
                    shape.randomPositionAmount = this.EmissionValues.RandomPosition.Get();
                if( this.EmissionValues.RandomDirection?.drivers != null )
                    shape.randomDirectionAmount = this.EmissionValues.RandomDirection.Get();
                this.EmissionValues.SpawnShape.OnInit( handle );
            }

            if( LifetimeValues != null )
            {
                if( this.LifetimeValues.Force?.drivers != null )
                {
                    var forceOverLifetime = handle.poolItem.particleSystem.forceOverLifetime;
                    forceOverLifetime.enabled = true;
                    var (min, max) = this.LifetimeValues.Force.Get();
                    forceOverLifetime.x = new ParticleSystem.MinMaxCurve( min.x, max.x );
                    forceOverLifetime.y = new ParticleSystem.MinMaxCurve( min.y, max.y );
                    forceOverLifetime.z = new ParticleSystem.MinMaxCurve( min.z, max.z );
                }
                if( this.LifetimeValues.Tint?.drivers != null )
                {
                    var colorOverLifetime = handle.poolItem.particleSystem.colorOverLifetime;

                    colorOverLifetime.color = this.LifetimeValues.Tint.GetMinMaxGradient();
                }
                if( this.LifetimeValues.Size?.drivers != null )
                {
                    var sizeOverLifetime = handle.poolItem.particleSystem.sizeOverLifetime;
                    var (min, max) = this.LifetimeValues.Size.Get();
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve( 1f, min, max );
                }
            }

            if( TrailValues != null )
            {
                var trails = handle.poolItem.particleSystem.trails;

                renderer.trailMaterial = this.TrailValues.Material;
                trails.mode = this.TrailValues.Style switch
                {
                    TrailStyle.Individual => ParticleSystemTrailMode.PerParticle,
                    TrailStyle.Connected => ParticleSystemTrailMode.Ribbon,
                    _ => throw new ArgumentException( $"Invalid TrailValues.Style '{this.TrailValues.Style}'." )
                };
                trails.inheritParticleColor = this.TrailValues.InheritTint;
                trails.generateLightingData = true;
                trails.sizeAffectsWidth = this.TrailValues.WidthFromSize;
                trails.sizeAffectsLifetime = this.TrailValues.LifetimeFromSize;
                trails.dieWithParticles = this.TrailValues.DieWithParticle;

                if( this.TrailValues.MinVertexDistance?.drivers != null )
                    trails.minVertexDistance = this.TrailValues.MinVertexDistance.Get();
                if( this.TrailValues.TrailFrequency?.drivers != null )
                    trails.ratio = this.TrailValues.TrailFrequency.Get();
                if( this.TrailValues.Lifetime?.drivers != null )
                    trails.lifetime = this.TrailValues.Lifetime.GetMinMaxCurve();
                if( this.TrailValues.Width?.drivers != null )
                    trails.widthOverTrail = this.TrailValues.Width.GetMinMaxCurve();
                if( this.TrailValues.Tint?.drivers != null )
                    trails.colorOverTrail = this.TrailValues.Tint.GetMinMaxGradient();
                if( this.TrailValues.TintOverLifetime?.drivers != null )
                    trails.colorOverLifetime = this.TrailValues.TintOverLifetime.GetMinMaxGradient();
            }
        }



        [MapsInheritingFrom( typeof( ParticleEffectDefinition ) )]
        public static SerializationMapping ParticleEffectDefinitionMapping()
        {
            return new MemberwiseSerializationMapping<ParticleEffectDefinition>()
                .WithMember( "render", o => o.RenderValues )
                .WithMember( "duration", o => o.Duration )
                .WithMember( "loop", o => o.Loop )
                .WithMember( "target_transform", o => o.TargetTransform )
                //.WithMember( "simulation_space", o => o.SimulationSpace )

                .WithMember( "initial", o => o.InitialValues )
                .WithMember( "emission", o => o.EmissionValues )
                .WithMember( "lifetime", o => o.LifetimeValues )
                .WithMember( "trails", o => o.TrailValues );
        }

        public IEffectHandle Play()
        {
            return ParticleEffectManager.Play( this );
        }
    }

    public static class A
    {

        public static ParticleSystem.MinMaxCurve GetMinMaxCurve( this ConstantEffectValue<float> value )
        {
            return new ParticleSystem.MinMaxCurve( value.Get() );
        }

        public static ParticleSystem.MinMaxCurve GetMinMaxCurve( this MinMaxEffectValue<float> value )
        {
            var minMax = value.Get();
            return new ParticleSystem.MinMaxCurve( minMax.min, minMax.max );
        }

        public static ParticleSystem.MinMaxGradient GetMinMaxGradient( this ConstantEffectValue<Color> value )
        {
            return new ParticleSystem.MinMaxGradient( value.Get() );
        }

        public static ParticleSystem.MinMaxGradient GetMinMaxGradient( this MinMaxEffectValue<Color> value )
        {
            var minMax = value.Get();
            return new ParticleSystem.MinMaxGradient( minMax.min, minMax.max );
        }
    }
}