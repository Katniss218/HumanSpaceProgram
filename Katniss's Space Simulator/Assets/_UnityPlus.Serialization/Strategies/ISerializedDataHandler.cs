using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Strategies
{
    public interface ISerializedDataHandler
    {
        public (SerializedData o, SerializedData d) ReadObjectsAndData();

        public void WriteObjectsAndData( SerializedData o, SerializedData d );
    }
}