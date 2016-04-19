using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql;

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
    }
}
