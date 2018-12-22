using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure
{
    /// <summary>
    /// The validator that enforces rules for Npgsql.
    /// </summary>
    public class NpgsqlModelValidator : RelationalModelValidator
    {
        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull] readonly Version _postgresVersion;

        /// <inheritdoc />
        public NpgsqlModelValidator(
            [NotNull] ModelValidatorDependencies dependencies,
            [NotNull] RelationalModelValidatorDependencies relationalDependencies,
            [NotNull] INpgsqlOptions npgsqlOptions)
            : base(dependencies, relationalDependencies)
            => _postgresVersion = Check.NotNull(npgsqlOptions, nameof(npgsqlOptions)).PostgresVersion;

        public override void Validate(IModel model)
        {
            base.Validate(model);
            ValidateIdentityVersionCompatibility(model);
        }

        #region Npgsql-specific validations

        /// <summary>
        /// Validates that identity columns are used only with PostgreSQL 10.0 or later.
        /// </summary>
        /// <param name="model">The model to validate.</param>
        protected virtual void ValidateIdentityVersionCompatibility([NotNull] IModel model)
        {
            if (VersionAtLeast(10, 0))
                return;

            var strategy = model.Npgsql().ValueGenerationStrategy;

            if (strategy == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn ||
                strategy == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
            {
                throw new InvalidOperationException($"'{strategy}' requires PostgreSQL 10.0 or later.");
            }

            foreach (var property in model.GetEntityTypes().SelectMany(e => e.GetProperties()))
            {
                var propertyStrategy = property.Npgsql().ValueGenerationStrategy;

                if (propertyStrategy == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn ||
                    propertyStrategy == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                {
                    throw new InvalidOperationException(
                        $"{property.DeclaringEntityType}.{property.Name}: '{propertyStrategy}' requires PostgreSQL 10.0 or later.");
                }
            }
        }

        #endregion

        #region Helpers

        bool VersionAtLeast(int major, int minor) => _postgresVersion is null || new Version(major, minor) <= _postgresVersion;

        #endregion
    }
}
