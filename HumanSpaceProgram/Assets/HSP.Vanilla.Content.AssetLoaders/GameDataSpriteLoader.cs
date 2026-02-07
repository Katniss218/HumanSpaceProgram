using HSP.Content;
using HSP.Vanilla.Content.AssetLoaders.Metadata;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace HSP.Vanilla.Content.AssetLoaders
{
    public class SpriteLoader : IAssetLoader
    {
        public const string RELOAD_SPRITES = HSPEvent.NAMESPACE_HSP + ".gdas.reload_sprites";

        // We register SpriteLoader BEFORE JsonLoader (implicitly via event order, though logic handles types).
        // This ensures if there's ambiguity on a .json file, the more specific Sprite loader gets a look first.
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_SPRITES, Before = new[] { JsonLoader.RELOAD_JSON_DATA } )]
        private static void RegisterSpriteLoader()
        {
            AssetRegistry.RegisterLoader( new SpriteLoader() );
        }

        public Type OutputType => typeof( Sprite );

        public bool CanLoad( AssetDataHandle handle, Type targetType )
        {
            if( handle.Format != CoreFormats.Json )
                return false;

            if( handle.TryGetLocalFilePath( out string path ) )
            {
                string fileName = Path.GetFileNameWithoutExtension( path );
                return fileName.EndsWith( "_sprite", StringComparison.OrdinalIgnoreCase );
            }

            return false;
        }

        public async Task<object> LoadAsync( AssetDataHandle handle, Type targetType, CancellationToken ct )
        {
            // 1. Deserialize Metadata (Background)
            SpriteMetadata meta;
            using( Stream stream = await handle.OpenMainStreamAsync( ct ).ConfigureAwait( false ) )
            using( StreamReader sr = new StreamReader( stream ) )
            {
                string json = await sr.ReadToEndAsync().ConfigureAwait( false );
                var data = new JsonStringReader( json ).Read();
                meta = SerializationUnit.Deserialize<SpriteMetadata>( data );
            }

            // 2. Resolve Texture
            Texture2D texture = null;
            string jsonPath = null;

            if( handle.TryGetLocalFilePath( out jsonPath ) )
            {
                string dir = Path.GetDirectoryName( jsonPath );
                string fileName = Path.GetFileNameWithoutExtension( jsonPath );
                string baseName = fileName.Substring( 0, fileName.LastIndexOf( "_sprite", StringComparison.OrdinalIgnoreCase ) );

                if( Directory.Exists( dir ) )
                {
                    string[] extensions = new[] { ".png", ".jpg", ".jpeg", ".tga", ".dds" };
                    string texturePath = null;

                    foreach( var ext in extensions )
                    {
                        string probe = Path.Combine( dir, baseName + ext );
                        if( File.Exists( probe ) )
                        {
                            texturePath = probe;
                            break;
                        }
                    }

                    if( texturePath != null )
                    {
                        string textureId = HumanSpaceProgramContent.GetAssetID( texturePath );
                        if( !string.IsNullOrEmpty( textureId ) )
                        {
#warning TODO - there exists both a scenario.json (metadata for scenario itself), and a scenario.png (icon) files, leading to bad discovery.
                            // AssetRegistry.GetAsync handles its own threading, we await it here.
                            texture = await AssetRegistry.GetAsync<Texture2D>( textureId ).ConfigureAwait( false );
                        }
                    }
                }
            }

            if( texture == null )
            {
                Debug.LogError( $"[SpriteLoader] Failed to resolve texture for sprite definition '{jsonPath ?? "unknown"}'" );
                return null;
            }

            // 3. Create Sprite (Main Thread)
            return await MainThreadDispatcher.RunAsync( () =>
            {
                Vector2 pivot = meta.Pivot;
                if( pivot.x > 1.0f || pivot.y > 1.0f || pivot.x < -1.0f || pivot.y < -1.0f )
                {
                    pivot = new Vector2( pivot.x / meta.Rect.width, pivot.y / meta.Rect.height );
                }

                Rect rect = meta.Rect;
                if( rect.width <= 0 || rect.height <= 0 )
                {
                    rect = new Rect( 0, 0, texture.width, texture.height );
                }

                Sprite sprite = Sprite.Create( texture, rect, pivot, 100.0f, 0, SpriteMeshType.FullRect, meta.Border );
                sprite.name = Path.GetFileNameWithoutExtension( jsonPath ?? "Sprite" );

                return sprite;
            } ).ConfigureAwait( false );
        }
    }
}