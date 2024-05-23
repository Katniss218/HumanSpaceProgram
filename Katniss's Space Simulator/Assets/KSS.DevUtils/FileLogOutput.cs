using KSS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityPlus.Console
{
    [DisallowMultipleComponent]
    public class FileLogOutput : MonoBehaviour
    {
        [SerializeField]
        private StreamWriter _output = null;

        [SerializeField]
        private string _defaultText = "Console:\n\n";

        /// <summary>
        /// Prints a string out to the console.
        /// </summary>
        public void Print( string message )
        {
            if( _output == null )
                _output = File.AppendText( Path.Combine( HumanSpaceProgram.GetBaseDirectoryPath(), "log.txt" ) );
            _output.Write( message );
            _output.Flush();
        }

        /// <summary>
        /// Prints a string terminated with a newline character.
        /// </summary>
        public void PrintLine( string message )
        {
            _output.Write( $"{message}\n" );
            _output.Flush();
        }

        private void HandleLog( string message, string stackTrace, LogType logType )
        {
            switch( logType )
            {
                case LogType.Log:
                    PrintLine( $"[{DateTime.Now.ToLongTimeString()}](___) - {message}" );
                    break;

                case LogType.Warning:
                    PrintLine( $"[{DateTime.Now.ToLongTimeString()}](WRN) - {message}" );
                    break;

                case LogType.Error:
                    PrintLine( $"[{DateTime.Now.ToLongTimeString()}](ERR) - {message}" );
                    break;

                case LogType.Exception:
                    PrintLine( $"[{DateTime.Now.ToLongTimeString()}](EXC) - {message}\n  at\n" + stackTrace + "" );
                    break;
            }
        }

        void Awake()
        {
            _output = File.CreateText( Path.Combine( HumanSpaceProgram.GetBaseDirectoryPath(), "log.txt" ) );
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