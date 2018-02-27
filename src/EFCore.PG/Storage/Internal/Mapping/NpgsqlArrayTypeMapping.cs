#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlArrayTypeMapping : RelationalTypeMapping
    {
        public RelationalTypeMapping ElementMapping { get; }

        /// <summary>
        /// Creates the default array mapping (i.e. for the single-dimensional CLR array type)
        /// </summary>
        public NpgsqlArrayTypeMapping(string storeType, RelationalTypeMapping elementMapping)
            : this(storeType, elementMapping, elementMapping.ClrType.MakeArrayType())
        {}

        /// <summary>
        /// Creates the default array mapping (i.e. for the single-dimensional CLR array type)
        /// </summary>
        public NpgsqlArrayTypeMapping(RelationalTypeMapping elementMapping, Type arrayType)
            : this(elementMapping.StoreType + "[]", elementMapping, arrayType)
        {}

        NpgsqlArrayTypeMapping(string storeType, RelationalTypeMapping elementMapping, Type arrayType)
            : base(new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(arrayType, null, CreateComparer(elementMapping, arrayType)), storeType
            ))
        {
            ElementMapping = elementMapping;
        }

        protected NpgsqlArrayTypeMapping(RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping)
            : base(parameters) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlArrayTypeMapping(StoreType, ElementMapping);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlArrayTypeMapping(Parameters.WithComposedConverter(converter), ElementMapping);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            // Only support one-dimensional arrays (at least for now)
            var arr = (Array)value;

            if (arr.Rank != 1)
                throw new NotSupportedException("Multidimensional array literals aren't supported yet");

            var sb = new StringBuilder();
            sb.Append("ARRAY[");
            for (var i = 0; i < arr.Length; i++)
            {
                sb.Append(ElementMapping.GenerateSqlLiteral(arr.GetValue(i)));
                if (i < arr.Length - 1)
                    sb.Append(",");
            }
            sb.Append("]");
            return sb.ToString();
        }

        #region Value Comparison

        static ValueComparer CreateComparer(RelationalTypeMapping elementMapping, Type arrayType)
        {
            Debug.Assert(arrayType.IsArray);
            var elementType = arrayType.GetElementType();

            // In .NET, single-dimensional arrays implement IList<T> and can therefore be accessed generically
            // (i.e. efficiently). Multi-dimensional arrays don't, and can only be accessed non-generically via IList.

            if (!typeof(IList<>).MakeGenericType(elementType).IsAssignableFrom(arrayType))
                return null;   // TODO: Implement multi-dimensional array support (#314)

            // We use different comparer implementations based on whether we have a non-null element comparer,
            // and if not, whether the element is IEquatable<TElem>

            if (elementMapping.Comparer != null)
                return (ValueComparer)Activator.CreateInstance(
                    typeof(SingleDimComparerWithComparer<>).MakeGenericType(elementType), elementMapping);

            if (typeof(IEquatable<>).MakeGenericType(elementType).IsAssignableFrom(elementType))
                return (ValueComparer)Activator.CreateInstance(typeof(SingleDimComparerWithIEquatable<>).MakeGenericType(elementType));

            // There's no custom comparer, and the element type doesn't implement IEquatable<TElem>. We have
            // no choice but to use the non-generic Equals method.
            return (ValueComparer)Activator.CreateInstance(typeof(SingleDimComparerWithEquals<>).MakeGenericType(elementType));
        }

        class SingleDimComparerWithComparer<TElem> : ValueComparer<TElem[]>
        {
            public SingleDimComparerWithComparer(RelationalTypeMapping elementMapping) : base(
                (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                o => o.GetHashCode(), // TODO: Need to get hash code of elements...
                source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

            public override Type Type => typeof(TElem[]);

            static bool Compare(TElem[] a, TElem[] b, ValueComparer<TElem> elementComparer)
            {
                if (a.Length != b.Length)
                    return false;

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < a.Length; i++)
                    if (!elementComparer.Equals(a[i], b[i]))
                        return false;

                return true;
            }

            static TElem[] Snapshot(TElem[] source, ValueComparer<TElem> elementComparer)
            {
                if (source == null)
                    return null;

                var snapshot = new TElem[source.Length];
                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < source.Length; i++)
                    snapshot[i] = (TElem)elementComparer.Snapshot(source[i]);
                return snapshot;
            }
        }

        class SingleDimComparerWithIEquatable<TElem> : ValueComparer<TElem[]>
            where TElem : IEquatable<TElem>
        {
            public SingleDimComparerWithIEquatable(): base(
                (a, b) => Compare(a, b),
                o => o.GetHashCode(), // TODO: Need to get hash code of elements...
                source => DoSnapshot(source)) {}

            public override Type Type => typeof(TElem[]);

            static bool Compare(TElem[] a, TElem[] b)
            {
                if (a.Length != b.Length)
                    return false;

                for (var i = 0; i < a.Length; i++)
                {
                    var elem1 = a[i];
                    var elem2 = b[i];
                    // Note: the following null checks are elided if TElem is a value type
                    if (elem1 == null)
                    {
                        if (elem2 == null)
                            continue;
                        return false;
                    }
                    if (!elem1.Equals(elem2))
                        return false;
                }

                return true;
            }

            static TElem[] DoSnapshot(TElem[] source)
            {
                if (source == null)
                    return null;
                var snapshot = new TElem[source.Length];
                for (var i = 0; i < source.Length; i++)
                    snapshot[i] = source[i];
                return snapshot;
            }
        }

        class SingleDimComparerWithEquals<TElem> : ValueComparer<TElem[]>
        {
            public SingleDimComparerWithEquals() : base(
                (a, b) => Compare(a, b),
                o => o.GetHashCode(), // TODO: Need to get hash code of elements...
                source => DoSnapshot(source)) {}

            public override Type Type => typeof(TElem[]);

            static bool Compare(TElem[] a, TElem[] b)
            {
                if (a.Length != b.Length)
                    return false;

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < a.Length; i++)
                {
                    var elem1 = a[i];
                    var elem2 = b[i];
                    if (elem1 == null)
                    {
                        if (elem2 == null)
                            continue;
                        return false;
                    }
                    if (!elem1.Equals(elem2))
                        return false;
                }

                return true;
            }

            static TElem[] DoSnapshot(TElem[] source)
            {
                if (source == null)
                    return null;

                var snapshot = new TElem[source.Length];
                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < source.Length; i++)
                    snapshot[i] = source[i];
                return snapshot;
            }
        }

        #endregion Value Comparison
    }
}
