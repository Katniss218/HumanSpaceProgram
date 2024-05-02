using KSS.Components;
using KSS.Control;
using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
	/// <summary>
	/// Represents an invokable element of a <see cref="FSequencer"/>'s sequence.
	/// </summary>
	public abstract class SequenceElement : ControlGroup, IPersistsObjects, IPersistsData
	{
		[NamedControl( "Actions", "", Editable = false )]
		/// <summary>
		/// The actions that this sequence element will call when it's fired.
		/// </summary>
		public List<SequenceActionBase> Actions = new();

		/// <summary>
		/// Called when the previous action is triggerred, or on load (the first element).
		/// </summary>
		public abstract void Initialize();

		/// <summary>
		/// Called to check if the sequence element can fire in this frame.
		/// </summary>
		public abstract bool CanInvoke();

		/// <summary>
		/// Called to fire the sequence element.
		/// </summary>
		public void Invoke()
		{
			foreach( var action in Actions )
			{
				action.TryInvoke();
			}
		}

		public virtual SerializedObject GetObjects( IReverseReferenceMap s )
        {
#warning TODO - needs a common method to create an object stub.
            SerializedArray array = new SerializedArray( Actions.Select( a => a.GetObjects( s ) ) );

			return new SerializedObject()
			{
				{ "actions", array }
			};
		}

		public virtual void SetObjects( SerializedObject data, IForwardReferenceMap l )
		{
			if( data.TryGetValue<SerializedArray>( "actions", out var actions ) )
			{
				Actions = new();
				foreach( var act in actions.Cast<SerializedObject>() )
				{
#warning TODO - needs a common method to save an instance of a specific subtype.
					SequenceActionBase action = act.ToSequenceAction( l );

					action.SetObjects( act, l );

					Actions.Add( action );
				}
			}

			/* Actions = new List<SequenceActionBase>()
             {
                 new SequenceAction<float>(),
                 new SequenceAction<Vector3>()
             };

             Actions[0].SetObjects( null, l );
             Actions[1].SetObjects( null, l );*/
		}

		public virtual SerializedData GetData( IReverseReferenceMap s )
		{
			return null;
		}

		public virtual void SetData( SerializedData data, IForwardReferenceMap l )
		{

		}
	}

	public class KeyboardSequenceElement : SequenceElement
	{
		public KeyCode Key { get; set; } = KeyCode.Space;

		public override void Initialize()
		{
		}

		public override bool CanInvoke()
		{
			return UnityEngine.Input.GetKey( Key );
		}

		public override SerializedData GetData( IReverseReferenceMap s )
		{
			return new SerializedObject()
			{
				{ "key", this.Key.GetData() }
			};
		}

		public override void SetData( SerializedData data, IForwardReferenceMap l )
		{
			if( data.TryGetValue( "key", out var key ) )
				Key = key.ToKeyCode();
		}
	}

	public class TimedSequenceElement : SequenceElement
	{
		/// <summary>
		/// The delay, in [s], from the firing of the previous element, after which the sequence element should fire.
		/// </summary>
		public float Delay { get; set; }

		private double _startUT;

		public override void Initialize()
		{
			_startUT = TimeStepManager.UT;
		}

		public override bool CanInvoke()
		{
			return TimeStepManager.UT >= _startUT + Delay;
		}

		public override SerializedData GetData( IReverseReferenceMap s )
		{
			return new SerializedObject()
			{
				{ "delay", this.Delay.GetData() },
				{ "start_ut", this._startUT.GetData() }
			};
		}

		public override void SetData( SerializedData data, IForwardReferenceMap l )
		{
			if( data.TryGetValue( "delay", out var delay ) )
				Delay = delay.ToFloat();

			if( data.TryGetValue( "start_ut", out var startUt ) )
				_startUT = startUt.ToDouble();
		}
	}
}