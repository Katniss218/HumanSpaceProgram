using HSP.Core;
using HSP.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.GameplayScene
{
	/// <summary>
	/// Controls the keyboard input for time scale control.
	/// </summary>
	[DisallowMultipleComponent]
	public class TimeScaleInputController : MonoBehaviour
	{
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
			if( TimeStepManager.LockTimescale ) // short-circuit exit before checking anything.
				return false;

			if( TimeStepManager.IsPaused )
			{
				TimeStepManager.SetTimeScale( 1f );
				return false;
			}

			float newscale = TimeStepManager.TimeScale * 2f;
			if( newscale <= TimeStepManager.GetMaxTimescale() )
				TimeStepManager.SetTimeScale( newscale );

			return false;
		}

		private bool Input_TimescaleDecrease( float value )
		{
			if( TimeStepManager.LockTimescale ) // short-circuit exit before checking anything.
				return false;

			TimeStepManager.SetTimeScale( TimeStepManager.TimeScale > 1f ? TimeStepManager.TimeScale / 2.0f : 0.0f );

			return false;
		}
	}
}