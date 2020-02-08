using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The type mapping for the PostgreSQL bit string type.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/datatype-bit.html
    /// </remarks>
    public class NpgsqlBitTypeMapping : NpgsqlTypeMapping
    {
        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlBitTypeMapping"/> class.
        /// </summary>
        public NpgsqlBitTypeMapping() : base("bit", typeof(BitArray), NpgsqlDbType.Bit) {}

        protected NpgsqlBitTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Bit) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlBitTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var bits = (BitArray)value;
            var sb = new StringBuilder();
            sb.Append("B'");
            for (var i = 0; i < bits.Count; i++)
                sb.Append(bits[i] ? '1' : '0');
            sb.Append('\'');
            return sb.ToString();
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var bits = (BitArray)value;
            var exprs = new Expression[bits.Count];
            for (var i = 0; i < bits.Count; i++)
                exprs[i] = Expression.Constant(bits[i]);
            return Expression.New(Constructor, Expression.NewArrayInit(typeof(bool), exprs));
        }

        static readonly ConstructorInfo Constructor =
            typeof(BitArray).GetConstructor(new[] { typeof(bool[]) });
    }
}
