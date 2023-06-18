using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Core.Mods
{
    [AttributeUsage( AttributeTargets.Method )]
    public class HumanSpaceProgramInvokeAttribute : Attribute
    {
        public enum Startup
        {
            /// <summary>
            /// The earliest possible startup time - right after the game launches, before anything else is loaded.
            /// </summary>
            Immediately
        }

        public Startup WhenToRun { get; set; }

        public HumanSpaceProgramInvokeAttribute( Startup whenToRun )
        {
            this.WhenToRun = whenToRun;
        }
    }
}