using HSP.Input;
using HSP.Time;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    /// <summary>
    /// Controls the keyboard input for time scale control.
    /// </summary>
    [DisallowMultipleComponent]
	public class TimeScaleInputController : MonoBehaviour
	{
		void OnEnable()
		{
			HierarchicalInputManager.AddAction( InputChannel.GAMEPLAY_TIMESCALE_INCREASE, InputChannelPriority.MEDIUM, Input_TimescaleIncrease );
			HierarchicalInputManager.AddAction( InputChannel.GAMEPLAY_TIMESCALE_DECREASE, InputChannelPriority.MEDIUM, Input_TimescaleDecrease );
		}

		void OnDisable()
		{
			HierarchicalInputManager.RemoveAction( InputChannel.GAMEPLAY_TIMESCALE_INCREASE, Input_TimescaleIncrease );
			HierarchicalInputManager.RemoveAction( InputChannel.GAMEPLAY_TIMESCALE_DECREASE, Input_TimescaleDecrease );
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