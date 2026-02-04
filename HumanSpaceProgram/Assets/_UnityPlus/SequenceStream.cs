using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityPlus
{
    /// <summary>
    /// A Unity-compatible .NET Standard 2.1 stream that sequences multiple streams.
    /// Optimized for low GC allocation using Span/Memory.
    /// </summary>
    public class SequenceStream : Stream
    {
        private readonly Stream[] _streams;
        private readonly bool _leaveOpen;
        private int _currentStreamIndex;
        private long _position;
        private bool _isDisposed;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        /// <summary>
        /// Tracks the total bytes read across all sequences. 
        /// Useful for displaying download progress in Unity UI.
        /// </summary>
        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public SequenceStream( params Stream[] streams ) : this( (IEnumerable<Stream>)streams ) { }

        public SequenceStream( IEnumerable<Stream> streams, bool leaveOpen = false )
        {
            if( streams == null )
                throw new ArgumentNullException( nameof( streams ) );

            // Materialize to array to avoid enumeration issues during read
            _streams = streams.Where( s => s != null ).ToArray();
            _leaveOpen = leaveOpen;
            _currentStreamIndex = 0;
            _position = 0;
        }

        // ---------------------------------------------------------
        // Modern .NET Standard 2.1 / Unity Optimization (Span<T>)
        // ---------------------------------------------------------

        public override int Read( Span<byte> buffer )
        {
            while( _currentStreamIndex < _streams.Length )
            {
                // Read from the current stream into the span
                int bytesRead = _streams[_currentStreamIndex].Read( buffer );

                if( bytesRead > 0 )
                {
                    _position += bytesRead;
                    // Return immediately. Do not loop to fill buffer.
                    // This prevents blocking Unity's main thread if the underlying stream is slow.
                    return bytesRead;
                }

                // Current stream exhausted, move to next
                _currentStreamIndex++;
            }

            return 0; // EOF
        }

        public override async ValueTask<int> ReadAsync( Memory<byte> buffer, CancellationToken cancellationToken = default )
        {
            while( _currentStreamIndex < _streams.Length )
            {
                // Use ValueTask to reduce heap allocations during async operations
                int bytesRead = await _streams[_currentStreamIndex]
                    .ReadAsync( buffer, cancellationToken )
                    .ConfigureAwait( false );

                if( bytesRead > 0 )
                {
                    _position += bytesRead;
                    return bytesRead;
                }

                _currentStreamIndex++;
            }

            return 0;
        }

        // ---------------------------------------------------------
        // Legacy Array Overrides (Required by Stream)
        // ---------------------------------------------------------

        public override int Read( byte[] buffer, int offset, int count )
        {
            ValidateBufferArgs( buffer, offset, count );
            // Forward to the efficient Span implementation
            return Read( new Span<byte>( buffer, offset, count ) );
        }

        public override Task<int> ReadAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            ValidateBufferArgs( buffer, offset, count );
            // Forward to the efficient Memory implementation
            return ReadAsync( new Memory<byte>( buffer, offset, count ), cancellationToken ).AsTask();
        }

        // ---------------------------------------------------------
        // Housekeeping
        // ---------------------------------------------------------

        public override void Flush()
        {
            if( _currentStreamIndex < _streams.Length && !_isDisposed )
            {
                _streams[_currentStreamIndex].Flush();
            }
        }

        protected override void Dispose( bool disposing )
        {
            if( !_isDisposed )
            {
                if( disposing && !_leaveOpen )
                {
                    foreach( var stream in _streams )
                    {
                        stream?.Dispose();
                    }
                }
                _isDisposed = true;
            }
            base.Dispose( disposing );
        }

        private static void ValidateBufferArgs( byte[] buffer, int offset, int count )
        {
            // .NET Standard 2.1 does not have Stream.ValidateBufferArguments, so we check manually
            if( buffer == null )
                throw new ArgumentNullException( nameof( buffer ) );
            if( offset < 0 ) 
                throw new ArgumentOutOfRangeException( nameof( offset ) );
            if( count < 0 ) 
                throw new ArgumentOutOfRangeException( nameof( count ) );
            if( buffer.Length - offset < count )
                throw new ArgumentException( "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection." );
        }

        // ---------------------------------------------------------
        // Unsupported Operations
        // ---------------------------------------------------------

        public override long Seek( long offset, SeekOrigin origin ) => throw new NotSupportedException( "SequenceStream does not support seeking." );
        public override void SetLength( long value ) => throw new NotSupportedException( "SequenceStream is read-only." );
        public override void Write( byte[] buffer, int offset, int count ) => throw new NotSupportedException( "SequenceStream is read-only." );
    }
}