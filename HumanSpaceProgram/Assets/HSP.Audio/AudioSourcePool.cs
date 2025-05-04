using System.Collections.Generic;
using UnityEngine;

namespace HSP.Audio
{
    public class AudioSourcePool
    {
        private List<AudioSourcePoolItem> _poolItems = new();

        private GameObject _poolParent;

        /// <summary>
        /// Plays a new audio that will follow the given transform until it ends playing
        /// </summary>
        public IAudioHandle GetItem( AudioClip clip, Transform transform, bool loop, AudioChannel channel, float volume = 1.0f, float pitch = 1.0f )
        {
            // Try to reuse an existing pool element first.
            foreach( var poolItem in this._poolItems )
            {
                if( poolItem.State == AudioHandleState.Finished )
                {
                    poolItem.SetAudioData( transform, clip, loop, channel, volume, pitch );
                    return poolItem;
                }
            }

            // No free elements in the pool, create a new one.
            var newPoolItem = CreatePoolItem();
            newPoolItem.SetAudioData( transform, clip, loop, channel, volume, pitch );

            this._poolItems.Add( newPoolItem );
            return newPoolItem;
        }


        private AudioSourcePoolItem CreatePoolItem()
        {
            GameObject gameObject = new GameObject( "AudioSourcePoolItem" );

            if( _poolParent == null )
            {
                _poolParent = new GameObject( "AudioSourcePool pool parent" );
            }
            gameObject.transform.SetParent( _poolParent.transform, false );

            AudioSourcePoolItem poolItem = gameObject.AddComponent<AudioSourcePoolItem>();

            return poolItem;
        }
    }
}