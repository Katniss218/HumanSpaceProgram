using System;

namespace UnityPlus.OverridableValueProviders
{
    public class OverridableValueProvider<T, TResult> : ITopologicallySortable<string>, IOverridable<string>
    {
        public string ID { get; }

        public string[] Blacklist { get; }

        public string[] Before { get; }

        public string[] After { get; }

        private readonly Func<T, TResult> _getter;

        public OverridableValueProvider( string id, Func<T, TResult> getter, string[] blacklist, string[] before, string[] after )
        {
            this.ID = id;
            this.Blacklist = blacklist;
            this.Before = before;
            this.After = after;
            this._getter = getter;
        }

        public TResult GetValue( T input )
        {
            return _getter( input );
        }
    }
}