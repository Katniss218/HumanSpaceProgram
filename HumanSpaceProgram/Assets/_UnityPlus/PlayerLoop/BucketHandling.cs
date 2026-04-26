namespace UnityPlus.PlayerLoop
{
    public enum BucketHandling
    {
        /// <summary>
        /// Skip systems for which the target bucket is not already instantiated. <br/>
        /// Requires target buckets to already exist in the native loop. Does not auto-hydrate the bucket paths.
        /// </summary>
        Skip,

        /// <summary>
        /// Try to instantiate the target buckets, but throw if unable.
        /// </summary>
        IncludeThrow,

        /// <summary>
        /// Try to instantiate the target buckets, but skip if unable. <br/>
        /// This is the lenient 'inclusive' option.
        /// </summary>
        IncludeSkip
    }
}