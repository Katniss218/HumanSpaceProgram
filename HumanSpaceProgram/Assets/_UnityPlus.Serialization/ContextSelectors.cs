using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Always returns the context at the specified index from a fixed array. <br/>
    /// Mostly used for encoding generic context arguments.
    /// </summary>
    public class UniformSelector : IContextSelector
    {
        public readonly ContextKey[] Contexts;

        public UniformSelector( params ContextKey[] contexts )
        {
            Contexts = contexts ?? Array.Empty<ContextKey>();
        }

        public ContextKey Select( ContextSelectionArgs args )
        {
            int len = Contexts.Length;
            if( len == 0 )
                return ContextKey.Default;
            if( len == 1 ) 
                return Contexts[0];

            return Contexts[args.Index % len];
        }
    }

    /// <summary>
    /// Selects a context based on the index of the element matching defined ranges.
    /// </summary>
    public class IndexRangeSelector : IContextSelector
    {
        private struct Rule
        {
            public int Min;
            public int Max; // Inclusive
            public ContextKey Context;
        }

        private List<Rule> _rules = new List<Rule>();
        private ContextKey _defaultContext;

        public IndexRangeSelector( ContextKey defaultContext = default )
        {
            _defaultContext = defaultContext;
        }

        public void AddRule( int min, int max, ContextKey context )
        {
            _rules.Add( new Rule { Min = min, Max = max, Context = context } );
        }

        public ContextKey Select( ContextSelectionArgs args )
        {
            foreach( var rule in _rules )
            {
                if( args.Index >= rule.Min && args.Index <= rule.Max )
                    return rule.Context;
            }
            return _defaultContext;
        }
    }

    /// <summary>
    /// Selects a context based on the Type of the object.
    /// Supports polymorphic assignment checking.
    /// </summary>
    public class TypeSelector : IContextSelector
    {
        private readonly Dictionary<Type, ContextKey> _typeMap;
        private readonly ContextKey _fallback;

        public TypeSelector( Dictionary<Type, ContextKey> typeMap, ContextKey fallback = default )
        {
            _typeMap = typeMap;
            _fallback = fallback;
        }

        public ContextKey Select( ContextSelectionArgs args )
        {
            // If ActualType is known (live object OR deserialized with $type), use it.
            // If ActualType is null (deserialized without $type), fallback to DeclaredType.

            Type targetType = args.ActualType ?? args.DeclaredType;

            if( targetType == null )
                return _fallback;

            // 1. Exact Match
            if( _typeMap.TryGetValue( targetType, out var ctx ) )
            {
                return ctx;
            }

            return _fallback;
        }
    }

    /// <summary>
    /// Selects a context by checking if the object type is assignable to specific types (Inheritance check).
    /// More expensive than TypeSelector but handles inheritance hierarchies.
    /// </summary>
    public class TypeAssignableSelector : IContextSelector
    {
        private struct Rule
        {
            public Type BaseType;
            public ContextKey Context;
        }

        private List<Rule> _rules = new List<Rule>();
        private ContextKey _defaultContext;

        public TypeAssignableSelector( ContextKey defaultContext = default )
        {
            _defaultContext = defaultContext;
        }

        public void AddRule( Type baseType, ContextKey context )
        {
            _rules.Add( new Rule { BaseType = baseType, Context = context } );
        }

        public ContextKey Select( ContextSelectionArgs args )
        {
            Type targetType = args.ActualType ?? args.DeclaredType;
            if( targetType == null ) return _defaultContext;

            foreach( var rule in _rules )
            {
                if( rule.BaseType.IsAssignableFrom( targetType ) )
                    return rule.Context;
            }

            return _defaultContext;
        }
    }

    /// <summary>
    /// Selects a context based on the string representation of the Key.
    /// Useful for Dictionaries or Objects where naming conventions dictate serialization logic (e.g. "asset_Texture").
    /// </summary>
    public class KeyPatternSelector : IContextSelector
    {
        public enum MatchMode { Prefix, Suffix, Exact, Contains }

        private struct Rule
        {
            public MatchMode Mode;
            public string Pattern;
            public ContextKey Context;
        }

        private List<Rule> _rules = new List<Rule>();
        private ContextKey _defaultContext;

        public KeyPatternSelector( ContextKey defaultContext = default )
        {
            _defaultContext = defaultContext;
        }

        public void AddRule( MatchMode mode, string pattern, ContextKey context )
        {
            _rules.Add( new Rule { Mode = mode, Pattern = pattern, Context = context } );
        }

        public ContextKey Select( ContextSelectionArgs args )
        {
            if( args.Key == null ) return _defaultContext;
            string keyStr = args.Key.ToString();

            foreach( var rule in _rules )
            {
                bool match = false;
                switch( rule.Mode )
                {
                    case MatchMode.Prefix: match = keyStr.StartsWith( rule.Pattern, StringComparison.Ordinal ); break;
                    case MatchMode.Suffix: match = keyStr.EndsWith( rule.Pattern, StringComparison.Ordinal ); break;
                    case MatchMode.Exact: match = string.Equals( keyStr, rule.Pattern, StringComparison.Ordinal ); break;
                    case MatchMode.Contains: match = keyStr.Contains( rule.Pattern ); break;
                }

                if( match ) return rule.Context;
            }

            return _defaultContext;
        }
    }

    /// <summary>
    /// Selects a context based on the size (count) of the parent collection.
    /// Useful for optimization strategies (e.g. switch to binary blob for large arrays).
    /// </summary>
    public class CountSelector : IContextSelector
    {
        private struct Rule
        {
            public int Threshold;
            public bool GreaterThan; // true = > Threshold, false = <= Threshold
            public ContextKey Context;
        }

        private List<Rule> _rules = new List<Rule>();
        private ContextKey _defaultContext;

        public CountSelector( ContextKey defaultContext = default )
        {
            _defaultContext = defaultContext;
        }

        public void AddRule( int threshold, bool greaterThan, ContextKey context )
        {
            _rules.Add( new Rule { Threshold = threshold, GreaterThan = greaterThan, Context = context } );
        }

        public ContextKey Select( ContextSelectionArgs args )
        {
            if( args.ContainerCount == -1 ) return _defaultContext;

            foreach( var rule in _rules )
            {
                if( rule.GreaterThan )
                {
                    if( args.ContainerCount > rule.Threshold ) return rule.Context;
                }
                else
                {
                    if( args.ContainerCount <= rule.Threshold ) return rule.Context;
                }
            }

            return _defaultContext;
        }
    }
}