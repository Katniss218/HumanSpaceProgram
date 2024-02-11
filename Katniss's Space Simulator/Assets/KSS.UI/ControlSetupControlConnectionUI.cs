using KSS.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Windows;

namespace KSS.UI
{
	public class ControlSetupControlConnectionUI
	{
		/// <summary>
		/// The endpoint that the connection UI goes out of (i.e. a 'From'). <br/>
		/// May be null.
		/// </summary>
		public ControlSetupControlUI Output { get; private set; } = null;

		/// <summary>
		/// The endpoint that the connection UI goes into (i.e. a 'To'). <br/>
		/// May be null.
		/// </summary>
		public ControlSetupControlUI Input { get; private set; } = null;

		/// <summary>
		/// Describes whether or not the connnection UI has a free endpoint.
		/// </summary>
		public bool IsOpenEnded => Output == null || Input == null;

		/// <summary>
		/// Returns the closed end of an open ended connection.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the connection is not open-ended.</exception>
		public ControlSetupControlUI GetClosedEnd()
		{
			if( Output == null )
				return Input;
			if( Input == null )
				return Output;

			throw new InvalidOperationException( $"Can't get the closed end of a connection that is not open-ended. Both endpoints are set." );
		}

		/// <summary>
		/// If one of the endpoints is not set, the end offset describes the offset of the unset endpoint in reference to the set endpoint's position.
		/// </summary>
		public Vector2 EndOffset { get; private set; } // This is used for connections that connect to components that aren't shown,
													   // as well as for the connection that is being dragged out by the mouse.

		RectTransform _graphic;

		public void DestroyGraphic()
		{
			throw new NotImplementedException();
		}

		internal static ControlSetupControlConnectionUI Create( ControlSetupWindow window, ControlSetupControlUI input, ControlSetupControlUI output )
		{
			if( output == null || input == null )
			{
				throw new ArgumentException( $"Both input and output must be set for a non-open-ended connection." );
			}

			ControlSetupControlConnectionUI connection = new ControlSetupControlConnectionUI();
			connection.Output = output;
			connection.Input = input;
			connection.EndOffset = Vector2.zero;

			return connection;
		}

		internal static ControlSetupControlConnectionUI CreateOpenEnded( ControlSetupWindow window, ControlSetupControlUI input, ControlSetupControlUI output, Vector2 offset )
		{
			if( output != null && input != null )
			{
				throw new ArgumentException( $"Either output or input must be null. Specify which end is open-ended." );
			}
			if( output == null && input == null )
			{
				throw new ArgumentException( $"Either output or input must be non-null. Specify which end is open-ended." );
			}

			ControlSetupControlConnectionUI connection = new ControlSetupControlConnectionUI();
			connection.Output = output;
			connection.Input = input;
			connection.EndOffset = offset;

			return connection;
		}
	}
}