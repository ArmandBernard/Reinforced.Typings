﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Reinforced.Typings.Ast.Dependency;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Attributes;
using Reinforced.Typings.Fluent.Interfaces;

namespace Reinforced.Typings.Fluent
{
    /// <summary>
    ///     Fluent configuration builder
    /// </summary>
    public class ConfigurationBuilder
    {
        private readonly List<string> _additionalDocumentationPathes = new List<string>();

        private readonly Dictionary<Type, IEnumConfigurationBuidler> _enumConfigurationBuilders =
            new Dictionary<Type, IEnumConfigurationBuidler>();

        private readonly Dictionary<Type, RtTypeName> _globalSubstitutions = new Dictionary<Type, RtTypeName>();
        private readonly Dictionary<Type, Func<Type,TypeResolver,RtTypeName>> _genericSubstitutions = new Dictionary<Type, Func<Type, TypeResolver, RtTypeName>>();
        private readonly List<RtImport> _imports = new List<RtImport>();
        private readonly List<RtReference> _references = new List<RtReference>();

        private readonly Dictionary<Type, ITypeConfigurationBuilder> _typeConfigurationBuilders =
            new Dictionary<Type, ITypeConfigurationBuilder>();

        internal ConfigurationBuilder(GlobalParameters global)
        {
            GlobalBuilder = new GlobalConfigurationBuilder(global);
        }

        internal List<string> AdditionalDocumentationPathes
        {
            get { return _additionalDocumentationPathes; }
        }

        internal List<RtReference> References
        {
            get { return _references; }
        }

        internal List<RtImport> Imports
        {
            get { return _imports; }
        }

        internal Dictionary<Type, ITypeConfigurationBuilder> TypeConfigurationBuilders
        {
            get { return _typeConfigurationBuilders; }
        }

        internal Dictionary<Type, IEnumConfigurationBuidler> EnumConfigurationBuilders
        {
            get { return _enumConfigurationBuilders; }
        }

        internal Dictionary<Type, RtTypeName> GlobalSubstitutions
        {
            get { return _globalSubstitutions; }
        }

        internal Dictionary<Type, Func<Type, TypeResolver, RtTypeName>> GenericSubstitutions
        {
            get { return _genericSubstitutions; }
        }

        internal GlobalConfigurationBuilder GlobalBuilder { get; private set; }

        internal ConfigurationRepository Build()
        {
            var repository = new ConfigurationRepository();
            foreach (var globalSubstitution in GlobalSubstitutions)
            {
                repository.GlobalSubstitutions[globalSubstitution.Key] = globalSubstitution.Value;
            }

            foreach (var globalGenSubstitution in GenericSubstitutions)
            {
                repository.GlobalGenericSubstitutions[globalGenSubstitution.Key] = globalGenSubstitution.Value;
            }
            foreach (var kv in _typeConfigurationBuilders)
            {
                if (kv.Value.GenericSubstitutions.Count > 0)
                    repository.TypeGenericSubstitutions[kv.Key] = kv.Value.GenericSubstitutions;

                if (kv.Value.Substitutions.Count > 0)
                    repository.TypeSubstitutions[kv.Key] = kv.Value.Substitutions;

                var cls = kv.Value as IClassConfigurationBuilder;
                var intrf = kv.Value as IInterfaceConfigurationBuilder;
                
                if (cls != null)
                {
                    repository.AttributesForType[kv.Key] = cls.AttributePrototype;
                    repository.DecoratorsForType[kv.Key] = new List<TsDecoratorAttribute>(cls.Decorators);
                }

                if (intrf != null)
                    repository.AttributesForType[kv.Key] = intrf.AttributePrototype;
                

                foreach (var kvm in kv.Value.MembersConfiguration)
                {
                    if (kvm.Value.CheckIgnored())
                    {
                        repository.Ignored.Add(kvm.Key);
                        continue;
                    }
                    var prop = kvm.Key as PropertyInfo;
                    var field = kvm.Key as FieldInfo;
                    var method = kvm.Key as MethodInfo;
                    if (prop != null)
                    {
                        repository.AttributesForProperties[prop] = (TsPropertyAttribute) kvm.Value.AttributePrototype;
                    }
                    if (field != null)
                    {
                        repository.AttributesForFields[field] = (TsPropertyAttribute) kvm.Value.AttributePrototype;
                    }
                    if (method != null)
                    {
                        repository.AttributesForMethods[method] = (TsFunctionAttribute) kvm.Value.AttributePrototype;
                    }
                    var dec = kvm.Value as IDecoratorsAggregator;
                    if (dec != null)
                    {
                        if (!repository.DecoratorsForMember.ContainsKey(kvm.Key))
                        {
                            repository.DecoratorsForMember[kvm.Key] = new List<TsDecoratorAttribute>();
                        }
                        repository.DecoratorsForMember[kvm.Key].AddRange(dec.Decorators);
                    }
                }
                foreach (var kvp in kv.Value.ParametersConfiguration)
                {
                    if (kvp.Value.CheckIgnored())
                    {
                        repository.Ignored.Add(kvp.Key);
                        continue;
                    }
                    repository.AttributesForParameters[kvp.Key] = kvp.Value.AttributePrototype;
                }
                repository.AddFileSeparationSettings(kv.Key, kv.Value);
            }
            foreach (var kv in _enumConfigurationBuilders)
            {
                repository.AttributesForType[kv.Key] = kv.Value.AttributePrototype;
                foreach (var enumValueExportConfiguration in kv.Value.ValueExportConfigurations)
                {
                    repository.AttributesForEnumValues[enumValueExportConfiguration.Key] =
                        enumValueExportConfiguration.Value.AttributePrototype;
                }
                repository.AddFileSeparationSettings(kv.Key, kv.Value);
                repository.DecoratorsForType[kv.Key] = new List<TsDecoratorAttribute>(kv.Value.Decorators);
            }
            repository.References.AddRange(_references);
            repository.Imports.AddRange(_imports);

            repository.AdditionalDocumentationPathes.AddRange(_additionalDocumentationPathes);
            return repository;
        }
    }
}