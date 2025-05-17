
namespace HSP.Effects
{
    public interface IValueGetter<T>
    {
        T Get();
    }

    /// <summary>
    /// Value Getter that has an initialize callback. <br/><br/>
    /// 
    /// NOTE TO IMPLEMENTERS: This should be used on top of <see cref="IValueGetter{T}"/>. Using this interface standalone makes no sense. <br/>
    /// But due to limitations of the language and type casting, I can't derive this non-generic interface from it.
    /// </summary>
    public interface IInitValueGetter<TDriven>
    {
        void OnInit( TDriven handle );
    }
}