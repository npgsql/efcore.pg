// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class NpgsqlFullAnnotationNames : RelationalFullAnnotationNames
    {
        protected NpgsqlFullAnnotationNames(string prefix)
            : base(prefix)
        {
            Serial = prefix + NpgsqlAnnotationNames.Serial;
            DefaultSequenceName = prefix + NpgsqlAnnotationNames.DefaultSequenceName;
            DefaultSequenceSchema = prefix + NpgsqlAnnotationNames.DefaultSequenceSchema;
            SequenceName = prefix + NpgsqlAnnotationNames.SequenceName;
            SequenceSchema = prefix + NpgsqlAnnotationNames.SequenceSchema;
            IndexMethod = prefix + NpgsqlAnnotationNames.IndexMethod;
        }

        public new static NpgsqlFullAnnotationNames Instance { get; } = new NpgsqlFullAnnotationNames(NpgsqlAnnotationNames.Prefix);

        public readonly string Serial;
        public readonly string DefaultSequenceName;
        public readonly string DefaultSequenceSchema;
        public readonly string SequenceName;
        public readonly string SequenceSchema;
        public readonly string IndexMethod;
    }
}
