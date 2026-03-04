namespace UnityPlus.Serialization
{
    public enum MemberResolutionResult
    {
        Resolved,
        RequiresPush,
        Deferred,
        Failed,
        /// <summary>
        /// Member missing from data, skip when deserializing.
        /// </summary>
        Missing
    }
}