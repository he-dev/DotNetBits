﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.IOnymous;

namespace Reusable.SmartConfig.Annotations
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class SettingProviderAttribute : Attribute
    {
        private string _prefix;
        private SettingNameStrength _strength;
        private readonly IImmutableSet<Type> _providerTypes;
        private readonly IImmutableSet<SoftString> _providerNames;

        private SettingProviderAttribute
        (
            SettingNameStrength strength,
            IEnumerable<Type> providerTypes,
            IEnumerable<SoftString> providerNames
        )
        {
            _providerTypes = providerTypes.ToImmutableHashSet();
            _providerNames = providerNames.ToImmutableHashSet();
            _strength = strength;
        }

        public SettingProviderAttribute(SettingNameStrength strength, params Type[] providerTypes)
            : this(strength, providerTypes, Enumerable.Empty<SoftString>())
        { }

        public SettingProviderAttribute(SettingNameStrength strength, params string[] providerNames)
            : this(strength, Enumerable.Empty<Type>(), providerNames.Select(SoftString.Create))
        { }

        public SettingProviderAttribute(SettingNameStrength strength)
            : this(strength, Enumerable.Empty<Type>(), Enumerable.Empty<SoftString>())
        { }

        public static readonly SettingProviderAttribute Default = new SettingProviderAttribute
        (
            SettingNameStrength.Medium,
            Enumerable.Empty<Type>(),
            Enumerable.Empty<SoftString>()
        );

        [CanBeNull]
        public Type AssemblyType { get; set; }

        //public Type FormatterType { get; set; }

        [CanBeNull]
        public string Prefix
        {
            get => _prefix ?? (AssemblyType is null ? default : Assembly.GetAssembly(AssemblyType).GetName().Name);
            set => _prefix = value;
        }

        // todo - disallow .Inherit
        public SettingNameStrength Strength
        {
            get => _strength;
            //set => _settingNameStrength = value;
        }

        public bool Matches<T>(T provider) where T : IResourceProvider
        {
            var matchesAny = provider == null || (_providerNames.Empty() && _providerTypes.Empty());

            return
                matchesAny ||
                _providerTypes.Contains(provider.GetType()) ||
                (
                    provider.Metadata.TryGetValue(ResourceProviderMetadataKeyNames.ProviderName, out var name) &&
                    _providerNames.Contains(name)
                );
        }
    }

    [UsedImplicitly]
    public class SettingAttribute : Attribute
    {
        [CanBeNull] private string _prefix;
        private PrefixHandling _prefixHandling = PrefixHandling.Inherit;

        internal SettingAttribute()
        {
        }

        [CanBeNull]
        public string Prefix
        {
            get => _prefix;
            set
            {
                _prefix = value;
                _prefixHandling = _prefix.IsNullOrEmpty() ? PrefixHandling.Inherit : PrefixHandling.Enable;
            }
        }

        [CanBeNull]
        public string Name { get; set; }

        [CanBeNull]
        public string ProviderName { get; set; }

        [CanBeNull]
        public Type ProviderType { get; set; }

        public SettingNameStrength Strength { get; set; } = SettingNameStrength.Inherit;

        public PrefixHandling PrefixHandling
        {
            get => _prefixHandling;
            set
            {
                _prefixHandling = value;
                if (_prefixHandling == PrefixHandling.Disable)
                {
                    _prefix = default;
                }
            }
        }

        // todo for future use
        //public bool Cache { get; set; } = true;
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingTypeAttribute : SettingAttribute
    {
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SettingMemberAttribute : SettingAttribute
    {
    }

    public enum PrefixHandling
    {
        Inherit = -1,
        Disable = 0,
        Enable = 1,
    }

    //public enum ProviderSearch
    //{
    //    /// <summary>
    //    /// Uses the specified provider, otherwise picks the first setting.
    //    /// </summary>
    //    Auto,

    //    /// <summary>
    //    /// Ignores any provider name and picks the first setting.
    //    /// </summary>
    //    FirstMatch,
    //}
}