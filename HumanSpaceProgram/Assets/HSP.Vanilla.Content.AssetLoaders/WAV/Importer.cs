using System.IO;
using UnityEngine;

namespace HSP.Vanilla.Content.AssetLoaders.WAV
{
    public static class Importer
    {
        public static AudioClip LoadWAV( string filePath )
        {
            byte[] bytes = File.ReadAllBytes( filePath );
            return LoadWAV( bytes, Path.GetFileNameWithoutExtension( filePath ) );
        }

        public static AudioClip LoadWAV( byte[] bytes, string name )
        {
            // RIFF
            if( bytes[0] != 0x52 || bytes[1] != 0x49 || bytes[2] != 0x46 || bytes[3] != 0x46 )
                throw new IOException( "Invalid ChunkID - Expected RIFF" );

            int chunkSize = BytesToInt32( bytes, 4 );

            // WAVE
            if( bytes[8] != 0x57 || bytes[9] != 0x41 || bytes[10] != 0x56 || bytes[11] != 0x45 )
                throw new IOException( "Invalid Format - Expected WAVE" );

            // fmt
            if( bytes[12] != 0x66 || bytes[13] != 0x6d || bytes[14] != 0x74 || bytes[15] != 0x20 )
                throw new IOException( "Invalid Format - Expected fmt " );

            int subchunk1Size = BytesToInt32( bytes, 16 );
            short audioFormat = BytesToInt16( bytes, 20 );
            short numChannels = BytesToInt16( bytes, 22 );
            int sampleRate = BytesToInt32( bytes, 24 );
            // int byteRate = BytesToInt32( bytes, 28 );
            // short blockAlign = BytesToInt16( bytes, 32 );
            short bitsPerSample = BytesToInt16( bytes, 34 );

            if( audioFormat != 1 )
                throw new IOException( "Invalid Format - Only PCM supported." );

            // data
            int pos = 20 + subchunk1Size;
            while( pos < bytes.Length )
            {
                if( bytes[pos] == 0x64 && bytes[pos + 1] == 0x61 && bytes[pos + 2] == 0x74 && bytes[pos + 3] == 0x61 ) // "data"
                {
                    break;
                }
                pos++;
                if( pos > 1000 )
                    throw new IOException( "Could not find data chunk" );
            }

            int subchunk2Size = BytesToInt32( bytes, pos + 4 );
            int dataStart = pos + 8;

            int numSamples = subchunk2Size / (numChannels * (bitsPerSample / 8));
            float[] pcmData = new float[numSamples * numChannels];

            int bytePos = dataStart;
            for( int i = 0; i < numSamples; i++ )
            {
                for( int c = 0; c < numChannels; c++ )
                {
                    if( bitsPerSample == 8 )
                    {
                        pcmData[numChannels * i + c] = (bytes[bytePos] / 128.0f) - 1.0f; // 8-bit is unsigned 0..255
                        bytePos += 1;
                    }
                    else if( bitsPerSample == 16 )
                    {
                        pcmData[numChannels * i + c] = BytesToSample16bit( bytes[bytePos], bytes[bytePos + 1] );
                        bytePos += 2;
                    }
                }
            }

            AudioClip audioClip = AudioClip.Create( name, numSamples, numChannels, sampleRate, false );
            audioClip.SetData( pcmData, 0 );

            return audioClip;
        }

        private static float BytesToSample16bit( byte low, byte high )
        {
            return (short)((high << 8) | low) / 32768.0f;
        }

        private static int BytesToInt32( byte[] bytes, int offset = 0 )
        {
            int value = 0;
            for( int i = 0; i < 4; i++ ) value |= (bytes[offset + i]) << (i * 8);
            return value;
        }

        private static short BytesToInt16( byte[] bytes, int offset = 0 )
        {
            int value = 0;
            for( int i = 0; i < 2; i++ ) value |= (bytes[offset + i]) << (i * 8);
            return (short)value;
        }
    }
}