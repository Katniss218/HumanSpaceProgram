using HSP.Components;
using HSP.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;

namespace HSP
{
    public static class IGhostable_Ex
    {

        [MapsInheritingFrom( typeof( Renderer ), Context = GhostableContext.Ghost )]
        public static SerializationMapping RendererMapping()
        {
            return new MemberwiseSerializationMapping<Renderer>()
            {
                ("shared_materials", new Member<Renderer, Material[]>( ArrayContext.Assets,
                    o => o.sharedMaterials.Select( m => AssetRegistry.Get<Material>( "builtin::Resources/Materials/ghost_wireframe" ) ).ToArray(),
                   (o, value) => o.sharedMaterials = value )),
            };
        }

        [MapsInheritingFrom( typeof( Collider ), Context = GhostableContext.Ghost )]
        public static SerializationMapping ColliderMapping()
        {
            return new MemberwiseSerializationMapping<Collider>()
            {
                ("is_trigger", new Member<Collider, bool>( o => true, (o, value) => o.isTrigger = value ))
            };
        }

        [MapsInheritingFrom( typeof( FPointMass ), Context = GhostableContext.Ghost )]
        public static SerializationMapping FPointMassMapping()
        {
            return new MemberwiseSerializationMapping<FPointMass>()
            {
                ("mass", new Member<FPointMass, float>( o => 0.0f, (o, value) => o.Mass = value ))
            };
        }
    }
}