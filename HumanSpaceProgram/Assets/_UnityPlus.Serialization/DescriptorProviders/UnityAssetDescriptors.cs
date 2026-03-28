using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityPlus.Serialization.Descriptors;

namespace UnityPlus.Serialization.DescriptorProviders
{
    public static class UnityAssetDescriptors
    {
        [MapsAnyInterface( ContextType = typeof( Ctx.Asset ) )]
        [MapsInheritingFrom( typeof( object ), ContextType = typeof( Ctx.Asset ) )]
        public static IDescriptor ProvideAsset<T>() where T : class
        {
            return new AssetDescriptor<T>();
        }

        public struct TextureInfo
        {
            public Texture texture;
            public Vector2 offset;
            public Vector2 scale;
        }

        [MapsInheritingFrom( typeof( TextureInfo ) )]
        public static IDescriptor TextureInfoGetter() => new MemberwiseDescriptor<TextureInfo>()
            .WithMember( "texture", typeof( Ctx.Asset ), o => o.texture )
            .WithMember( "offset", o => o.offset )
            .WithMember( "scale", o => o.scale );

        [MapsInheritingFrom( typeof( Material ) )]
        public static IDescriptor Material() => new MemberwiseDescriptor<Material>()
            .WithReadonlyMember( "shader", typeof( Ctx.Asset ), o => o.shader )
            .WithFactory<Shader>( s => new Material( s ), "shader" )
            .WithMember( "textures",
                o =>
                {
                    var shader = o.shader;
                    var textures = new Dictionary<string, TextureInfo>();
                    int count = shader.GetPropertyCount();
                    for( int i = 0; i < count; i++ )
                    {
                        if( shader.GetPropertyType( i ) != ShaderPropertyType.Texture )
                            continue;

                        string name = shader.GetPropertyName( i );
                        TextureInfo info = new TextureInfo()
                        {
                            texture = o.GetTexture( shader.GetPropertyNameId( i ) ),
                            offset = o.GetTextureOffset( shader.GetPropertyNameId( i ) ),
                            scale = o.GetTextureScale( shader.GetPropertyNameId( i ) )
                        };
                        textures.Add( name, info );
                    }
                    return textures;
                },
                ( ref Material o, Dictionary<string, TextureInfo> value ) =>
                {
                    var shader = o.shader;
                    foreach( var kvp in value )
                    {
                        int id = Shader.PropertyToID( kvp.Key );
                        if( o.HasProperty( id ) )
                        {
                            o.SetTexture( id, kvp.Value.texture );
                            o.SetTextureOffset( id, kvp.Value.offset );
                            o.SetTextureScale( id, kvp.Value.scale );
                        }
                    }
                }
            )
            // Vectors
            .WithMember( "vectors",
                o =>
                {
                    var shader = o.shader;
                    var vectors = new Dictionary<string, Vector4>();
                    int count = shader.GetPropertyCount();
                    for( int i = 0; i < count; i++ )
                    {
                        if( shader.GetPropertyType( i ) != ShaderPropertyType.Vector )
                            continue;

                        string name = shader.GetPropertyName( i );
                        Vector4 val = o.GetVector( shader.GetPropertyNameId( i ) );
                        vectors.Add( name, val );
                    }
                    return vectors;
                },
                ( ref Material o, Dictionary<string, Vector4> value ) =>
                {
                    var shader = o.shader;
                    foreach( var kvp in value )
                    {
                        int id = Shader.PropertyToID( kvp.Key );
                        if( o.HasProperty( id ) )
                            o.SetVector( id, kvp.Value );
                    }
                }
            )
            // Colors
            .WithMember( "colors",
                o =>
                {
                    var shader = o.shader;
                    var colors = new Dictionary<string, Color>();
                    int count = shader.GetPropertyCount();
                    for( int i = 0; i < count; i++ )
                    {
                        if( shader.GetPropertyType( i ) != ShaderPropertyType.Color )
                            continue;

                        string name = shader.GetPropertyName( i );
                        Color val = o.GetColor( shader.GetPropertyNameId( i ) );
                        colors.Add( name, val );
                    }
                    return colors;
                },
                ( ref Material o, Dictionary<string, Color> value ) =>
                {
                    var shader = o.shader;
                    foreach( var kvp in value )
                    {
                        int id = Shader.PropertyToID( kvp.Key );
                        if( o.HasProperty( id ) )
                            o.SetColor( id, kvp.Value );
                    }
                }
            )
            // Floats
            .WithMember( "floats",
                o =>
                {
                    var shader = o.shader;
                    var floats = new Dictionary<string, float>();
                    int count = shader.GetPropertyCount();
                    for( int i = 0; i < count; i++ )
                    {
                        var type = shader.GetPropertyType( i );
                        if( type != ShaderPropertyType.Float && type != ShaderPropertyType.Range )
                            continue;

                        string name = shader.GetPropertyName( i );
                        float val = o.GetFloat( shader.GetPropertyNameId( i ) );
                        floats.Add( name, val );
                    }
                    return floats;
                },
                ( ref Material o, Dictionary<string, float> value ) =>
                {
                    var shader = o.shader;
                    foreach( var kvp in value )
                    {
                        int id = Shader.PropertyToID( kvp.Key );
                        if( o.HasProperty( id ) )
                            o.SetFloat( id, kvp.Value );
                    }
                }
            )
            // Ints
            .WithMember( "ints",
                o =>
                {
                    var shader = o.shader;
                    var ints = new Dictionary<string, int>();
                    int count = shader.GetPropertyCount();
                    for( int i = 0; i < count; i++ )
                    {
                        if( shader.GetPropertyType( i ) != ShaderPropertyType.Int )
                            continue;

                        string name = shader.GetPropertyName( i );
                        int val = o.GetInteger( shader.GetPropertyNameId( i ) );
                        ints.Add( name, val );
                    }
                    return ints;
                },
                ( ref Material o, Dictionary<string, int> value ) =>
                {
                    var shader = o.shader;
                    foreach( var kvp in value )
                    {
                        int id = Shader.PropertyToID( kvp.Key );
                        if( shader.FindPropertyIndex( kvp.Key ) != -1 )
                            o.SetInteger( id, kvp.Value );
                    }
                }
            )
            // Keywords
            .WithMember( "keywords",
                o =>
                {
                    return o.enabledKeywords.Select( kw => kw.name ).ToArray();
                },
                ( ref Material o, string[] value ) =>
                {
                    foreach( string keyword in value )
                    {
                        o.EnableKeyword( keyword );
                    }
                }
            );

        [MapsInheritingFrom( typeof( ScriptableObject ) )]
        public static IDescriptor GetScriptableObject() => new MemberwiseDescriptor<ScriptableObject>()
            .WithRawFactory( ( data, ctx ) =>
            {
                if( data.TryGetValue( KeyNames.TYPE, out var type ) )
                {
                    Type soType = type.DeserializeType();

                    ScriptableObject obj = ScriptableObject.CreateInstance( soType );
                    if( data.TryGetValue( KeyNames.ID, out var id ) )
                    {
                        ctx.ForwardMap.SetObj( id.DeserializeGuid(), obj );
                    }
                    return obj;
                }
                return null;
            } )
            .WithMember( "name", o => o.name, ( ref ScriptableObject o, object v ) => o.name = (string)v )
            .WithMember( "hideFlags", o => o.hideFlags, ( ref ScriptableObject o, object v ) => o.hideFlags = (HideFlags)v );
    }
}