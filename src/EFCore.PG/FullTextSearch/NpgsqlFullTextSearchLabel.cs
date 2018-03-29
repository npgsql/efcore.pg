// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public readonly struct NpgsqlFullTextSearchLabel
    {
        public static readonly NpgsqlFullTextSearchLabel A = 'A';
        public static readonly NpgsqlFullTextSearchLabel B = 'B';
        public static readonly NpgsqlFullTextSearchLabel C = 'C';
        public static readonly NpgsqlFullTextSearchLabel D = 'D';
        public static readonly NpgsqlFullTextSearchLabel Default = D;

        public char Label { get; }

        public NpgsqlFullTextSearchLabel(char label)
        {
            Label = label;
        }

        public static implicit operator NpgsqlFullTextSearchLabel(char label) => new NpgsqlFullTextSearchLabel(label);
        public static implicit operator char(NpgsqlFullTextSearchLabel label) => label.Label;
    }
}
