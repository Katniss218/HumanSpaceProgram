using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
   /*public static class PersistsAutomatic
    {
        private struct PersistentField
        {
            public FieldInfo f;

            public string serializedName;

            public PersistentField( FieldInfo f, string serializedName )
            {
                this.f = f;
                this.serializedName = serializedName;
            }
        }

        private struct PersistentProperty
        {
            public PropertyInfo p;

            public string serializedName;

#warning actually, the type itself should decide whether or not to serialize itself as immutable. Not the property. And whether or not to serialize data or objects or both also.

            // auto should probably call both obj and data on every field anyway right?

            // so automatic doesn't care about that, unless we want to add another attribute for the entire thing. automatic doesn't serialize immutables.

            public PersistentProperty( PropertyInfo p, string serializedName )
            {
                this.p = p;
                this.serializedName = serializedName;
            }
        }

        private static readonly Dictionary<Type, (PersistentField[] fields, PersistentProperty[] properties)> _persistObjectsMembers = new();
        private static readonly Dictionary<Type, (PersistentField[] fields, PersistentProperty[] properties)> _persistDataMembers = new();

        private static ((PersistentField[] fields, PersistentProperty[] properties) refT, (PersistentField[] fields, PersistentProperty[] properties) dataT) CacheType( Type type )
        {
            List<PersistentField> finalDataFields = new();
            List<PersistentField> finalReferenceFields = new();
            List<PersistentProperty> finalDataProperties = new();
            List<PersistentProperty> finalReferenceProperties = new();

            FieldInfo[] fields = type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            foreach( var field in fields )
            {
                PersistObjectAttribute objAttr = field.GetCustomAttribute<PersistObjectAttribute>();
                if( objAttr != null )
                {
                    finalReferenceFields.Add( new PersistentField( field, objAttr.SerializedName, objAttr.AsImmutable ) );
                }

                PersistDataAttribute dataAttr = field.GetCustomAttribute<PersistDataAttribute>();
                if( dataAttr != null )
                {
                    finalDataFields.Add( new PersistentField( field, dataAttr.SerializedName, dataAttr.AsImmutable ) );
                }
            }

            PropertyInfo[] properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            foreach( var property in properties )
            {
                PersistObjectAttribute objAttr = property.GetCustomAttribute<PersistObjectAttribute>();
                if( objAttr != null )
                {
                    finalReferenceProperties.Add( new PersistentProperty( property, objAttr.SerializedName, objAttr.AsImmutable ) );
                }

                PersistDataAttribute dataAttr = property.GetCustomAttribute<PersistDataAttribute>();
                if( dataAttr != null )
                {
                    finalDataProperties.Add( new PersistentProperty( property, dataAttr.SerializedName, dataAttr.AsImmutable ) );
                }
            }

            var objectsTuple = (finalReferenceFields.ToArray(), finalReferenceProperties.ToArray());
            var dataTuple = (finalDataFields.ToArray(), finalDataProperties.ToArray());

            _persistObjectsMembers[type] = objectsTuple;
            _persistDataMembers[type] = dataTuple;
            return (objectsTuple, dataTuple);
        }

        public static void SetObjects( object obj, Type objType, SerializedObject data, IForwardReferenceMap l )
        {
            if( !_persistObjectsMembers.TryGetValue( objType, out var array ) )
            {
                array = CacheType( objType ).refT;
            }

            foreach( var field in array.fields )
            {
                if( data.TryGetValue<SerializedObject>( field.serializedName, out var fieldData ) )
                {
                    if( field.asImmutable )
                    {
                        // AsImmutable means that we want to serialize the object and its data and assign the data at creation time.
                        object fieldValue = ObjectFactory.CreateImmutable( fieldData, l );

                        field.f.SetValue( obj, fieldValue );
                    }
                    else
                    {
                        object fieldValue = ObjectFactory.Create( fieldData, l );

                        fieldValue.SetObjects( fieldData, l );
                        field.f.SetValue( obj, fieldValue );
                    }
                }
            }
            foreach( var property in array.properties )
            {
                if( data.TryGetValue<SerializedObject>( property.serializedName, out var propertyData ) )
                {
                    if( property.asImmutable )
                    {
                        // AsImmutable means that we want to serialize the object and its data and assign the data at creation time.
                        object fieldValue = ObjectFactory.CreateImmutable( propertyData, l );

                        property.p.SetValue( obj, fieldValue );
                    }
                    else
                    {
                        object fieldValue = ObjectFactory.Create( propertyData, l );

                        fieldValue.SetObjects( propertyData, l );
                        property.p.SetValue( obj, fieldValue );
                    }
                }
            }
        }

        public static SerializedObject GetObjects( object obj, Type objType, IReverseReferenceMap s )
        {
            if( !_persistObjectsMembers.TryGetValue( objType, out var array ) )
            {
                array = CacheType( objType ).refT;
            }

            SerializedObject data = new SerializedObject();

            foreach( var field in array.fields )
            {
#warning TODO - immutable needs separate extension methods that will handle adding the extra data.
                object fieldValue = field.f.GetValue( obj );

                SerializedObject so = fieldValue.GetObjects( s );
                data.Add( field.serializedName, so );
            }
            foreach( var property in array.properties )
            {
                object propertyValue = property.p.GetValue( obj );

                SerializedObject so = propertyValue.GetObjects( s );
                data.Add( property.serializedName, so );
            }
            return data;
        }


        public static SerializedData GetData( object obj, Type objType, IReverseReferenceMap s )
        {
            if( !_persistDataMembers.TryGetValue( objType, out var array ) )
            {
                array = CacheType( objType ).dataT;
            }

            SerializedObject rootSO = new SerializedObject();

            foreach( var field in array.fields )
            {
                object fieldValue = field.f.GetValue( obj );

                SerializedData so = fieldValue.GetData( s );
                rootSO.Add( field.serializedName, so );
            }
            foreach( var property in array.properties )
            {
                object propertyValue = property.p.GetValue( obj );

                SerializedData so = propertyValue.GetData( s );
                rootSO.Add( property.serializedName, so );
            }
            return rootSO;
        }


        public static void SetData( object obj, Type objType, IForwardReferenceMap l, SerializedData data )
        {
            if( !_persistDataMembers.TryGetValue( objType, out var array ) )
            {
                array = CacheType( objType ).dataT;
            }

            foreach( var field in array.fields )
            {
                if( data.TryGetValue( field.serializedName, out var fieldData ) )
                {
                    if( field.asImmutable )
                    {
                        // AsImmutable means that we want to serialize the object and its data and assign the data at creation time.
                        object fieldValue = ObjectFactory.CreateImmutable( (SerializedObject)fieldData, l );

                        field.f.SetValue( obj, fieldValue );
                    }
                    else
                    {
                        object fieldValue = field.f.GetValue( obj );
                        if( fieldValue != null )
                        {
                            fieldValue.SetData( l, fieldData );
                        }
                    }
                }
            }
            foreach( var property in array.properties )
            {
                if( data.TryGetValue( property.serializedName, out var propertyData ) )
                {
                    if( property.asImmutable )
                    {
                        // AsImmutable means that we want to serialize the object and its data and assign the data at creation time.
                        object fieldValue = ObjectFactory.CreateImmutable( (SerializedObject)propertyData, l );

                        property.p.SetValue( obj, fieldValue );
                    }
                    else
                    {
                        object propertyValue = property.p.GetValue( obj );
                        if( propertyValue != null )
                        {
                            propertyValue.SetData( l, propertyData );
                        }
                    }
                }
            }
        }
    }*/
}