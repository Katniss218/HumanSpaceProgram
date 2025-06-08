using System.IO;
using UnityEngine;

namespace HSP.Vanilla.Content.AssetLoaders.WAV
{
    public static class Importer
    {
        /// <remarks>
        /// This method can only handle PCM (uncompressed) wave files for now.
        /// </remarks>
        public static AudioClip LoadWAV( string filePath )
        {
            byte[] bytes = File.ReadAllBytes( filePath );

            //
            //	RIFF HEADER.
            //

            // File's RIFF marker (value: 'RIFF' (ASCII)).
            if( bytes[0] != 0x52 || bytes[1] != 0x49 || bytes[2] != 0x46 || bytes[3] != 0x46 )
            {
                throw new IOException( "Invalid ChunkID (Block: 0-3) - Expected value of '0x52494646', big endian." );
            }

            // chunk size - Block: 4-7
            int chunkSize = BytesToInt32( bytes, 4 );

            // File's WAVE type marker (value: 'WAVE' (ASCII)).
            if( bytes[8] != 0x57 || bytes[9] != 0x41 || bytes[10] != 0x56 || bytes[11] != 0x45 )
            {
                throw new IOException( "Invalid Format (Block: 8-11) - Expected value of '0x57415645', big endian." );
            }

            //
            //	fmt  SUBCHUNK.
            //

            // File's format chunk ID (value: 'fmt ' (ASCII)).
            if( bytes[12] != 0x66 || bytes[13] != 0x6d || bytes[14] != 0x74 || bytes[15] != 0x20 )
            {
                throw new IOException( "Invalid Format (Block: 12-15) - Expected value of '0x666d7420', big endian." );
            }

            // Should be == 16 (for PCM audio)
            int subchunk1Size = BytesToInt32( bytes, 16 );
            if( subchunk1Size != 16 )
            {
                throw new IOException( "Invalid Format - Malformed WAVE file!" );
            }

            short audioFormat = BytesToInt16( bytes, 20 );
            if( audioFormat != 1 )
            {
                throw new IOException( "Invalid Format - The importer currently only supports uncompressed (PCM) files." );
            }

            short numChannels = BytesToInt16( bytes, 22 );

            int sampleRate = BytesToInt32( bytes, 24 );

            int byteRate = BytesToInt32( bytes, 28 );

            short blockAlign = BytesToInt16( bytes, 32 );

            short bitsPerSample = BytesToInt16( bytes, 34 ); // each channel


            if( byteRate != sampleRate * numChannels * (bitsPerSample / 8) )
            {
                throw new IOException( "Invalid Format - Malformed WAVE file!" );
            }

            if( blockAlign != numChannels * (bitsPerSample / 8) )
            {
                throw new IOException( "Invalid Format - Malformed WAVE file!" );
            }

            //
            //	data SUBCHUNK.
            //

            // File's data chunk ID (value: 'data' (ASCII)).
            if( bytes[36] != 0x64 || bytes[37] != 0x61 || bytes[38] != 0x74 || bytes[39] != 0x61 )
            {
                throw new IOException( "Invalid Format (Block: 36-39) - Expected value of '0x64617461', big endian." );
            }

            // Should be == numSamples * numChannels * (bitsPerSample / 8)
            int subchunk2Size = BytesToInt32( bytes, 40 );

            if( chunkSize != 4 + (8 + subchunk1Size) + (8 + subchunk2Size) )
            {
                throw new IOException( "Invalid Format - Malformed WAVE file!" );
            }

            //									 #  \/ bytes-per-sample * channels \/
            int numSamples = (bytes.Length - 44) / (numChannels * (bitsPerSample / 8)); // Calculate the number of samples.
            if( subchunk2Size != numSamples * numChannels * (bitsPerSample / 8) )
            {
                throw new IOException( "Invalid Format - Malformed WAVE file!" );
            }

            // Reading of the actual samples starts here.

            float[] pcmData = new float[numSamples * numChannels];
            int pos = 44; // First sample's byte offset.

            for( int i = 0; i < numSamples; i++ )
            {
                for( int currentChannel = 0; currentChannel < numChannels; currentChannel++ )
                {
                    if( bitsPerSample == 8 )
                    {
                        pcmData[numChannels * i + currentChannel] = BytesToSample8bit( bytes[pos] );
                        pos += 1;
                        continue;
                    }
                    if( bitsPerSample == 16 )
                    {
                        pcmData[numChannels * i + currentChannel] = BytesToSample16bit( bytes[pos], bytes[pos + 1] );
                        pos += 2;
                        continue;
                    }
                }
            }

            AudioClip audioClip = AudioClip.Create( filePath, numSamples, numChannels, sampleRate, false );
            audioClip.SetData( pcmData, 0 );

            return audioClip;
        }

        private static float BytesToSample16bit( byte low, byte high )
        {
            return (short)((high << 8) | low) / 32768.0f;
        }

        private static float BytesToSample8bit( byte value )
        {
            return value / 128.0f;
        }

        private static int BytesToInt32( byte[] bytes, int offset = 0 )
        {
            int value = 0;
            for( int i = 0; i < 4; i++ )
            {
                value |= (bytes[offset + i]) << (i * 8); // little endian
            }
            return value;
        }

        private static short BytesToInt16( byte[] bytes, int offset = 0 )
        {
            int value = 0;
            for( int i = 0; i < 2; i++ )
            {
                value |= (bytes[offset + i]) << (i * 8); // little endian
            }
            return (short)value;
        }
    }
}