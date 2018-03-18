﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Attributes;
using Reinforced.Typings.Fluent.Interfaces;

namespace Reinforced.Typings.Fluent
{
    /// <summary>
    ///     Configuration builder for type
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public abstract class TypeConfigurationBuilder<TType> : ITypeConfigurationBuilder
    {
        private readonly ICollection<TsAddTypeImportAttribute> _imports = new List<TsAddTypeImportAttribute>();

        private readonly Dictionary<MemberInfo, IAttributed<TsAttributeBase>> _membersConfiguration =
            new Dictionary<MemberInfo, IAttributed<TsAttributeBase>>();

        private readonly Dictionary<ParameterInfo, IAttributed<TsParameterAttribute>> _parametersConfiguration
            = new Dictionary<ParameterInfo, IAttributed<TsParameterAttribute>>();

        private readonly ICollection<TsAddTypeReferenceAttribute> _references = new List<TsAddTypeReferenceAttribute>();
        private readonly Dictionary<Type, RtTypeName> _substitutions = new Dictionary<Type, RtTypeName>();
        private readonly Dictionary<Type, Func<Type, TypeResolver, RtTypeName>> _genericSubstitutions = new Dictionary<Type, Func<Type, TypeResolver, RtTypeName>>();

        Type ITypeConfigurationBuilder.Type
        {
            get { return typeof(TType); }
        }


        Dictionary<Type, RtTypeName> ITypeConfigurationBuilder.Substitutions
        {
            get { return _substitutions; }
        }

        /// <summary>
        /// Substitutions to be used only when in this type
        /// </summary>
        public Dictionary<Type, Func<Type, TypeResolver, RtTypeName>> GenericSubstitutions
        {
            get { return _genericSubstitutions; }
        }

        /// <summary>
        /// Gets whether type configuration is flatten
        /// </summary>
        public abstract bool IsHierarchyFlatten { get; }

        /// <summary>
        /// Flatten limiter
        /// </summary>
        public abstract Type FlattenLimiter { get; }

        Dictionary<ParameterInfo, IAttributed<TsParameterAttribute>> ITypeConfigurationBuilder.ParametersConfiguration
        {
            get { return _parametersConfiguration; }
        }

        Dictionary<MemberInfo, IAttributed<TsAttributeBase>> ITypeConfigurationBuilder.MembersConfiguration
        {
            get { return _membersConfiguration; }
        }

        ICollection<TsAddTypeReferenceAttribute> IReferenceConfigurationBuilder.References
        {
            get { return _references; }
        }

        ICollection<TsAddTypeImportAttribute> IReferenceConfigurationBuilder.Imports
        {
            get { return _imports; }
        }

        string IReferenceConfigurationBuilder.PathToFile { get; set; }

        /// <inheritdoc />
        public abstract double MemberOrder { get; set; }
    }
}