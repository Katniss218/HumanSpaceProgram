﻿using KSS.Control;
using KSS.Control.Controls;
using KSS.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Input;

namespace KSS.Components
{
	/// <summary>
	/// Sends steering and throttle signals based on player input.
	/// </summary>
	public class FPlayerInputAvionics : MonoBehaviour
	{
		// avionics will be a complicated and very broad system with many different subsystems, subcomponents, etc.

		// a control part (e.g. probe core) will have multiple control components (modules). One of those will almost always be a sequencer.




		// avionics will have internal channels that activate based on "things"

		private float _pitchSignal; // channel representing the pitch control signal.
		private float _yawSignal; // channel representing the yaw control signal.
		private float _rollSignal; // channel representing the roll control signal.
		private float _throttleSignal;

		[field: SerializeField]
		public Vector3 AttitudeSensitivity { get; set; } = Vector3.one;
		[field: SerializeField]
		public Vector3 TranslationSensitivity { get; set; } = Vector3.one;

		/// <summary>
		/// Desired throttle level, in [0..1].
		/// </summary>
		[NamedControl( "Throttle" )]
		public ControllerOutput<float> OnSetThrottle = new();

		/// <summary>
		/// Desired vessel-space (pitch, yaw, roll) attitude change, in [-Inf..Inf].
		/// </summary>
		[NamedControl( "Attitude" )]
		public ControllerOutput<Vector3> OnSetAttitude = new();

		/// <summary>
		/// Desired scene-space (X, Y, Z) position change, in [-Inf..Inf].
		/// </summary>
		[NamedControl( "Translation" )]
		public ControllerOutput<Vector3> OnSetTranslation = new();

		void OnEnable()
		{
			HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH, HierarchicalInputPriority.MEDIUM, Input_Pitch );
			HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW, HierarchicalInputPriority.MEDIUM, Input_Yaw );
			HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL, HierarchicalInputPriority.MEDIUM, Input_Roll );

			HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, HierarchicalInputPriority.MEDIUM, Input_FullThrottle );
			HierarchicalInputManager.AddAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, HierarchicalInputPriority.MEDIUM, Input_CutThrottle );
		}

		void OnDisable()
		{
			HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH, Input_Pitch );
			HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW, Input_Yaw );
			HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL, Input_Roll );

			HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, Input_FullThrottle );
			HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, Input_CutThrottle );
		}

		private bool Input_FullThrottle( float value )
		{
			_throttleSignal = 1.0f;

			OnSetThrottle.TrySendSignal( _throttleSignal );
			return false;
		}

		private bool Input_CutThrottle( float value )
		{
			_throttleSignal = 0.0f;

			OnSetThrottle.TrySendSignal( _throttleSignal );
			return false;
		}

		bool Input_Pitch( float value )
		{
			_pitchSignal = value * AttitudeSensitivity.x;

			OnSetAttitude.TrySendSignal( new Vector3( _pitchSignal, _yawSignal, _rollSignal ) );
			return false;
		}

		bool Input_Yaw( float value )
		{
			_yawSignal = value * AttitudeSensitivity.y;

			OnSetAttitude.TrySendSignal( new Vector3( _pitchSignal, _yawSignal, _rollSignal ) );
			return false;
		}

		bool Input_Roll( float value )
		{
			_rollSignal = value * AttitudeSensitivity.z;

			OnSetAttitude.TrySendSignal( new Vector3( _pitchSignal, _yawSignal, _rollSignal ) );
			return false;
		}
	}
}