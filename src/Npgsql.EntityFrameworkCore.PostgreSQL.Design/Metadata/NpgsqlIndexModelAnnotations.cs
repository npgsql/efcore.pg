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
    public class NpgsqlIndexModelAnnotations
    {
        private readonly IndexModel _index;

        public NpgsqlIndexModelAnnotations([NotNull] IndexModel index)
        {
            Check.NotNull(index, nameof(index));

            _index = index;
        }

        /// <summary>
        /// If the index contains an expression (rather than simple column references), the expression is contained here.
        /// This is currently unsupported and will be ignored.
        /// </summary>
        public string Expression
        {
            get { return _index[NpgsqlDatabaseModelAnnotationNames.Expression] as string; }
            [param: CanBeNull] set { _index[NpgsqlDatabaseModelAnnotationNames.Expression] = value; }
        }
    }
}
