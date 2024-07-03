namespace UnityPlus.Serialization
{
    /// <summary>
    /// A method that returns a TMember that is a member of TSource.
    /// </summary>
    public delegate TMember Getter<TSource, TMember>( TSource item );

    /// <summary>
    /// A method that sets a TMember that is a member of TSource.
    /// </summary>
    public delegate void Setter<TSource, TMember>( TSource item, TMember member );

    /// <summary>
    /// A method that sets a TMember that is a member of TSource.
    /// </summary>
    public delegate void RefSetter<TSource, TMember>( ref TSource item, TMember member );

    //
    //
    //

    public delegate void LoadAction( ref object obj, SerializedData data, ILoader l );
    public delegate void LoadReferencesAction( ref object obj, SerializedData data, ILoader l );

    public delegate void LoadAction<TSource>( ref TSource obj, SerializedData data, ILoader l );
    public delegate void LoadReferencesAction<TSource>( ref TSource obj, SerializedData data, ILoader l );
}