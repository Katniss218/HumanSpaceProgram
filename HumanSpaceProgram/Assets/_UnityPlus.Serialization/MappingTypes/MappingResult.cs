namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents the state of a save/load invocation on a mapping.
    /// </summary>
    public enum MappingResult : byte
    {
        Finished = 0,
        Failed,
        Progressed,
        NoChange
    }

    public static class MappingResult_Ex
    {
        public static MappingResult GetCompoundResult( bool anyFailed, bool anyFinished, bool anyProgressed )
        {
            if( anyFailed )
            {
                if( anyFinished || anyProgressed )
                    return MappingResult.Progressed;

                return MappingResult.Failed;
            }

            if( anyFinished && !anyProgressed )
                return MappingResult.Finished;

            if( anyProgressed )
                return MappingResult.Progressed;

            return MappingResult.NoChange;
        }
    }
}