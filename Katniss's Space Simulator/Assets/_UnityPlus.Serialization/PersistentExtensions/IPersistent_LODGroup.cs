using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class IPersistent_LODGroup
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this LODGroup lg, IReverseReferenceMap s )
        {
            LOD[] lods = lg.GetLODs();
            SerializedArray lodsArray = new SerializedArray();
            foreach( var lod in lods )
            {
                SerializedArray renderersArray = new SerializedArray();
                foreach( var renderer in lod.renderers )
                {
                    renderersArray.Add( s.WriteObjectReference( renderer ) );
                }

                lodsArray.Add( new SerializedObject()
                {
                    { "percent", lod.screenRelativeTransitionHeight.GetData() },
                    { "renderers", renderersArray }
                } );
            }
            return new SerializedObject()
            {
                { "lods", lodsArray },
                { "size", lg.size.GetData() }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this LODGroup lg, SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "lods", out var lods1 ) )
            {
                SerializedArray lodsArray = (SerializedArray)lods1;
                LOD[] lods = new LOD[lodsArray.Count];
                int i = 0;
                foreach( var lod in lodsArray )
                {
                    lods[i] = new LOD();

                    if( lod.TryGetValue( "percent", out var percent ) )
                        lods[i].screenRelativeTransitionHeight = percent.AsFloat();

                    if( lod.TryGetValue( "renderers", out var renderers1 ) )
                    {
                        SerializedArray renderersArray = (SerializedArray)renderers1;
                        Renderer[] renderers = new Renderer[renderersArray.Count];
                        int j = 0;
                        foreach( var renderer in renderersArray )
                        {
                            renderers[j] = (Renderer)l.ReadObjectReference( renderer );
                            j++;
                        }
                    }
                    i++;
                }
            }

            if( data.TryGetValue( "size", out var size ) )
                lg.size = size.AsFloat();
        }
    }
}