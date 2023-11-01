using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    /// <summary>
    /// Inherit from this class if you're making a manager component you wish to serialize the data of.
    /// </summary>
    /// <remarks>
    /// Usage: 'CelestialBodyManager : HSPManager<![CDATA[<]]>CelestialBodyManager<![CDATA[>]]>'
    /// </remarks>
    [RequireComponent( typeof( PreexistingReference ) )]
    public abstract class HSPManager : MonoBehaviour
    {

    }
}