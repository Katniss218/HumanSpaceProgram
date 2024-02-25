using KSS.Core;
using KSS.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Input;

namespace KSS.GameplayScene
{
	/// <summary>
	/// Controls the keyboard input for time scale control.
	/// </summary>
	[DisallowMultipleComponent]
	public class TimeScaleInputController : MonoBehaviour
	{
		[HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".add_timescale_icontroller" )]
		private static void CreateInstanceInScene()
		{
			GameplaySceneManager.Instance.gameObject.AddComponent<TimeScaleInputController>();
		}

		void OnEnable()
		{
			HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_TIMESCALE_INCREASE, HierarchicalInputPriority.MEDIUM, Input_TimescaleIncrease );
			HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_TIMESCALE_DECREASE, HierarchicalInputPriority.MEDIUM, Input_TimescaleDecrease );
		}

		void OnDisable()
		{
			HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_TIMESCALE_INCREASE, Input_TimescaleIncrease );
			HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_TIMESCALE_DECREASE, Input_TimescaleDecrease );
		}

		private bool Input_TimescaleIncrease( float value )
		{
			if( TimeManager.LockTimescale ) // short-circuit exit before checking anything.
				return false;

			if( TimeManager.IsPaused )
			{
				TimeManager.SetTimeScale( 1f );
				return false;
			}

			float newscale = TimeManager.TimeScale * 2f;
			if( newscale <= TimeManager.GetMaxTimescale() )
				TimeManager.SetTimeScale( newscale );

			return false;
		}

		private bool Input_TimescaleDecrease( float value )
		{
			if( TimeManager.LockTimescale ) // short-circuit exit before checking anything.
				return false;

			TimeManager.SetTimeScale( TimeManager.TimeScale > 1f ? TimeManager.TimeScale / 2.0f : 0.0f );

			return false;
		}
	}
}