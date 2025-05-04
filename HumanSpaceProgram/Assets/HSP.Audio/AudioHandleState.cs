
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
        /// The audio is scheduled to play after the initial delay.
        /// </summary>
        PlayingDelay,
        /// <summary>
        /// The audio is currently plaing, and is fading in.
        /// </summary>
        PlayingFade,
        /// <summary>
        /// The audio is currently playing.
        /// </summary>
        Playing,
        /// <summary>
        /// The audio is scheduled to stop or fade out after the delay.
        /// </summary>
        FinishedDelay,
        /// <summary>
        /// The audio is currently fading out.
        /// </summary>
        FinishedFade,
        /// <summary>
        /// The audio has stopped playing and can be reclaimed.
        /// </summary>
        Finished
    }
}