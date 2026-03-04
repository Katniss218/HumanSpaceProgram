using System;

namespace UnityPlus.Serialization
{
    [AttributeUsage( AttributeTargets.Method )]
    public abstract class MappingProviderAttribute : Attribute
    {
        /// <summary>
        /// Specifies the interface type that this mapping will be used to map.
        /// </summary>
        public Type MappedType { get; set; }

        /// <summary>
        /// Specifies the integer context that this mapping will be used in.
        /// </summary>
        [Obsolete( "Use ContextType for new code." )]
        public int Context
        {
            get => Contexts != null && Contexts.Length > 0 ? Contexts[0] : 0;
            set => Contexts = new int[] { value };
        }

        /// <summary>
        /// Specifies a number of integer contexts that this mapping will be used in.
        /// </summary>
        [Obsolete( "Use ContextTypes for new code." )]
        public int[] Contexts { get; set; } = new int[] { 0 };

        /// <summary>
        /// Specifies the context type this mapping applies to.
        /// Supports Open Generics (e.g. typeof(Ctx.Dict<,>)).
        /// </summary>
        public Type ContextType
        {
            get => ContextTypes != null && ContextTypes.Length > 0 ? ContextTypes[0] : null;
            set => ContextTypes = value != null ? new Type[] { value } : Array.Empty<Type>();
        }

        /// <summary>
        /// Specifies a number of context types this mapping applies to.
        /// Supports Open Generics (e.g. typeof(Ctx.Dict<,>)).
        /// </summary>
        public Type[] ContextTypes { get; set; }
    }
}