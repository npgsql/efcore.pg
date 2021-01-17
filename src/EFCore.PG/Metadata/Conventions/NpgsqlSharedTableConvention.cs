using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions
{
    /// <summary>
    ///     A convention that manipulates names of database objects for entity types that share a table to avoid clashes.
    /// </summary>
    public class NpgsqlSharedTableConvention : SharedTableConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="NpgsqlSharedTableConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public NpgsqlSharedTableConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <inheritdoc />
        protected override bool AreCompatible(IIndex index, IIndex duplicateIndex, in StoreObjectIdentifier storeObject)
            => base.AreCompatible(index, duplicateIndex, storeObject)
               && index.AreCompatibleForNpgsql(duplicateIndex, storeObject, shouldThrow: false);
    }
}
