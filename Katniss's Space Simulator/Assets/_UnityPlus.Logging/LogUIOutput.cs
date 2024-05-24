using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.Logging
{
    /// <summary>
    /// Enables forwarding of log messages (see <see cref="Output"/>). <br />
    /// Should probably be placed in a scene that doesn't get unloaded.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LogUIOutput : MonoBehaviour
    {
        private const string LOG_COLOR_INFO = "#dddddd";
        private const string LOG_COLOR_WARN = "#dddd55";
        private const string LOG_COLOR_ERROR = "#dd5555";

        private const string LOG_COLOR_EXCEPTION = "#dd5555";
        private const string LOG_COLOR_EXCEPTION_STACK = "#c55555";

        [field: SerializeField]
        private TMPro.TextMeshProUGUI Output { get; set; } = null;

        private void Initialize()
        {
            if( Output == null )
            {
                throw new InvalidOperationException( $"Can't initialize {nameof( LogUIOutput )} - Output UI element is not specified." );
            }

            if( !Output.richText )
            {
                Debug.LogWarning( $"The output UI element ({Output}) isn't set to Rich Rext, setting to Rich Text now." );
                Output.richText = true;
            }

            Output.text = Output.text ?? ""; // This is required to fix glitch requiring reenabling the gameObject after adding some text to the output (if it's set to blank).
            Output.gameObject.SetActive( false );
        }

        /// <summary>
        /// Prints a string out to the console.
        /// </summary>
        public void Print( string message )
        {
            if( Output == null )
                Initialize();

            Output.text += message;
        }

        /// <summary>
        /// Prints a string terminated with a newline character.
        /// </summary>
        public void PrintLine( string message )
        {
            if( Output == null )
                Initialize();

            Output.text += $"{message}\n";
        }

        private void HandleLog( string message, string stackTrace, LogType logType )
        {
            switch( logType )
            {
                default:
                    PrintLine( $"<color={LOG_COLOR_INFO}>[{DateTime.Now.ToLongTimeString()}](___) - {message}</color>" );
                    break;

                case LogType.Warning:
                    PrintLine( $"<color={LOG_COLOR_WARN}>[{DateTime.Now.ToLongTimeString()}](WRN) - {message}</color>" );
                    break;

                case LogType.Error:
                    PrintLine( $"<color={LOG_COLOR_ERROR}>[{DateTime.Now.ToLongTimeString()}](ERR) - {message}</color>" );
                    Output.gameObject.SetActive( true );
                    break;

                case LogType.Exception:
                    PrintLine( $"<color={LOG_COLOR_EXCEPTION}>[{DateTime.Now.ToLongTimeString()}](EXC) - {message}</color>\n  at\n<color={LOG_COLOR_EXCEPTION_STACK}>" + stackTrace + "</color>" );
                    Output.gameObject.SetActive( true );
                    break;
            }
        }

        void OnEnable()
        {
            Application.logMessageReceived += this.HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= this.HandleLog;
        }
    }
}