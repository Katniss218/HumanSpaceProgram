using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Components
{
	public interface IThruster
	{
		/// <summary>
		/// The actual thrust produced by the thruster at this moment in time, in [N].
		/// </summary>
		float Thrust { get; }
		
		/// <summary>
		/// Defines which way the engine thrusts (thrust is applied in its `forward` (Z+) direction).
		/// </summary>
		Transform ThrustTransform { get; }
	}
}