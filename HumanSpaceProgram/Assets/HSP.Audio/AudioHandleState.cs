
namespace HSP.Audio
{
    /// <summary>
    /// All states that an audio handle can be in.
    /// </summary>
    public enum AudioHandleState
    {
        /// <summary>
        /// The audio is ready to play, but has not started yet.
        /// </summary>
        Ready,
        /// <summary>
        /// The audio is currently playing.
        /// </summary>
        Playing,
        /// <summary>
        /// The audio has stopped playing and can be reclaimed.
        /// </summary>
        Finished
    }
}