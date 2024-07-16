using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.OverridableValueProviders
{
    public abstract class OverridableValueProvider<T, TResult> : ITopologicallySortable<string>, IOverridable<string>
    {
        public string ID { get; }

        public string[] Blacklist { get; }

        public string[] Before { get; }

        public string[] After { get; }

#warning TODO - maybe a delegate instead of inheritance?

        protected OverridableValueProvider( string id, string[] blacklist, string[] before, string[] after )
        {
            this.ID = id;
            this.Blacklist = blacklist;
            this.Before = before;
            this.After = after;
        }

        public abstract TResult GetValue( T input1 );
    }
}