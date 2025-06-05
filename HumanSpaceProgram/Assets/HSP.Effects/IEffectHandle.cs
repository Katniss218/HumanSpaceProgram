namespace HSP.Effects
{
    public enum ObjectPoolItemState
    {
        Ready,
        Playing,
        Finished
    }

    public interface IEffectHandle
    {
        public ObjectPoolItemState State { get; }

        public void EnsureValid();
        public bool IsValid();

        public void Play();
        public bool TryPlay();

        public void Stop();
        public bool TryStop();
    }
}