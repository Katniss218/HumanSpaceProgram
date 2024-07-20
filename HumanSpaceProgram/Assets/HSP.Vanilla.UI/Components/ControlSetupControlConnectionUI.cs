using HSP.UI;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    /// <summary>
    /// UI for connections between two <see cref="ControlSetupControlUI"/>s.
    /// </summary>
    public class ControlSetupControlConnectionUI : MonoBehaviour
    {
        public const float THICKNESS = 3f;
        public const float OPEN_ENDED_OFFSET = 40f;

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

        private UILineRenderer _lineRenderer;
        private UIControlSetupWindow _window;

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
        /// For open-ended connections, this is the offset of the open end relative to the closed end.
        /// </summary>
        public Vector2 EndOffset { get; private set; }

        public void Destroy()
        {
            Destroy( this.gameObject );
        }

        internal void RecalculateEndPositions()
        {
            if( IsOpenEnded )
            {
                ControlSetupControlUI closedEndpoint = GetClosedEnd();

                _lineRenderer.Points = new[]
                {
                    closedEndpoint.Circle.TransformPointTo( closedEndpoint.Circle.GetLocalCenter(), (RectTransform)_window.ConnectionContainer.transform ),
                    closedEndpoint.Circle.TransformPointTo( closedEndpoint.Circle.GetLocalCenter(), (RectTransform)_window.ConnectionContainer.transform ) + EndOffset,
                };
            }
            else
            {
                _lineRenderer.Points = new[]
                {
                    Input.Circle.TransformPointTo( Input.Circle.GetLocalCenter(), (RectTransform)_window.ConnectionContainer.transform ),
                    Output.Circle.TransformPointTo( Output.Circle.GetLocalCenter(), (RectTransform)_window.ConnectionContainer.transform ),
                };
            }
        }

        internal static ControlSetupControlConnectionUI Create( UIControlSetupWindow window, ControlSetupControlUI input, ControlSetupControlUI output )
        {
            if( output == null || input == null )
                throw new ArgumentException( $"Both input and output must be set for a non-open-ended connection." );

            return Internal_Create( window, input, output, Vector2.zero );
        }

        internal static ControlSetupControlConnectionUI CreateOpenEnded( UIControlSetupWindow window, ControlSetupControlUI input, ControlSetupControlUI output, Vector2 offset )
        {
            if( output != null && input != null )
                throw new ArgumentException( $"Either output or input must be null. Specify which end is open-ended." );
            if( output == null && input == null )
                throw new ArgumentException( $"Either output or input must be non-null. Specify which end is open-ended." );

            return Internal_Create( window, input, output, offset );
        }

        private static ControlSetupControlConnectionUI Internal_Create( UIControlSetupWindow window, ControlSetupControlUI input, ControlSetupControlUI output, Vector2 offset )
        {
            UIPanel connectionPanel = window.ConnectionContainer.AddPanel( new UILayoutInfo( UIFill.Fill() ), null );

            DestroyImmediate( connectionPanel.gameObject.GetComponent<Image>() );
            UILineRenderer lineRenderer = connectionPanel.gameObject.AddComponent<UILineRenderer>();
            lineRenderer.raycastTarget = false;
            lineRenderer.Thickness = THICKNESS;

            ControlSetupControlConnectionUI connection = connectionPanel.gameObject.AddComponent<ControlSetupControlConnectionUI>();
            connection._lineRenderer = lineRenderer;
            connection._window = window;
            connection.Output = output;
            connection.Input = input;
            connection.EndOffset = offset;

            connection.RecalculateEndPositions();

            return connection;
        }
    }
}