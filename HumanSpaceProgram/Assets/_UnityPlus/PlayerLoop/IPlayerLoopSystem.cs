namespace UnityPlus.PlayerLoop
{
    /// <summary>
    /// Custom player loops implementing this interface will be invoked  by the player loop at the appropriate time, <br/>
    /// as determined by the attached <see cref="PlayerLoopSystemAttribute"/>. <br/>
    /// </summary>
    /// <remarks>
    /// The class/struct implementing this must include a public parameterless constructor, and be decorated with a [PlayerLoopSystem]. <br/>
    /// </remarks>
    public interface IPlayerLoopSystem
    {
        /// <summary>
        /// Main callback method, called once per system invocation (usually once per frame, but can vary depending on which parent system it is registered to).
        /// </summary>
        void Run();
    }
}