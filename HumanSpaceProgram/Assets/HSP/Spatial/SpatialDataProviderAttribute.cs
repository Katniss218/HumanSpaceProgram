using System;

namespace HSP.Spatial
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    public sealed class SpatialDataProviderAttribute : Attribute
    {
        public Type DataID { get; set; } // the type of the class that contains the registries.
        public string ID { get; set; }
        public string[] Blacklist { get; set; }

        public string[] Before { get; set; }
        public string[] After { get; set; }

        public SpatialDataProviderAttribute( Type dataId, string id )
        {
            this.DataID = dataId;
            this.ID = id;
        }
    }
}