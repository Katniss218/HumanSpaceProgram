using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using UnityEngine;

namespace UnityPlus.Logging
{
    [DisallowMultipleComponent]
    public sealed class LogFileOutput : MonoBehaviour
    {
        [field: SerializeField]
        public string LogFilePath { get; set; } = "log.txt";

        private StreamWriter _output = null;

        private void Initialize()
        {
            _output = File.CreateText( Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), LogFilePath ) );

            _output.WriteLine( "Operating System: " + SystemInfo.operatingSystem );
            _output.WriteLine( $"CPU: {SystemInfo.processorType} ({(float)SystemInfo.processorFrequency / 1000f} GHz)" );
            _output.WriteLine( $"Memory: {(float)SystemInfo.systemMemorySize / 1024f} GB" );
            _output.WriteLine( $"GPU: {SystemInfo.graphicsDeviceName} ({(float)SystemInfo.graphicsMemorySize / 1024f} GB)" );
            _output.WriteLine( "SIMD: " + (Vector.IsHardwareAccelerated ? $"Yes ({Vector<byte>.Count * 8} bytes)" : "No") );
            _output.WriteLine( "\n-- Log Starts Here -------------------------------------------------------\n" );
        }

        /// <summary>
        /// Prints a string out to the console.
        /// </summary>
        public void Print( string message )
        {
            if( _output == null )
                Initialize();

            _output.Write( message );
            _output.Flush();
        }

        /// <summary>
        /// Prints a string terminated with a newline character.
        /// </summary>
        public void PrintLine( string message )
        {
            if( _output == null )
                Initialize();

            _output.Write( $"{message}\n" );
            _output.Flush();
        }

        private void HandleLog( string message, string stackTrace, LogType logType )
        {
            switch( logType )
            {
                default:
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