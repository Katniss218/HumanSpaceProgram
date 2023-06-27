using KSS.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSS.Control;
using UnityPlus.Serialization;

namespace KSS.Functionalities
{
    public class FSequencer: MonoBehaviour, IPersistent
    {
        // list of sequences.

        // timed from previous or key-based. key is user-assignable.
        // the sequence can be defined as looping

        public void SetData( ILoader l, SerializedData data )
        {
            throw new NotImplementedException();
        }

        public SerializedData GetData( ISaver s )
        {
            throw new NotImplementedException();
        }
    }
}