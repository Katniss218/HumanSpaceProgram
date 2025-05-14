
using System;
using UnityEngine;

namespace HSP.Audio
{
    /// <summary>
    /// Represents an audio that has been prepared to play, and/or may be currently playing.
    /// </summary>
    public interface IAudioHandle
    {
        /// <summary>
        /// The audio clip that the audio handle is currently using.
        /// </summary>
        public AudioClip Clip { get; }

        /// <summary>
        /// The state that the audio handle is currently in.
        /// </summary>
        public AudioHandleState State { get; }

        /// <summary>
        /// The current volume of the audio.
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// The current pitch of the audio.
        /// </summary>
        public float Pitch { get; set; }

        /// <summary>
        /// Whether or not the audio will loop.
        /// </summary>
        public bool Loop { get; }

        //
        //  Playback controls
        //

        /// <summary>
        /// Starts the playback immediately.
        /// </summary>
        public void Play();

        /// <summary>
        /// Stops the playback immediately.
        /// </summary>
        public void Stop();
    }
}