﻿using JetBrains.Annotations;
using Reusable.Flawless;

namespace Reusable.SmartConfig
{
    public class SqlServerColumnMapping
    {
        private static readonly IExpressValidator<string> ColumnValidator = ExpressValidator.For<string>(builder =>
        {
            builder.NotNull();
            builder.False(c => string.IsNullOrEmpty(c));
        });

        private string _name;
        private string _value;

        public SqlServerColumnMapping()
        {
            Name = nameof(Name);
            Value = nameof(Value);
        }

        [NotNull]
        public string Name
        {
            get => _name;
            set => _name = value.ValidateWith(ColumnValidator).Assert();
        }

        [NotNull]
        public string Value
        {
            get => _value;
            set => _value = value.ValidateWith(ColumnValidator).Assert();
        }

        public static implicit operator SqlServerColumnMapping((string name, string value) mapping)
        {
            return new SqlServerColumnMapping
            {
                Name = mapping.name,
                Value = mapping.value
            };
        }
    }
}