using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP._DevUtils
{
    internal class AsyncTester : MonoBehaviour
    {
        async void Awake()
        {
            Debug.Log( "A" );
            await Task.Delay( 1000 );
            Debug.Log( "B" );
        }
        
        async void Start()
        {
            Debug.Log( "A" );
            await Task.Delay( 1000 );
            Debug.Log( "B" );
        }

        async void Update()
        {
            Debug.Log( "C" );
            await Task.Delay( 1000 );
            Debug.Log( "D" );
        }

        async void FixedUpdate()
        {
            Debug.Log( "E" );
            await Task.Delay( 1000 );
            Debug.Log( "F" );
        }
    }
}
