﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.Quickey;
using Reusable.OneTo1;

namespace Reusable.Commander
{
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class CommandArgumentMetadata
    {
        private CommandArgumentMetadata(PropertyInfo member)
        {
            Property = member;
            Position = member.GetCustomAttribute<PositionAttribute>()?.Value;
            Id =
                Position.HasValue
                    ? Identifier.FromPosition(Position.Value)
                    : CommandHelper.GetCommandParameterId(member);
            Description = member.GetCustomAttribute<DescriptionAttribute>()?.Description;
            DefaultValue = member.GetCustomAttribute<DefaultValueAttribute>()?.Value;
            Required = member.IsDefined(typeof(RequiredAttribute)) || Id.Default.Option.Contains(NameOption.Positional);
            ConverterType = member.GetCustomAttribute<Reusable.OneTo1.TypeConverterAttribute>()?.ConverterType;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.DisplayValue(x => x.Id.Default.ToString());
            b.DisplayValue(x => x.Property.PropertyType.ToPrettyString(false));
            b.DisplayValue(x => x.Position);
        });

        [NotNull]
        public PropertyInfo Property { get; }

        [NotNull]
        public Identifier Id { get; }

        [CanBeNull]
        public string Description { get; }

        [CanBeNull]
        public int? Position { get; }

        [CanBeNull]
        public object DefaultValue { get; }

        public bool Required { get; }
        
        public Type ConverterType { get; }

        [NotNull]
        public static CommandArgumentMetadata Create([NotNull] PropertyInfo property)
        {
            return new CommandArgumentMetadata(property ?? throw new ArgumentNullException(nameof(property)));
        }

        //public void SetValue(object obj, object value) => Property.SetValue(obj, value);
    }
}