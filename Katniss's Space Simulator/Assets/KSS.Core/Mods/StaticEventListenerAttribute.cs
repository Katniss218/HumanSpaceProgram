using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Core.Mods
{
    /// <summary>
    /// Runs a method when a specified static event is fired.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public class StaticEventListenerAttribute : Attribute
    {
        public string EventID { get; set; }
        public string ID { get; set; }
        public string[] Blacklist { get; set; }

        public StaticEventListenerAttribute( string eventId, string id )
        {
            this.EventID = eventId;
            this.ID = id;
        }
    }
}