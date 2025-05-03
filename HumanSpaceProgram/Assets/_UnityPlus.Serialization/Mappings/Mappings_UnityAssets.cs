using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityPlus.Serialization
{
    public struct TextureInfo
    {
        public Texture texture;
        public Vector2 offset;
        public Vector2 scale;


        [MapsInheritingFrom( typeof( TextureInfo ) )]
        public static SerializationMapping TextureInfoMapping()
        {
            return new MemberwiseSerializationMapping<TextureInfo>()
                .WithMember( "texture", ObjectContext.Asset, o => o.texture )
                .WithMember( "offset", o => o.offset )
                .WithMember( "scale", o => o.scale );
        }
    }
}

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_UnityAssets
    {
        [MapsInheritingFrom( typeof( Material ) )]
        public static SerializationMapping MaterialMapping()
        {
            return new MemberwiseSerializationMapping<Material>()
                .WithReadonlyMember( "shader", ObjectContext.Asset, o => o.shader )
                .WithFactory<Shader>( s => new Material( s ) )
                .WithMember( "textures", o =>
                {
                    var shader = o.shader;
                    var textures = new Dictionary<string, TextureInfo>();
                    int count = shader.GetPropertyCount();
                    for( int i = 0; i < count; i++ )
                    {
                        if( shader.GetPropertyType( i ) != ShaderPropertyType.Texture )
                            continue;

                        string name = shader.GetPropertyName( i );
                        TextureInfo info = new()
                        {
                            texture = o.GetTexture( shader.GetPropertyNameId( i ) ),
                            offset = o.GetTextureOffset( shader.GetPropertyNameId( i ) ),
                            scale = o.GetTextureScale( shader.GetPropertyNameId( i ) )
                        };
                        textures.Add( name, info );
                    }
                    return textures;
                }, ( o, value ) =>
                {
                    var shader = o.shader;
                    foreach( (string name, TextureInfo info) in value )
                    {
                        o.SetTexture( Shader.PropertyToID( name ), info.texture );
                        o.SetTextureOffset( Shader.PropertyToID( name ), info.offset );
                        o.SetTextureScale( Shader.PropertyToID( name ), info.scale );
                    }
                } )
                .WithMember( "vectors", o =>
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
                }, ( o, value ) =>
                {
                    var shader = o.shader;
                    foreach( (string name, Vector4 val) in value )
                    {
                        o.SetVector( Shader.PropertyToID( name ), val );
                    }
                } )
                .WithMember( "colors", o =>
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
                }, ( o, value ) =>
                {
                    var shader = o.shader;
                    foreach( (string name, Color val) in value )
                    {
                        o.SetColor( Shader.PropertyToID( name ), val );
                    }
                } )
                .WithMember( "floats", o =>
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
                }, ( o, value ) =>
                {
                    var shader = o.shader;
                    foreach( (string name, float val) in value )
                    {
                        o.SetFloat( Shader.PropertyToID( name ), val );
                    }
                } )
                .WithMember( "ints", o =>
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
                }, ( o, value ) =>
                {
                    var shader = o.shader;
                    foreach( (string name, int val) in value )
                    {
                        o.SetInteger( Shader.PropertyToID( name ), val );
                    }
                } )
                .WithMember( "keywords", o =>
                {
                    return o.enabledKeywords.Select( kw => kw.name ).ToArray();
                }, ( o, value ) =>
                {
                    foreach( string keyword in value )
                    {
                        o.EnableKeyword( keyword );
                    }
                } );
        }
    }
}