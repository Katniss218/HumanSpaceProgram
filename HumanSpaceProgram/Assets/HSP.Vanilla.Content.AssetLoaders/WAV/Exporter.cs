
using System;
using System.IO;
using UnityEngine;

namespace HSP.Vanilla.Content.AssetLoaders.WAV
{
    public static class Exporter
    {
        public static void Export( string filePath, AudioClip clip )
        {
            if( clip == null )
                throw new ArgumentNullException( nameof( clip ) );

            string dir = Path.GetDirectoryName( filePath );
            if( !string.IsNullOrEmpty( dir ) && !Directory.Exists( dir ) )
                Directory.CreateDirectory( dir );

            using( FileStream fs = new FileStream( filePath, FileMode.Create, FileAccess.Write ) )
            using( BinaryWriter bw = new BinaryWriter( fs ) )
            {
                // Parameters
                int channels = clip.channels;
                int frequency = clip.frequency;
                int samples = clip.samples;
                int bitsPerSample = 16;

                // Calculate chunk sizes
                int subChunk1Size = 16; // PCM
                int subChunk2Size = samples * channels * (bitsPerSample / 8);
                int chunkSize = 4 + (8 + subChunk1Size) + (8 + subChunk2Size);

                // --- RIFF Header ---
                bw.Write( 0x46464952 ); // "RIFF"
                bw.Write( chunkSize );
                bw.Write( 0x45564157 ); // "WAVE"

                // --- fmt Subchunk ---
                bw.Write( 0x20746d66 ); // "fmt "
                bw.Write( subChunk1Size );
                bw.Write( (short)1 ); // AudioFormat 1 = PCM
                bw.Write( (short)channels );
                bw.Write( frequency );

                int byteRate = frequency * channels * (bitsPerSample / 8);
                bw.Write( byteRate );

                short blockAlign = (short)(channels * (bitsPerSample / 8));
                bw.Write( blockAlign );
                bw.Write( (short)bitsPerSample );

                // --- data Subchunk ---
                bw.Write( 0x61746164 ); // "data"
                bw.Write( subChunk2Size );

                // Write Data
                // We need to read the float data from the clip and convert to Int16
                float[] data = new float[samples * channels];
                clip.GetData( data, 0 );

                foreach( float sample in data )
                {
                    // Clamp to [-1, 1]
                    float s = sample;
                    if( s > 1f )
                        s = 1f;
                    else if( s < -1f )
                        s = -1f;

                    // Scale to Int16 range
                    short shortSample = (short)(s * 32767);
                    bw.Write( shortSample );
                }
            }
        }
    }
}