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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class NpgsqlColumnModelAnnotations
    {
        private readonly ColumnModel _column;

        public NpgsqlColumnModelAnnotations([NotNull] ColumnModel column)
        {
            Check.NotNull(column, nameof(column));

            _column = column;
        }

        /// <summary>
        /// Identifies PostgreSQL serial columns.
        /// These columns are configured with a default value coming from a specifically-named sequence.
        /// </summary>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/datatype-numeric.html
        /// </remarks>
        public bool IsSerial
        {
            get
            {
                var value = _column[NpgsqlDatabaseModelAnnotationNames.IsSerial];
                return value is bool && (bool)value;
            }
            [param: CanBeNull]
            set { _column[NpgsqlDatabaseModelAnnotationNames.IsSerial] = value; }
        }

        /// <summary>
        /// Identifies the type of the PostgreSQL type of this column (e.g. array, range, base).
        /// </summary>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/datatype-numeric.html
        /// </remarks>
        internal PostgresTypeType PostgresTypeType
        {
            get
            {
                var value = _column[NpgsqlDatabaseModelAnnotationNames.PostgresTypeType];
                return (PostgresTypeType?)value ?? PostgresTypeType.Base;
            }
            [param: CanBeNull]
            set { _column[NpgsqlDatabaseModelAnnotationNames.PostgresTypeType] = value; }
        }

        /// <summary>
        /// If this column's data type is an array, contains the data type of its elements.
        /// Otherwise null.
        /// </summary>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/datatype-numeric.html
        /// </remarks>
        internal string ElementDataType
        {
            get
            {
                return (string)_column[NpgsqlDatabaseModelAnnotationNames.ElementDataType];
            }
            [param: CanBeNull]
            set { _column[NpgsqlDatabaseModelAnnotationNames.ElementDataType] = value; }
        }
    }
}
